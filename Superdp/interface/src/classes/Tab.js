import { computed, shallowReactive, watch } from "vue";
import { watchDebounced } from "@vueuse/core";
import { interopQueen } from "../globals";
import { Client } from "./Client";
import { v4 as uuidv4 } from "uuid";

export class Tab extends EventTarget {
  constructor({
    id = uuidv4(),
    client,
    props: { type = null, state = "disconnected" } = {},
    serializedLogs = [],
  }) {
    super();
    this.id = id;

    this.client = client;
    console.assert(this.client instanceof Client);

    this.isActive = computed(() => this.props.parent?.activeTab === this);
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
      if (this.props.type === "rdp" && this.props.parent?.workAreaSize)
        return this.props.parent.workAreaSize;

      if (this.props.type === "ssh" && this.props.rows && this.props.cols)
        return { rows: this.props.rows, cols: this.props.cols };

      return null;
    });

    this.connectionOptions = computed(() => ({
      clientId: this.client.id,
      tabId: this.id,
      type: this.props.type,

      client: {
        ...this.client.props,
      },

      ...(this.dimensions.value ?? {}),

      visible: this.isActive.value,
    }));

    const debounceParams = { debounce: 222, maxWait: 555 };

    // tab show/hide
    watch(
      this.isActive,
      (isActive) => {
        if (this.props.state?.startsWith("connect")) this.update();
        if (isActive) this.dispatchEvent(new Event("focus"));
      },
      { flush: "post" }
    );

    // window resize
    watchDebounced(
      this.dimensions,
      () =>
        this.isActive.value &&
        this.props.state === "connected" &&
        this.update(),
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
    if (this.props.state == "disconnected" || this.props.type === null)
      this.props.type = this.client.props.type;
    this.props.state = "connecting";

    // Wait for dimensions before sending connect request
    if (this.dimensions.value === null) {
      await new Promise((resolve) =>
        watch(this.dimensions, () => resolve(), {
          once: true,
        })
      );
    }

    return await interopQueen.Connect(
      JSON.stringify(this.connectionOptions.value)
    );
  }

  async disconnect() {
    if (this.props.state === "disconnected") return;
    this.props.state = "disconnecting";
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
    if (this.props.state !== "connected") return;
    return await interopQueen.SSHInput(this.id, data);
  }

  processMessage(msg) {
    switch (msg.type) {
      case "TAB_LOG":
        const { content, event } = msg;

        if (event === "disconnect") {
          this.props.state = "disconnected";
        } else if (event === "connect") {
          this.props.state = "connected";
          this.client.setLastConnected(Date.now());
          this.dispatchEvent(new Event("focus"));
        }
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
    // release old buffer if it exists
    if (this.props.buffer?.raw)
      chrome.webview.releaseBuffer(this.props.buffer.raw);

    const displayBufferSize = e.additionalData.displayBufferSize;
    const buffer = e.getBuffer();
    this.props.buffer = {
      raw: buffer,
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
