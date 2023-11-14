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
  gap: 10px;
}

.option {
  border: 1px solid #303030;
  color: #aaa;
  padding: 5px 10px;
  border-radius: 1px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.option.active {
  color: #ccc;
  border: 1px solid transparent;
  background-color: #303030;
}

.option:hover {
  color: #ccc;
  background-color: #383838;
}
</style>
