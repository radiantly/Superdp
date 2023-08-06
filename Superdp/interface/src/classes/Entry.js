import { computed, shallowReactive, shallowRef } from "vue";

export class Entry {
  #parent = shallowRef(null);
  #root = computed(() => this.#parent.value?.root ?? this);
  isDir = () => false;

  get parent() {
    return this.#parent.value;
  }
  set parent(dirEntry) {
    this.#parent.value = dirEntry;
  }

  get root() {
    return this.#root.value;
  }
}
