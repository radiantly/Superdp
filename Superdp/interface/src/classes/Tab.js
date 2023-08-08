import { computed, shallowReactive, shallowRef, watch } from "vue";
import { watchDebounced } from "@vueuse/core";
import { interopQueen } from "../globals";

export class Tab {
  constructor({ client }) {
    this.client = client;

    this.isActive = computed(() => this.props.parent?.props.active === this);
    this.props = shallowReactive({
      rdpControlVisible: false,
      parent: null,
      connectInProgress: false,

      // A tab from a different window could possibly own the rdp control
      ownsRDPControl: true,
    });
    this.logs = shallowReactive([]);

    // Set rdp control visibility based on whether tab is active
    watch(this.isActive, (isActive) => {
      if (!this.props.ownsRDPControl) return;
      console.debug("Active tab watcher fire", this);
      if (isActive) this.#setRDPSize(this.props.parent.props.workAreaSize);
      this.#setRDPVisibility(isActive);
    });

    // rdp control resize
    watchDebounced(
      () => this.props.parent?.props.workAreaSize,
      (size) => {
        if (!this.isActive.value || !this.props.ownsRDPControl) return;
        console.log("Size update", size);
        this.#setRDPSize(size);
      },
      { debounce: 222, maxWait: 555 }
    );
  }

  get id() {
    return this.client.id;
  }

  async #setRDPSize({ width, height }) {
    await interopQueen.RDPSetSize(this.id, width, height);
  }

  async #setRDPVisibility(isVisible) {
    await interopQueen.RDPSetVisibility(this.id, isVisible);
  }

  async connect() {
    this.ownsRDPControl = true;
    this.props.connectInProgress = true;
    await this.#setRDPSize(this.props.parent.props.workAreaSize);
    await interopQueen.RDPConnect(
      this.id,
      this.client.props.host,
      this.client.props.username,
      this.client.props.password
    );
  }

  async disconnect() {
    if (!this.ownsRDPControl) return;
    await interopQueen.RDPDisconnect(this.id);
  }

  log(text) {
    this.logs.push({ date: new Date(), content: text });
  }

  processRDPLog({ content, visibility, event }) {
    this.props.rdpControlVisible = visibility;
    if (event) this.props.connectInProgress = false;
    this.log(content);
  }

  giveupOwnership() {
    this.props.ownsRDPControl = false;
    this.props.parent?.remove(this);
  }
}
