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
    <div class="info-text">New Connection</div>
  </NavBarItem>
</template>

<style scoped>
.new-tab {
  --icon-fill: var(--light);
  background-color: var(--darker-gray);
  position: relative;
  flex-grow: 1;
}
.new-tab:hover {
  --icon-fill: var(--lightest);
  background-color: var(--darke-gray);
}
.new-tab.active {
  background-color: var(--dark-gray);
}
.new-tab.active::before {
  content: "";
  position: absolute;
  top: -1px;
  left: 0;
  width: 100%;
  height: 1px;
  background-color: var(--dark-gray);
}
.new-tab > .info-text {
  opacity: 0;
  color: var(--lightest-gray);
}
.new-tab:not(.active):hover > .info-text {
  opacity: 1;
}
svg {
  fill: var(--icon-fill);
  margin: 0 10px;
}
.new-tab.active svg {
  opacity: 0;
}
</style>
