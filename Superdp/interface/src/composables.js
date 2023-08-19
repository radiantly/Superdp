import { onBeforeUnmount, onMounted } from "vue";

import { clientManager, interopQueen } from "./globals";
import { TabManager } from "./classes/TabManager";

export const provideData = async () => {
  const creationTimestamp = await interopQueen.GetFormCreationTimestamp();
  const elapsedTime = Date.now() - creationTimestamp;
  console.log(`Startup took ${elapsedTime}ms`);
};

export const useWebMessages = () => {
  const messageHandler = (message) => clientManager.processMessage(message);

  onMounted(() => chrome.webview.addEventListener("message", messageHandler));
  onBeforeUnmount(() =>
    chrome.webview.removeEventListener("message", messageHandler)
  );
};

export const useTabManager = (...args) => {
  const tabManager = new TabManager(...args);

  onMounted(() => clientManager.tabManagers.add(tabManager));
  onBeforeUnmount(() => clientManager.tabManagers.delete(tabManager));

  return tabManager;
};
