<script setup>
const model = defineModel();
</script>

<template>
  <input v-model="model" />
</template>

<style scoped>
input {
  border: none;
  outline: none;
  min-width: min(100%, 275px);
  width: 0;
  background-color: var(--da-gray);
  padding: 5px 10px;
  transition: background-color 0.2s ease;
  border-bottom: 2px solid transparent;
}

input:hover,
input:focus {
  background-color: var(--gray);
}

input:focus {
  border-bottom: 2px solid var(--striking-blue);
}
</style>
