<script setup>
import { VscAdd } from "react-icons/vsc";
import { applyPureReactInVue } from "veaury";

import NavBarItem from "./NavBarItem.vue";
const VscAddVue = applyPureReactInVue(VscAdd);
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
