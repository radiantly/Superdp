import { shallowReactive } from "vue";

export class ContextMenu {
  constructor() {
    this.props = shallowReactive({
      visible: false,
      items: [],
      pos: {
        x: 0,
        y: 0,
      },
    });
  }
  /**
   * Show context menu
   * @param {MouseEvent} event
   * @param {{label: String}[]} items
   */
  show(event, items) {
    event.preventDefault();
    this.props.items = items;
    this.props.visible = true;
    this.props.pos = {
      x: event.clientX,
      y: event.clientY,
    };
  }
  hide() {
    this.props.visible = false;
  }
}
