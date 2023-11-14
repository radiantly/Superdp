<script setup>
import { computed, inject } from "vue";
import { ClientEntry } from "../../../classes/ClientEntry";
import { tabManagerKey } from "../../../keys";
import LargeInput from "./LargeInput.vue";
import LabeledGroup from "../../LabeledGroup.vue";
import TextInput from "../../TextInput.vue";
import SelectInput from "../../SelectInput.vue";
import { clientManager } from "../../../globals";

const props = defineProps({
  entry: {
    type: ClientEntry,
    required: true,
  },
  fullSize: {
    type: Boolean,
    required: true,
  },
});

const client = computed(() => props.entry.client);

const tabManager = inject(tabManagerKey, clientManager.session.children[0]);

const handleConnect = () => {
  console.log(client);
  client.value.createTab(tabManager);
};
</script>

<template>
  <form :class="{ 'full-size': fullSize }">
    <LargeInput
      v-if="fullSize"
      v-model="client.props.name"
      placeholder="Untitled connection"
    />
    <LabeledGroup label="Connection name" v-else>
      <TextInput
        class="txt-input"
        v-model="client.props.name"
        placeholder="Untitled connection"
      />
    </LabeledGroup>
    <LabeledGroup label="Hostname" :hint="client.hints.host">
      <TextInput
        class="txt-input"
        v-model="client.props.host"
        placeholder="127.0.0.1"
      />
    </LabeledGroup>
    <LabeledGroup label="Connection type">
      <SelectInput
        class="txt-input"
        :options="['rdp', 'ssh']"
        v-model="client.props.type"
      />
    </LabeledGroup>
    <LabeledGroup label="Username" :hint="client.hints.username">
      <TextInput
        class="txt-input"
        v-model="client.props.username"
        placeholder="admin"
      />
    </LabeledGroup>
    <LabeledGroup label="Password">
      <TextInput
        class="txt-input"
        v-model="client.props.password"
        type="password"
        placeholder="Le$$1sM0re"
      />
    </LabeledGroup>
    <button class="submit" @click.prevent="handleConnect">Connect</button>
  </form>
</template>

<style scoped>
form {
  display: flex;
  flex-direction: column;
  gap: 15px;
  padding: 0 19px;
}
form.full-size {
  gap: 30px;
  padding: 35px 50px;
}

form input {
  border: none;
  outline: none;
  min-width: 0;
  font-family: var(--ui-font);
  width: 0;
}

:deep(.txt-input) {
  min-width: min(100%, 275px);
}
.submit {
  align-self: flex-start;
  border: none;
  background-color: #3178ca;
  font: var(--ui-font);
  color: white;
  padding: 5px 15px;
  border-radius: 2px;
}
</style>
