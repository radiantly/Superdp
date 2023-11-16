import { computed } from "vue";
import { TabManager } from "./TabManager";
import { watchDebounced } from "@vueuse/core";
import { interopQueen } from "../globals";

// Later on this SessionManager will be useful in handling split views, etc
export class SessionManager {
  constructor() {
    this.children = [new TabManager()];

    this.navAreas = computed(() =>
      this.children.map((tabManager) => tabManager.props.navSize)
    );
    // watchDebounced(this.navAreas, () =>
    //   interopQueen.UpdateNavAreas(this.navAreas.value)
    // );
  }

  getTab(tabId) {
    for (const tabManager of this.children)
      for (const tab of tabManager.tabs) if (tab.id === tabId) return tab;

    return null;
  }
}
