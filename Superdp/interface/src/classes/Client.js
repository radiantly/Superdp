import { shallowReactive, computed, reactive } from "vue";
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

    this.invalidFields = reactive({
      host: computed(
        () =>
          this.props.host.length === 0 || /[^a-z0-9.-]/i.test(this.props.host)
      ),
      username: computed(() => this.props.username.includes(" ")),
      type: computed(() => this.props.type === null),
    });

    this.valid = computed(
      () => Object.values(this.invalidFields).filter(Boolean).length === 0
    );

    // feedback on the entered properties
    this.hints = reactive({
      host: computed(() => {
        if (this.props.host.length === 0) return "(required)";
        if (this.invalidFields.host) return "Invalid hostname";
        return null;
      }),
      username: computed(() => {
        if (this.invalidFields.username) return "No spaces allowed";
        const split = this.props.username.split("\\");
        if (split.length >= 2) return `Domain: ${split[0]}`;
        return null;
      }),
      type: computed(() => (this.invalidFields.type ? "Please select" : null)),
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
    type = null,
    host = "",
    name = "",
    username = "",
    password = "",
    key = "",
  } = {}) {
    this.watcher.ignoreUpdates(() => {
      this.props.type = type;
      this.props.host = host;
      this.props.name = name;
      this.props.username = username;
      this.props.password = password;
      this.props.key = key;
    });
  }

  // Creates tab if it doesn't exist. Otherwise switches to tab
  async createTab(tabManager) {
    const tab = new Tab({ client: this });

    this.addTab(tabManager, tab);
    tab.connect();

    return tab;
  }

  async addTab(tabManager, tab) {
    this.tabs.set(tab.id, tab);
    tabManager.add(tab);
    tabManager.setActive(tab);
  }

  processMessage(msg) {
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
