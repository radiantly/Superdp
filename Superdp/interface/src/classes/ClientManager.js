import { useDebounceFn } from "@vueuse/core";
import { ChangeManager } from "./ChangeManager";
import { Client } from "./Client";
import { DirEntry } from "./DirEntry";
import { interopQueen } from "../globals";
import { Tab } from "./Tab";

export class ClientManager {
  #debouncedWriteConfig;
  constructor(conf) {
    this.changes = new ChangeManager();
    this.idToClient = new Map();
    this.dirEntries = new Map();
    this.tabManagers = new Set();

    // Populate clients
    this.reconcile(conf);
    console.debug("ReadConf parsed:", conf);

    if (!this.root) {
      this.root = new DirEntry({ manager: this });
      this.#writeConfig();
    }
    interopQueen();
    this.#debouncedWriteConfig = useDebounceFn(() => this.#writeConfig(), 5000);
  }

  processMessage({ data }) {
    console.debug("RECV", data);
    if (data.clientId)
      return this.idToClient.get(data.clientId)?.processMessage(data);

    switch (data.type) {
      case "RECONCILE":
        this.reconcile(data.changes);
        break;
      case "TAB_ADD":
        this.reconcile({ clients: [data.tab.client] });

        // TODO: make this nicer
        const tabManager = this.tabManagers.keys().next().value;
        const client = this.get(data.tab.client.id);
        const tab = new Tab({
          client,
          props: data.tab.props,
          serializedLogs: data.tab.logs,
        });
        client.recreateExistingTab(tabManager, tab);
        break;
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
