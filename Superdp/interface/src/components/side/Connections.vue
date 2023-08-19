<!-- eslint-disable vue/multi-word-component-names -->
<script setup>
import TreeManager from "./tree/TreeManager.vue";
</script>
<template>
  <div class="top-row">
    <div class="title">Connection manager</div>
    <!-- TODO: Add/Teleport icons here -->
  </div>
  <TreeManager />
</template>
<style scoped>
.top-row {
  display: flex;
  min-width: 0;
}
.title {
  flex-grow: 1;
  text-transform: uppercase;
  color: #bbb;
  font-weight: 400;
  font-size: 11px;
  padding: 12px 18px;
  white-space: nowrap;
  text-overflow: ellipsis;
  overflow: hidden;
}
</style>
