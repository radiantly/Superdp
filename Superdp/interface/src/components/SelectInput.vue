<script setup>
defineProps({
  options: {
    type: Array,
    required: true,
  },
  modelValue: {},
});
</script>

<template>
  <div class="select-wrap">
    <div
      class="option"
      v-for="val in options"
      :class="{ active: val === modelValue }"
      :key="val"
      @click="$emit('update:modelValue', val)"
    >
      {{ val }}
    </div>
  </div>
</template>

<style scoped>
.select-wrap {
  display: flex;
  background-color: var(--da-gray);
  align-self: flex-start;
}

.option {
  color: var(--lightest-gray);
  padding: 5px 15px;
  border-radius: 1px;
  cursor: pointer;
  transition: color 0.2s ease, background-color 0.2s ease;
}

.option:hover {
  background-color: var(--gray);
}

.option.active {
  color: var(--lightest);
  background-color: var(--striking-blue);
}
</style>
