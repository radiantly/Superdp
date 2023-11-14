import { computed, shallowReactive, shallowRef, watch } from "vue";
import { watchDebounced } from "@vueuse/core";
import { interopQueen } from "../globals";
import { Client } from "./Client";
import { v4 as uuidv4 } from "uuid";

export class Tab {
  constructor({ client, props = {}, serializedLogs = [] }) {
    this.id = uuidv4();

    this.client = client;
    console.assert(this.client instanceof Client);

    this.isActive = computed(() => this.props.parent?.props.active === this);
    this.props = shallowReactive({
      ...props,
      type: null,
      state: "disconnected",
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

    this.displayBuffer = null;
    this.sizeBuffer = null;

    const debounceParams = { debounce: 222, maxWait: 555 };

    // rdp window show/hide
    watch(
      this.isActive,
      () =>
        this.props.type === "rdp" &&
        this.props.state === "connected" &&
        this.update()
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
    this.props.state = "disconnecting";
    return await interopQueen.Disconnect(
      JSON.stringify(this.connectionOptions.value)
    );
  }

  async update() {
    console.log(this.connectionOptions.value);
    return await interopQueen.Update(
      JSON.stringify(this.connectionOptions.value)
    );
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

      default:
        console.assert(false, "Unknown message type");
    }
  }

  processSharedBuffer(e) {
    const displayBufferSize = e.additionalData.displayBufferSize;
    const buffer = e.getBuffer();
    this.displayBuffer = new Uint8Array(buffer, 0, displayBufferSize);
    this.sizeBuffer = new Uint32Array(buffer, displayBufferSize, 1);
  }

  resizeTerminal(rows, cols) {
    this.props.rows = rows;
    this.props.cols = cols;
  }

  serialize() {
    return {
      client: this.client.serialize(),
      logs: this.getSerializedLogs(),
      props: {
        rdpControlVisible: this.props.rdpControlVisible,
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
