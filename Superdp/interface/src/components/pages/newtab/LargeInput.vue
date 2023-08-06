<script setup>
import IconPencil from "../../icons/IconPencil.vue";

defineProps(["modelValue", "placeholder"]);
defineEmits(["update:modelValue"]);
</script>

<template>
  <label class="name-group">
    <input
      class="name-input"
      :placeholder="placeholder"
      :value="modelValue"
      @input="$emit('update:modelValue', $event.target.value)"
    />
    <IconPencil />
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
  font-family: var(--ui-font);
  width: 0;
  font-size: 40px;
  font-weight: 300;
  flex-grow: 1;
  background-color: transparent;
  color: #eee;
  padding: 0;
}

.name-input:not(:hover) + svg {
  opacity: 0;
}
.name-group > svg {
  margin-left: 5px;
  width: 22px;
  height: 22px;
  align-self: center;
  fill: #eee;
}
</style>
