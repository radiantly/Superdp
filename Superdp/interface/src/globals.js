import { ClientManager } from "./classes/ClientManager";
import { DragManager } from "./classes/DragManager";
import { ContextMenu } from "./classes/ContextMenu";
import { ref, watch } from "vue";

export const interopQueen = chrome.webview.hostObjects.interopQueen;

export const dragManager = new DragManager();

export const webViewInForeground = ref(0);
watch(webViewInForeground, (newValue) =>
  newValue
    ? interopQueen.BringWebViewToFront()
    : interopQueen.SendWebViewToBack()
);

export const overlayVisible = ref(false);

const conf = JSON.parse((await interopQueen.ReadConf()) || "{}");
export const clientManager = new ClientManager(conf);

/*
 * Context Menu
 */
export const contextMenu = new ContextMenu();
watch(
  () => contextMenu.props.visible,
  (isVisible) => (webViewInForeground.value += isVisible ? 1 : -1)
);
