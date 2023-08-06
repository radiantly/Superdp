import { shallowReactive } from "vue";
import { ClientEntry } from "./ClientEntry";
import { DirEntry } from "./DirEntry";

class ContextMenu {
  constructor() {
    this.props = shallowReactive({});
  }

  #getMenuItems(obj) {
    if (obj instanceof ClientEntry) {
      return [
        {
          label: "Delete",
          handler: (obj) => obj.parent.removeChild(obj),
        },
      ];
    }

    if (obj instanceof DirEntry) {
      return [
        {
          label: "New connection...",
          handler: () =>
            (focusedEntry.value = clientManager.createClient({
              parentEntry: props.entry,
            }).entry),
        },
        {
          label: "New directory group...",
          handler: () =>
            (focusedEntry.value = new DirEntry({
              manager: clientManager,
              parentEntry: props.entry,
            })),
        },
      ];
    }
  }

  show(e, obj) {}
}
