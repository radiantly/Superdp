import { useDebounceFn } from "@vueuse/core";
import { ChangeManager } from "./ChangeManager";
import { Client } from "./Client";
import { DirEntry } from "./DirEntry";
import { broadcastChannel, interopQueen } from "../globals";
import { Tab } from "./Tab";
import { SessionManager } from "./SessionManager";
import { broadcast } from "../utils";

export class ClientManager {
  #debouncedWriteConfig;
  constructor(conf) {
    this.changes = new ChangeManager();
    this.idToClient = new Map();
    this.dirEntries = new Map();
    this.session = new SessionManager();

    // Populate clients
    this.reconcile(conf);
    console.debug("ReadConf parsed:", conf);

    if (!this.root) {
      this.root = new DirEntry({ manager: this });
      this.#writeConfig();
    }

    this.#debouncedWriteConfig = useDebounceFn(() => this.#writeConfig(), 5000);

    chrome.webview.addEventListener("message", (msg) => {
      console.debug("> wv2message", msg);
      this.processMessage(msg);
    });
    broadcastChannel.addEventListener("message", (msg) => {
      console.debug("> broadcast", msg);
      this.processBroadcastMessage(msg);
    });

    window.chrome.webview.addEventListener("sharedbufferreceived", (e) => {
      console.debug("> wv2buffer", e);
      window.ev = e;

      if (e.additionalData?.tabId)
        return this.session
          .getTab(e.additionalData.tabId)
          ?.processSharedBuffer(e);
    });
  }

  processBroadcastMessage({ data }) {
    const response = this.processMessage({ data });
    if (data.replyTo) new BroadcastChannel(data.replyTo).postMessage(response);
  }

  processMessage({ data }) {
    if (data.tabId)
      return this.session.getTab(data.tabId)?.processMessage(data);

    if (data.clientId)
      return this.idToClient.get(data.clientId)?.processMessage(data);

    switch (data.type) {
      case "RECONCILE":
        this.reconcile(data.changes);
        break;

      case "TAB_TRANSFER_REQUEST":
        broadcast({ type: data.type, tabId: data.transferTabId }, true).then(
          async ({
            id,
            client: serializedClient,
            logs: serializedLogs,
            props,
          }) => {
            this.reconcile({ clients: [serializedClient] });
            const client = this.get(serializedClient.id);
            const tabManager = this.session.children[0];
            const tab = new Tab({ id, client, props, serializedLogs });
            tab.update();
            client.addTab(tabManager, tab);
          }
        );
        break;

      default:
        console.assert(false, "Unknown message type");
    }
  }

  reconcile({ clients = [], dir_entries = [] }) {
    // Clients
    for (const { id, props } of clients) {
      if (this.idToClient.has(id)) this.idToClient.get(id).reconcile({ props });
      else this.idToClient.set(id, new Client({ id, props, manager: this }));
    }

    // Dir Entries
    for (const { id, props, root } of dir_entries) {
      if (this.dirEntries.has(id)) continue;
      const entry = new DirEntry({ id, props, manager: this });
      if (root) this.root = entry;
    }

    for (const { id, props, children } of dir_entries) {
      this.dirEntries.get(id).reconcileStep1({ props, children });
    }

    for (const { id, children } of dir_entries) {
      this.dirEntries.get(id).reconcileStep2({ children });
    }
  }

  /**
   * Creates a new client and adds an associated entry to the dir tree
   * @param {{parentEntry: DirEntry, clientProps: Object}} options client configuration options
   * @returns {Client} the newly created client and associated entry
   */
  createClient({ parentEntry = this.root, clientProps = {} } = {}) {
    const client = new Client({
      props: clientProps,
      manager: this,
    });
    parentEntry.addChild(client.entry);
    this.idToClient.set(client.id, client);
    this.save(client);
    return client;
  }

  get(clientId) {
    return this.idToClient.get(clientId);
  }

  getEntry(entryId) {
    return this.dirEntries.get(entryId) || this.get(entryId)?.entry;
  }

  save(obj) {
    this.changes.add(obj);
    this.#debouncedWriteConfig();
  }

  async #writeConfig() {
    const clients = [...this.idToClient.values()].map((client) =>
      client.serialize()
    );
    const dir_entries = this.root.serializeTree();
    const confStr = JSON.stringify({ clients, dir_entries });
    await interopQueen.WriteConf(confStr);
  }
}
