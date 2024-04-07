<script setup>
const props = defineProps({
  label: {
    type: String,
    required: true,
  },
});
</script>
<template>
  <div class="top-row">
    <div class="title">
      {{ label }}
    </div>
    <div class="icons">
      <slot></slot>
    </div>
  </div>
</template>
<style scoped>
.top-row {
  display: flex;
  min-width: 0;
  justify-content: space-between;
  color: var(--light);
  padding: 0 18px;
}
.title {
  flex-grow: 1;
  text-transform: uppercase;
  color: var(--light);
  font-weight: 400;
  font-size: 11px;
  padding: 12px 0;
  white-space: nowrap;
  text-overflow: ellipsis;
  overflow: hidden;
}
.icons {
  display: flex;
  align-items: center;
}
</style>
