import { TabManager } from "./TabManager";

// Later on this SessionManager will be useful in handling split views, etc
export class SessionManager {
  constructor() {
    this.children = [new TabManager()];
  }

  getTab(tabId) {
    for (const tabManager of this.children) {
      for (const tab of tabManager.tabs) {
        if (tab.id === tabId) {
          return tab;
        }
      }
    }
    return null;
  }
}
