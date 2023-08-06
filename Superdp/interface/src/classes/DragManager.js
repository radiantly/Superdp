import { shallowReactive } from "vue";
export class DragManager {
  constructor() {
    this.obj = null;
    this.data = {};
    this.props = shallowReactive({
      obj: null,
    });
  }

  start({ buttons }, callback) {
    if (buttons !== 1) return;
    this.props.isDragging = true;
    callback(this.props);
  }

  end({ buttons }, callback) {
    if (buttons === 0 && this.props.isDragging) callback(this.props);
    this.clear();
  }

  clear() {
    Object.keys(this.props).forEach((key) => delete this.props[key]);
  }
}
