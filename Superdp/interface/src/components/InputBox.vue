<script setup>
defineProps({
  label: {
    type: String,
  },
  modelValue: {},
});
defineOptions({
  inheritAttrs: false,
});
</script>

<template>
  <label class="input-box">
    <div v-if="label" class="label-text">{{ label }}</div>
    <input
      v-bind="$attrs"
      :value="modelValue"
      @input="$emit('update:modelValue', $event.target.value)"
    />
  </label>
</template>

<style scoped>
.input-box {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.label-text {
  text-transform: uppercase;
  color: #b3b3b3;
  font-size: 11px;
  font-weight: 700;
  user-select: none;
}

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
