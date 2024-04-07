<script setup>
defineProps(["placeholder"]);
const model = defineModel();
</script>

<template>
  <label class="name-group">
    <input class="name-input" :placeholder="placeholder" v-model="model" />
  </label>
</template>

<style scoped>
.name-group {
  display: flex;
  position: relative;
}

.name-group .name-wrap {
  overflow: hidden;
}

.name-group .name-input {
  border: none;
  outline: none;
  min-width: 0;
  width: 0;
  font-size: 40px;
  font-weight: 300;
  flex-grow: 1;
  background-color: transparent;
  padding: 0;
  position: relative;
}
.name-group::before {
  content: "";
  position: absolute;
  top: -7px;
  left: -12px;
  width: calc(100% + 24px);
  height: calc(100% + 14px);
  border-radius: 3px;
  transition: background-color 0.2s ease;
}
.name-group:focus-within::before,
.name-group:hover::before {
  background-color: var(--da-gray);
}
</style>
