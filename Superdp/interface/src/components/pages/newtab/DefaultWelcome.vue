<script setup>
import { inject, ref } from "vue";
import { clientManager } from "../../../globals";
import { Tab } from "../../../classes/Tab";
import { tabManagerKey } from "../../../keys";

const tabManager = inject(tabManagerKey);

const quickHost = ref("");
const handleKeydown = (e) => {
  if (e.key === "Enter") {
    const [username, host] = quickHost.value.includes("@")
      ? quickHost.value.split("@", 2)
      : ["", quickHost.value];
    const client = clientManager.createFloatingClient({
      username,
      host,
    });
    const tab = new Tab({ client });
    client.addTab(tabManager, tab);
    quickHost.value = "";
  }
};
</script>

<template>
  <div class="container">
    <input
      class="quick-input"
      placeholder="root@172.48.1.104"
      @keydown="handleKeydown"
      v-model="quickHost"
    />
  </div>
</template>
<style scoped>
.container {
  padding: 35px 50px;

  display: flex;
  flex-direction: column;
}
.quick-input {
  font-size: 24px;
  font-family: var(--ui-font);
  border: none;
  background-color: var(--da-gray);
  border-radius: 3px;
  padding: 8px 15px;
  outline: none;
}
.quick-input::placeholder {
  color: var(--lighter-gray);
}
</style>
