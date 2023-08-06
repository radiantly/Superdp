import { useDebounceFn } from "@vueuse/core";
import { Client } from "./Client";
import { DirEntry } from "./DirEntry";
import { interopQueen } from "../globals";

export class ChangeManager {
  #reportChanges;
  constructor() {
    this.reset();

    this.#reportChanges = useDebounceFn(() => this.#broadcast(), 222, {
      maxWait: 999,
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
    return JSON.stringify({
      type: "RECONCILE",
      changes: {
        clients: this.#serializeSet(this.changes.clients),
        dir_entries: this.#serializeSet(this.changes.dir_entries),
      },
    });
  }

  async #broadcast() {
    const message = this.#prepareSerializedMessage();
    this.reset();
    await interopQueen.BroadcastMessageToOtherInstances(message);
  }
}
