<script setup>
import IconPlus from "../icons/IconPlus.vue";
import NavBarItem from "./NavBarItem.vue";
const props = defineProps({
  active: Boolean,
});
</script>

<template>
  <NavBarItem class="new-tab" :class="{ active: props.active }">
    <IconPlus />
  </NavBarItem>
</template>

<style scoped>
.new-tab {
  --icon-fill: #bbb;
}
.new-tab:hover {
  --icon-fill: #fff;
  background-color: var(--gray);
}
svg {
  width: 13px;
  height: 13px;
  fill: var(--icon-fill);
  margin: 0 10px;
}
</style>
