import { useDebounceFn } from "@vueuse/core";
import { Client } from "./Client";
import { DirEntry } from "./DirEntry";
import { broadcast } from "../utils";

export class ChangeManager {
  #reportChanges;
  constructor() {
    this.reset();

    this.#reportChanges = useDebounceFn(() => this.#broadcast(), 222, {
      maxWait: 444,
    });
  }

  reset() {
    this.changes = {
      clients: new Set(),
      dir_entries: new Set(),
    };
  }

  add(obj) {
    if (obj instanceof Client) this.changes.clients.add(obj);
    else if (obj instanceof DirEntry) this.changes.dir_entries.add(obj);
    this.#reportChanges();
  }

  #serializeSet(s) {
    return Array.from(s.values()).map((elem) => elem.serialize());
  }

  #prepareSerializedMessage() {
    return {
      type: "RECONCILE",
      changes: {
        clients: this.#serializeSet(this.changes.clients),
        dir_entries: this.#serializeSet(this.changes.dir_entries),
      },
    };
  }

  #broadcast() {
    const message = this.#prepareSerializedMessage();
    this.reset();
    console.debug("< broadcast", message);
    broadcast(message);
  }
}
