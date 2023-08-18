import { computed, shallowReactive, shallowRef, watch } from "vue";
import { watchDebounced } from "@vueuse/core";
import { interopQueen } from "../globals";
import { Client } from "./Client";

export class Tab {
  constructor({ client, props = {}, serializedLogs = [] }) {
    this.client = client;
    console.assert(this.client instanceof Client);

    this.isActive = computed(() => this.props.parent?.props.active === this);
    this.props = shallowReactive({
      rdpControlVisible: false,
      connectInProgress: false,

      // A tab from a different window could possibly own the rdp control
      ownsRDPControl: true,

      ...props,
      parent: null,
    });

    this.logs = shallowReactive(
      serializedLogs.map(({ timestamp, content }) => ({
        date: new Date(timestamp),
        content,
      }))
    );

    const watchHandler = () => {
      if (!this.props.ownsRDPControl || !this.props.parent) return;
      this.#setRDPControlCharacteristics();
    };

    // Set rdp control visibility based on whether tab is active
    watch(this.isActive, watchHandler);

    // rdp control resize
    watchDebounced(() => this.props.parent?.props.workAreaSize, watchHandler, {
      debounce: 222,
      maxWait: 555,
    });
  }

  get id() {
    return this.client.id;
  }

  get #serializedLogs() {
    return this.logs.map(({ date, content }) => ({
      timestamp: +date,
      content,
    }));
  }

  async #setRDPControlCharacteristics(
    { x, y, width, height, shouldBeVisible } = {
      ...this.props.parent?.props.workAreaSize,
      shouldBeVisible: this.isActive.value,
    }
  ) {
    await interopQueen.RDPSetCharacteristics(
      this.id,
      x,
      y,
      width,
      height,
      shouldBeVisible
    );
  }

  async connect() {
    this.ownsRDPControl = true;
    this.props.connectInProgress = true;
    this.#setRDPControlCharacteristics();
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

  serialize() {
    return {
      client: this.client.serialize(),
      logs: this.#serializedLogs,
      props: {
        rdpControlVisible: this.props.rdpControlVisible,
        connectInProgress: this.props.connectInProgress,
        ownsRDPControl: this.props.ownsRDPControl,
      },
    };
  }

  serializeMsg() {
    return {
      type: "TAB_ADD",
      tab: this.serialize(),
    };
  }
}
