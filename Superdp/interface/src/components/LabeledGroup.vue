<script setup>
defineProps({
  label: {
    type: String,
  },
  hint: {
    type: String,
    default: null,
  },
});
</script>

<template>
  <label class="input-box">
    <div class="label-row">
      <div v-if="label" class="label-text">{{ label }}</div>
      <div v-if="hint !== null" class="hint-text">{{ hint }}</div>
    </div>
    <slot></slot>
  </label>
</template>
<style scoped>
.input-box {
  display: flex;
  flex-direction: column;
  gap: 8px;
  align-self: flex-start;
}
.label-row {
  display: flex;
  font-size: 11px;
  gap: 8px;
  justify-content: space-between;
  white-space: nowrap;
}
.label-text {
  text-transform: uppercase;
  color: var(--light);
  font-weight: 700;
  user-select: none;
}
.hint-text {
  color: var(--lightest-gray);
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
