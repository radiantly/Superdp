import { ClientManager } from "./classes/ClientManager";
import { DragManager } from "./classes/DragManager";
import { ContextMenu } from "./classes/ContextMenu";
import { computed, ref, watch } from "vue";

export const broadcast = new BroadcastChannel("everybody");

export const interopQueen = new Proxy(chrome.webview.hostObjects.interopQueen, {
  get(queen, key) {
    console.debug("<", key);
    return queen[key];
  },
});

export const dragManager = new DragManager();

export const overlayVisible = ref(false);

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
watch(webViewInBackground, (newValue) =>
  newValue
    ? interopQueen.SendWebViewToBack()
    : interopQueen.BringWebViewToFront()
);
