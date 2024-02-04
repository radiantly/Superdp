import { ClientManager } from "./classes/ClientManager";
import { DragManager } from "./classes/DragManager";
import { ContextMenu } from "./classes/ContextMenu";
import { computed, ref, watch } from "vue";
import { sleep } from "./utils";

export const broadcastChannel = new BroadcastChannel("everybody");

export const interopQueen = new Proxy(chrome.webview.hostObjects.interopQueen, {
  get(queen, key) {
    return (...args) => {
      console.debug("<", key, args);
      return queen[key](...args);
    };
  },
});

export const dragManager = new DragManager();

export const overlayVisible = ref(false);
export const windowIsMaximized = ref(false);

const conf = JSON.parse((await interopQueen.ReadConf()) || "{}");
export const clientManager = new ClientManager(conf);

/*
 * Context Menu
 */
export const contextMenu = new ContextMenu();

// Replace this with watchEffect?
export const webViewInBackground = computed(
  () => !overlayVisible.value && !contextMenu.props.visible
);
watch(
  webViewInBackground,
  async (newValue) =>
    await (newValue
      ? sleep(200).then(() => interopQueen.SendWebViewToBack()) // sleep for animation
      : interopQueen.BringWebViewToFront())
);
