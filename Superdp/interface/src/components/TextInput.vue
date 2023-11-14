<script setup>
defineProps({
  modelValue: {},
});
</script>

<template>
  <input
    :value="modelValue"
    @input="$emit('update:modelValue', $event.target.value)"
  />
</template>

<style scoped>
input {
  border: none;
  outline: none;
  min-width: min(100%, 275px);
  font-family: var(--ui-font);
  width: 0;
  background-color: #303030;
  color: #ccc;
  padding: 5px 10px;
  transition: background-color 0.2s ease;
}

input:hover,
input:focus {
  background-color: #383838;
}
</style>
