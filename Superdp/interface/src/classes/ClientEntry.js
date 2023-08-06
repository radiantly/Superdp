import { Entry } from "./Entry";

export class ClientEntry extends Entry {
  constructor({ client }) {
    super();
    this.type = "leaf";
    this.client = client;
  }

  get id() {
    return this.client.id;
  }
}
