import { Entry } from "./Entry";
import { shallowReactive, ref, computed } from "vue";
import { v4 as uuidv4 } from "uuid";
import { watchIgnorable } from "@vueuse/core";

const entryComparator = (a, b) => {
  if (a.constructor !== b.constructor) return a.isDir() ? -1 : 1;

  const result = a.isDir()
    ? a.props.name.localeCompare(b.props.name)
    : a.client.label.value.localeCompare(b.client.label.value);

  return result ? result : a.id.localeCompare(b.id);
};

export class DirEntry extends Entry {
  #manager;
  #childSet;
  #childSetChange;
  isDir = () => true;
  constructor({ manager, id, props, parentEntry }) {
    super();
    this.#manager = manager;
    this.id = id || uuidv4();

    this.type = "dir";

    this.#childSet = shallowReactive(new Set());

    // This may turn out to be too slow if there are many elements
    this.children = computed(() =>
      [...this.#childSet.values()].sort(entryComparator)
    );
    this.#childSetChange = ref(0);

    // We can't watch this.#childSet because watchers are deep by default,
    // and picks up on changes on child entry reactive objects.
    // We don't watch this.children because it changes when any child name/label change
    // What we need is a shallow watch on this.#childSet
    this.childrenWatcher = watchIgnorable(this.#childSetChange, () =>
      this.#manager.save(this)
    );

    this.props = shallowReactive({});
    this.propsWatcher = watchIgnorable(
      () => this.props.name,
      () => this.#manager.save(this)
    );
    this.#populateProps(props);

    this.#manager.dirEntries.set(id, this);
    parentEntry?.addChild(this);
  }

  get label() {
    return this.props.name;
  }

  #populateProps({ name = "New Directory", collapsed = false } = {}) {
    this.propsWatcher.ignoreUpdates(() => {
      this.props.name = name;
      this.props.collapsed = collapsed;
    });
  }

  addChild(entry) {
    console.assert(entry.parent === null);
    entry.parent = this;

    console.assert(!this.#childSet.has(entry));
    this.#childSet.add(entry);

    this.#childSetChange.value++;
  }

  removeChild(entry) {
    console.assert(this.#childSet.has(entry));
    this.#childSet.delete(entry);

    console.assert(entry.parent === this);
    entry.parent = null;

    this.#childSetChange.value++;
  }

  toggleCollapse() {
    this.props.collapsed = !this.props.collapsed;
  }

  serialize() {
    const obj = {
      id: this.id,
      props: {
        name: this.props.name,
        collapsed: this.props.collapsed,
      },
      children: this.children.value.map((entry) => entry.id),
    };
    if (this.parent === null) obj.root = true;
    return obj;
  }

  serializeTree(Ar = []) {
    Ar.push(this.serialize());
    this.children.value
      .filter((entry) => entry.isDir())
      .forEach((entry) => entry.serializeTree(Ar));
    return Ar;
  }

  reconcileStep1({ props, children }) {
    this.#populateProps(props);

    this.childrenWatcher.ignoreUpdates(() => {
      const ids = new Set(children);

      // Remove entries
      for (const entry of this.#childSet.values()) {
        if (!ids.has(entry.id)) this.removeChild(entry);
      }
    });
  }

  reconcileStep2({ children }) {
    this.childrenWatcher.ignoreUpdates(() => {
      // Add additional entries
      for (const id of children) {
        const entry = this.#manager.getEntry(id);
        if (entry) this.addChild(entry);
      }
    });
  }
}
