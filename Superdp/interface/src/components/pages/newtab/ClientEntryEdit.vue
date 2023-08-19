<script setup>
import { computed, inject } from "vue";
import { ClientEntry } from "../../../classes/ClientEntry";
import { tabManagerKey } from "../../../keys";
import LargeInput from "./LargeInput.vue";
import InputBox from "../../InputBox.vue";

const props = defineProps({
  entry: {
    type: ClientEntry,
    required: true,
  },
});

const client = computed(() => props.entry.client);

const tabManager = inject(tabManagerKey);

const handleConnect = () => {
  console.log(client);
  client.value.createOrSwitchToTab(tabManager);
};
</script>

<template>
  <form>
    <LargeInput v-model="client.props.name" placeholder="Untitled connection" />
    <InputBox
      class="txt-input"
      label="Hostname"
      v-model="client.props.host"
      placeholder="127.0.0.1"
      :info="/^[a-z0-9.-]*$/i.test(client.props.host) ? '' : 'Invalid hostname'"
    />
    <InputBox
      class="txt-input"
      label="Username"
      v-model="client.props.username"
      placeholder="admin"
      :info="
        client.props.username.includes('\\')
          ? 'Domain: ' + client.props.username.split('\\')[0]
          : ''
      "
    />
    <InputBox
      class="txt-input"
      label="Password"
      v-model="client.props.password"
      type="password"
      placeholder="Le$$1sM0re"
    />
    <button class="submit" @click.prevent="handleConnect">Connect</button>
  </form>
</template>

<style scoped>
form {
  display: flex;
  flex-direction: column;
  gap: 15px;
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
