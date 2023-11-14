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
  </div>
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
