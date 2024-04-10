<script setup>
import { VscAddVue } from "../icons";

import NavBarItem from "./NavBarItem.vue";
const props = defineProps({
  active: Boolean,
});
</script>

<template>
  <NavBarItem class="new-tab" :class="{ active: props.active }">
    <VscAddVue className="react-icon" />
  </NavBarItem>
</template>

<style scoped>
.new-tab {
  --icon-fill: var(--light);
}
.new-tab:hover {
  --icon-fill: var(--lightest);
  background-color: var(--da-gray);
}
svg {
  fill: var(--icon-fill);
  margin: 0 10px;
}
</style>
