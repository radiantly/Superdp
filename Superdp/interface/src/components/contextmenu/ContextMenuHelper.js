import { inject, provide, shallowReactive } from "vue";
import { contextMenuHelperKey } from "../../keys";

class ContextMenuHelper {
  constructor(options) {
    if (options?.provide) {
      this.menu = shallowReactive({
        visible: false,
        items: [],
        pos: {
          x: 0,
          y: 0,
        },
      });
      provide(contextMenuHelperKey, this.menu);
    } else {
      this.menu = inject(contextMenuHelperKey);
    }
  }
  /**
   * Show context menu
   * @param {MouseEvent} event
   * @param {{label: String}[]} items
   */
  show(event, items) {
    event.preventDefault();
    this.menu.items = items;
    this.menu.visible = true;
    this.menu.pos = {
      x: event.clientX,
      y: event.clientY,
    };
  }
  hide() {
    this.menu.visible = false;
  }
}

export const useContextMenu = (options) => {
  return new ContextMenuHelper(options);
};
