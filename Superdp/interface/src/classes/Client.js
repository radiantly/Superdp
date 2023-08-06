import { shallowReactive, computed } from "vue";
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

    this.props = shallowReactive({});

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

  #populateProps({ host = "", name = "", username = "", password = "" } = {}) {
    this.watcher.ignoreUpdates(() => {
      this.props.host = host;
      this.props.name = name;
      this.props.username = username;
      this.props.password = password;
    });
  }

  // Creates tab if it doesn't exist. Otherwise switches to tab
  createOrSwitchToTab(tabManager) {
    if (!this.tab) this.tab = new Tab({ client: this });
    tabManager.add(this.tab);
    tabManager.setActive(this.tab);

    this.tab.connect();
    return this.tab;
  }

  processMessage(msg) {
    switch (msg.type) {
      case "RDP_LOG":
        this.tab.processRDPLog(msg);
        break;
      case "RDP_NEWOWNER":
        this.tab.giveupOwnership();
        break;
    }
  }

  serialize() {
    return {
      id: this.id,
      props: this.props,
    };
  }

  reconcile({ props }) {
    this.#populateProps(props);
  }
}
