import { onBeforeUnmount, onMounted, provide, shallowReactive } from "vue";
import { clientsKey, dragManagerKey, treeKey } from "./keys";

import { clientManager, interopQueen } from "./globals";
import { TabManager } from "./classes/TabManager";

const allEventHandlers = new Map();

export class EventHandler {
  constructor(componentKey) {
    if (!new.target)
      throw new TypeError("EventHandler() must be instantiated with new");

    this.registeredHandlers = [];

    if (!allEventHandlers.has(componentKey))
      allEventHandlers.set(componentKey, new Map());

    this.handlers = allEventHandlers.get(componentKey);
  }

  register(eventName, handler) {
    if (!this.handlers.has(eventName)) this.handlers.set(eventName, new Set());

    this.handlers.get(eventName).add(handler);
    this.registeredHandlers.push([eventName, handler]);

    return this;
  }

  unregisterAll() {
    for (const [eventName, handler] of this.registeredHandlers) {
      this.handlers.get(eventName).delete(handler);
    }
  }

  handleEvent(event) {
    const handlers = this.handlers.get(event.type) || [];
    for (const handler of handlers) handler(event);
  }
}

export const useKeyedEventHandler = (key) => {
  const eventHandler = new EventHandler();

  onBeforeUnmount(() => eventHandler.unregisterAll());

  return eventHandler;
};

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
