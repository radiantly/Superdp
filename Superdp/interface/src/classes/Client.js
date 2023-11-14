import { shallowReactive, computed, nextTick, reactive } from "vue";
import { watchIgnorable } from "@vueuse/core";
import { v4 as uuidv4 } from "uuid";
import { Tab } from "./Tab";
import { ClientEntry } from "./ClientEntry";

export class Client {
  #manager;
  #entry = null;
  constructor({ id, props, manager } = {}) {
    this.id = id || uuidv4();
    this.#manager = manager;

    this.tabs = new Map();
    this.props = shallowReactive({});

    // feedback on the entered properties
    this.hints = reactive({
      host: computed(() =>
        /[^a-z0-9.-]/i.test(this.props.host) ? "Invalid hostname" : null
      ),
      username: computed(() =>
        this.props.type === "rdp" && this.props.username.includes("\\")
          ? "Domain: " + this.props.username.split("\\")[0]
          : null
      ),
    });

    this.watcher = watchIgnorable(this.props, () => this.#manager.save(this));

    this.#populateProps(props);
    this.label = computed(
      () => this.props.name || this.props.host || "Untitled connection"
    );
  }

  get entry() {
    if (this.#entry) return this.#entry;
    return (this.#entry = new ClientEntry({ client: this }));
  }

  #populateProps({
    type = "rdp",
    host = "",
    name = "",
    username = "",
    password = "",
  } = {}) {
    this.watcher.ignoreUpdates(() => {
      this.props.type = type;
      this.props.host = host;
      this.props.name = name;
      this.props.username = username;
      this.props.password = password;
    });
  }

  // Creates tab if it doesn't exist. Otherwise switches to tab
  async createTab(tabManager) {
    // If the client is an rdp client, then we restrict to a single tab
    // and switch to the existing rdp tab if it exists
    let tab = null;
    if (this.props.type === "rdp") {
      tab = [...this.tabs.values()].find((tab) => tab.props.type === "rdp");

      // If tab is already part of an existing tab manager, we reparent it
      if (tab?.props.parent) tab.props.parent.remove(tab);
    }

    if (!tab) {
      tab = new Tab({ client: this });
      this.tabs.set(tab.id, tab);
    }

    tabManager.add(tab);
    tabManager.setActive(tab);

    // So that the dimensions are right before we send a connection request
    await nextTick();
    tab.connect();

    return tab;
  }

  async recreateExistingTab(tabManager, tab) {
    // Sanity check: Client must not already be associated to a visible tab
    console.assert((this.tab?.props.parent ?? null) === null);

    console.assert(tab instanceof Tab);
    this.tab = tab;

    tabManager.add(this.tab);
    tabManager.setActive(this.tab);

    return this.tab;
  }

  processMessage(msg) {
    if (msg.tabId) return this.tabs.get(msg.tabId)?.processMessage(msg);

    switch (msg.type) {
      default:
        console.assert(false, "Unknown message type");
    }
  }

  serialize() {
    return {
      id: this.id,
      props: { ...this.props },
    };
  }

  reconcile({ props }) {
    this.#populateProps(props);
  }
}
