<script setup>
import { computed, inject } from "vue";
import { ClientEntry } from "../../../classes/ClientEntry";
import { tabManagerKey } from "../../../keys";
import LargeInput from "./LargeInput.vue";
import LabeledGroup from "../../LabeledGroup.vue";
import TextInput from "../../TextInput.vue";
import SelectInput from "../../SelectInput.vue";
import TextAreaInput from "../../TextAreaInput.vue";

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

const tabManager = inject(tabManagerKey);
const tab = inject("tab", null);
</script>

<template>
  <form :class="{ 'full-size': fullSize }">
    <LargeInput
      v-if="fullSize"
      v-model="client.props.name"
      placeholder="Untitled connection"
    />
    <LabeledGroup class="input-width" label="Connection name" v-else>
      <TextInput
        class="input-width"
        v-model="client.props.name"
        placeholder="Untitled connection"
      />
    </LabeledGroup>
    <LabeledGroup
      class="input-width"
      label="Hostname"
      :hint="client.hints.host"
    >
      <TextInput class="input-width" v-model="client.props.host" />
    </LabeledGroup>
    <LabeledGroup
      class="input-width"
      label="Connection type"
      :hint="client.hints.type"
    >
      <SelectInput :options="['rdp', 'ssh']" v-model="client.props.type" />
    </LabeledGroup>
    <LabeledGroup
      class="input-width"
      label="Username"
      :hint="client.hints.username"
    >
      <TextInput class="input-width" v-model="client.props.username" />
    </LabeledGroup>
    <LabeledGroup
      v-show="client.props.type === 'rdp'"
      class="input-width"
      label="Password"
    >
      <TextInput
        class="input-width"
        v-model="client.props.password"
        type="password"
      />
    </LabeledGroup>
    <LabeledGroup
      v-show="client.props.type === 'ssh'"
      class="input-width"
      label="SSH Key"
    >
      <TextAreaInput class="input-width" v-model="client.props.key" />
    </LabeledGroup>
    <button
      v-if="tab"
      class="submit"
      :class="{
        valid: client.valid.value && tab.props.state === 'disconnected',
      }"
      @click.prevent="() => tab.connect()"
    >
      {{ tab.props.state === "disconnected" ? "Connect" : tab.props.state }}
    </button>
    <button
      v-else
      class="submit"
      :class="{
        valid: client.valid.value,
      }"
      @click.prevent="() => client.createTab(tabManager)"
    >
      Connect
    </button>
  </form>
</template>

<style scoped>
form {
  display: flex;
  flex-direction: column;
  gap: 15px;
  padding: 0 18px;
  overflow: auto;
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

:deep(.input-width) {
  min-width: min(100%, 275px);
}
.submit {
  align-self: flex-start;
  border: none;
  color: var(--lightest-gray);
  background-color: var(--gray);
  font: var(--ui-font);
  padding: 5px 15px;
  border-radius: 2px;
  pointer-events: none;
  transition: color 0.2s ease, background-color 0.2s ease;
  text-transform: capitalize;
}
.submit.valid {
  color: var(--lightest);
  pointer-events: auto;
  background-color: var(--striking-blue);
  cursor: pointer;
}
</style>
