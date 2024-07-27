import { shallowReactive } from "vue";
import { v4 as uuidv4 } from "uuid";

export class TabManager {
  static NEW_TAB = null;
  #id;
  #tabs;
  #props;
  constructor() {
    this.#id = uuidv4();
    this.#tabs = shallowReactive([]);
    this.#props = shallowReactive({
      workAreaSize: {
        x: 0,
        y: 0,
        width: 0,
        height: 0,
      },
      navSize: {
        x: 0,
        y: 0,
        width: 0,
        height: 0,
      },
      active: null,
    });
  }

  get tabs() {
    return this.#tabs;
  }

  get activeTab() {
    return this.#props.active;
  }

  setActive(tab) {
    if (tab === TabManager.NEW_TAB || this.#tabs.includes(tab))
      this.#props.active = tab;
  }

  get navSize() {
    return this.#props.navSize;
  }

  setNavSize({ x, y, width, height }) {
    console.log({ width });
    this.#props.navSize = { x, y, width, height };
  }

  setWorkAreaSize({ x, y, width, height }) {
    this.#props.workAreaSize = { x, y, width, height };
  }

  add(tab) {
    this.insert(tab, this.#tabs.length);
  }

  insert(tab, position) {
    tab.props.parent?.remove(tab);

    tab.props.parent = this;
    this.#tabs.splice(position, 0, tab);
  }

  remove(tab) {
    console.assert(tab.props.parent === this);
    const idx = this.#tabs.findIndex((t) => t === tab);

    this.#tabs.splice(idx, 1);

    if (tab === this.#props.active) {
      this.#props.active = this.#tabs.length
        ? this.#tabs[Math.min(idx, this.#tabs.length - 1)]
        : null;
    }

    tab.props.parent = null;
  }
}
