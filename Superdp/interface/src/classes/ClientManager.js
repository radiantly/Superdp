import { v4 as uuidv4 } from "uuid";
import { shallowReactive, shallowReadonly } from "vue";

import { ChangeManager } from "./ChangeManager";
import { Client } from "./Client";
import { DirEntry } from "./DirEntry";
import { broadcastChannel, windowIsMaximized } from "../globals";
import { Tab } from "./Tab";
import { SessionManager } from "./SessionManager";
import { broadcast, broadcastMessageLog, postMessageTo } from "../utils";
import { Entry } from "./Entry";

export class ClientManager {
  #id;
  #changes;
  #session;
  #clientMap;
  #clientMapReadonly;
  #dirEntries;
  constructor(conf) {
    this.#id = uuidv4();

    this.#changes = new ChangeManager();
    this.#session = new SessionManager();

    this.#clientMap = shallowReactive(new Map());
    this.#clientMapReadonly = shallowReadonly(this.#clientMap);
    this.#dirEntries = new Map();

    // Populate clients
    this.reconcile(conf);
    console.debug("ReadConfig:", conf);

    if (!this.root) {
      this.root = this.createDirEntry(null);
      this.save(this.root);
    }

    chrome.webview.addEventListener("message", (msg) => {
      console.debug("> wv2message", msg);
      this.processMessage(msg);
    });
    broadcastChannel.addEventListener(
      "message",
      broadcastMessageLog((msg) => this.processBroadcastMessage(msg))
    );
    new BroadcastChannel(this.#id).addEventListener(
      "message",
      broadcastMessageLog((msg) => this.processBroadcastMessage(msg))
    );

    window.chrome.webview.addEventListener("sharedbufferreceived", (e) => {
      console.debug("> wv2buffer", e);
      window.ev = e;

      if (e.additionalData?.tabId)
        return this.#session
          .getTab(e.additionalData.tabId)
          ?.processSharedBuffer(e);
    });
  }

  get id() {
    return this.#id;
  }

  get session() {
    return this.#session;
  }

  get clients() {
    return this.#clientMapReadonly;
  }

  processBroadcastMessage({ data }) {
    const response = this.processMessage({ data });
    if (data.replyTo) broadcast(response, new BroadcastChannel(data.replyTo));
  }

  processMessage({ data }) {
    if (data.tabId)
      return this.#session.getTab(data.tabId)?.processMessage(data);

    if (data.clientId)
      return this.#clientMap.get(data.clientId)?.processMessage(data);

    switch (data.type) {
      case "FORMBORDERSTYLE_CHANGE":
        windowIsMaximized.value = data.formBorderStyle == 0;
        break;

      case "RECONCILE":
        this.reconcile(data.changes);
        break;

      /**
       * Let's say I want to move a tab from one window to another. How does
       * it work?
       * When the user starts to drag a tab, we take note of this. Then, when
       * the user leaves the window dragging the tab, we call
       * interopQueen.CreateNewDraggedWindow() with the tab id.
       * This then creates a new window and sends a TAB_TRANSFER_REQUEST that
       * gets handled below.
       * The code below then uses a broadcast channel to send the same
       * request, but this time specifically to the tab (find the handler in
       * Tab.js)
       * That handler then returns a serialized version of the tab that
       * contains the various properties that can be used to create the tab.
       *
       * Now moving this tab with the newly created window does basically the
       * same thing, except that we set a CloseOnTransfer flag on the new form
       * and when the rdp/ssh backend transfers, it closes.
       * TODO: Change this so that it is not dependent on the specific
       * connection backend
       */
      case "TAB_TRANSFER_REQUEST":
        postMessageTo(
          { type: data.type, tabId: data.transferTabId },
          data.originatingFormId,
          true
        ).then(
          async ({
            id,
            client: serializedClient,
            logs: serializedLogs,
            props,
          }) => {
            this.reconcile({ clients: [serializedClient] });
            const client = this.get(serializedClient.id);
            const tabManager = this.#session.children[0];
            const tab = new Tab({ id, client, props, serializedLogs });
            tab.transfer();
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
      if (this.#clientMap.has(id)) this.#clientMap.get(id).reconcile({ props });
      else this.#clientMap.set(id, new Client({ id, props, manager: this }));
    }

    // Dir Entries
    for (const { id, props, root } of dir_entries) {
      if (this.#dirEntries.has(id)) continue;
      const entry = this.createDirEntry(null, { id, props });
      if (root) this.root = entry;
    }

    for (const { id, props, children } of dir_entries) {
      this.#dirEntries.get(id).reconcileStep1({ props, children });
    }

    for (const { id, children } of dir_entries) {
      this.#dirEntries.get(id).reconcileStep2({ children });
    }
  }

  /**
   * Creates a new client and adds an associated entry to the dir tree
   * @param {{parentEntry: DirEntry, clientProps: Object}} options client configuration options
   * @returns {Client} the newly created client and associated entry
   */
  createClient({ parentEntry = this.root, clientProps = {} } = {}) {
    const client = this.createFloatingClient(clientProps);
    parentEntry.addChild(client.entry);
    this.save(client);
    return client;
  }

  createFloatingClient(clientProps) {
    const client = new Client({
      props: clientProps,
      manager: this,
    });
    this.#clientMap.set(client.id, client);
    return client;
  }

  createDirEntry(parentEntry = this.root, { id, props } = {}) {
    const dirEntry = new DirEntry({ id, props, manager: this });
    parentEntry?.addChild(dirEntry);
    this.#dirEntries.set(dirEntry.id, dirEntry);
    return dirEntry;
  }

  get(clientId) {
    return this.#clientMap.get(clientId);
  }

  getEntry(entryId) {
    return this.#dirEntries.get(entryId) || this.get(entryId)?.entry;
  }

  save(obj) {
    this.#changes.add(obj);
  }

  /**
   * Check if an entry is actually part of the root tree
   * If it isn't, it could be a deleted or a temp entry
   * @param {Entry} entry
   * @returns bool
   */
  isSaved(entry) {
    return this.root == entry.root;
  }
}
