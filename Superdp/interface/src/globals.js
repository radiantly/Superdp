import { ClientManager } from "./classes/ClientManager";
import { DragManager } from "./classes/DragManager";
import { ref, watch } from "vue";

export const interopQueen = chrome.webview.hostObjects.interopQueen;

export const dragManager = new DragManager();

export const webViewInBackground = ref(true);
watch(webViewInBackground, (newValue) =>
  newValue
    ? interopQueen.SendWebViewToBack()
    : interopQueen.BringWebViewToFront()
);

export const overlayVisible = ref(false);

const conf = JSON.parse((await interopQueen.ReadConf()) || "{}");
export const clientManager = new ClientManager(conf);
