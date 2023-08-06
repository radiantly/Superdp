import { shallowRef, shallowReactive, reactive } from "vue";
import { Tab } from "./Tab";

export class TabManager {
  static NEW_TAB = null;
  constructor() {
    this.tabs = shallowReactive([]);
    this.activeTab = shallowRef(null);
    this.props = shallowReactive({
      workAreaSize: {
        width: 0,
        height: 0,
      },
      active: null,
    });
  }

  setSize({ width, height }) {
    this.props.workAreaSize = { width, height };
  }

  setActive(tab) {
    this.props.active = tab;
  }

  add(tab) {
    this.insert(tab, this.tabs.length);
  }

  insert(tab, position) {
    tab.props.parent?.remove(tab);

    tab.props.parent = this;
    this.tabs.splice(position, 0, tab);
  }

  remove(tab) {
    console.assert(tab.props.parent === this);
    const idx = this.tabs.findIndex((t) => t === tab);

    this.tabs.splice(idx, 1);

    if (tab === this.props.active) {
      this.props.active = this.tabs.length
        ? this.tabs[Math.min(idx, this.tabs.length - 1)]
        : null;
    }

    tab.props.parent = null;
  }
}
