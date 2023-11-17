import { computed, shallowReactive, shallowRef, watch } from "vue";
import { watchDebounced } from "@vueuse/core";
import { interopQueen } from "../globals";
import { Client } from "./Client";
import { v4 as uuidv4 } from "uuid";

export class Tab {
  constructor({
    id = uuidv4(),
    client,
    props: { type = null, state = "disconnected" } = {},
    serializedLogs = [],
  }) {
    this.id = id;

    this.client = client;
    console.assert(this.client instanceof Client);

    this.isActive = computed(() => this.props.parent?.props.active === this);
    this.props = shallowReactive({
      type,
      state,
      buffer: null,
      parent: null,
    });

    this.logs = shallowReactive(
      serializedLogs.map(({ timestamp, content }) => ({
        date: new Date(timestamp),
        content,
      }))
    );

    this.dimensions = computed(() => {
      if (this.props.type === "rdp")
        return this.props.parent?.props.workAreaSize;

      if (this.props.type === "ssh")
        return { rows: this.props.rows, cols: this.props.cols };

      return {};
    });

    this.connectionOptions = computed(() => ({
      clientId: this.client.id,
      tabId: this.id,
      type: this.props.type,

      client: {
        ...this.client.props,
      },

      ...this.dimensions.value,

      visible: this.isActive.value,
    }));

    const debounceParams = { debounce: 222, maxWait: 555 };

    // rdp window show/hide
    watch(
      this.isActive,
      () =>
        this.props.type === "rdp" &&
        this.props.state === "connected" &&
        this.update(),
      { flush: "post" }
    );

    // rdp window resize
    watchDebounced(
      this.dimensions,
      () =>
        this.isActive.value &&
        this.props.type === "rdp" &&
        this.props.state === "connected" &&
        this.update(),
      debounceParams
    );

    // ssh terminal resize
    watchDebounced(
      () => [this.props.rows, this.props.cols],
      () => {
        console.log(this.props.rows, this.props.cols);
        if (!this.isActive.value || this.props.type !== "ssh") return;
        this.update();
      },
      debounceParams
    );
  }

  getSerializedLogs() {
    return this.logs.map(({ date, content }) => ({
      timestamp: +date,
      content,
    }));
  }

  log(text) {
    this.logs.push({ date: new Date(), content: text });
  }

  async connect() {
    this.props.state = "connecting";
    if (this.props.type === null) this.props.type = this.client.props.type;
    return await interopQueen.Connect(
      JSON.stringify(this.connectionOptions.value)
    );
  }

  async disconnect() {
    return await interopQueen.Disconnect(
      JSON.stringify(this.connectionOptions.value)
    );
  }

  async update() {
    return await interopQueen.Update(
      JSON.stringify(this.connectionOptions.value)
    );
  }

  async transfer() {
    return await interopQueen.Transfer(
      JSON.stringify(this.connectionOptions.value)
    );
  }

  async sshInput(data) {
    return await interopQueen.SSHInput(data);
  }

  processMessage(msg) {
    switch (msg.type) {
      case "RDP_LOG":
        const { content, event } = msg;

        if (event === "disconnect") this.props.state = "disconnected";
        else if (event === "connect") this.props.state = "connected";

        this.log(content);
        break;

      case "RDP_NEWOWNER":
        this.props.state = "disconnected";
        this.props.parent?.remove(this);
        break;

      case "TAB_TRANSFER_REQUEST":
        const serializedTab = this.serialize();
        this.props.state = "disconnected";
        this.props.parent?.remove(this);
        return serializedTab;

      default:
        console.assert(false, "Unknown message type");
    }
  }

  processSharedBuffer(e) {
    const displayBufferSize = e.additionalData.displayBufferSize;
    const buffer = e.getBuffer();
    this.props.buffer = {
      display: new Uint8Array(buffer, 0, displayBufferSize),
      size: new Int32Array(buffer, displayBufferSize, 1),
    };
  }

  setTerminalSize(rows, cols) {
    this.props.rows = rows;
    this.props.cols = cols;
  }

  serialize() {
    return {
      id: this.id,
      client: this.client.serialize(),
      logs: this.getSerializedLogs(),
      props: {
        ...this.props,
        buffer: null,
        parent: null,
      },
    };
  }
}
