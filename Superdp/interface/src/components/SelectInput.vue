<script setup>
import { v4 as uuidv4 } from "uuid";
defineProps({
  options: {
    type: Array,
    required: true,
  },
});
const model = defineModel();

// The input radios need the same name so that the browser knows that it's part
// of the same radio button group
const name = uuidv4();
</script>

<template>
  <div class="select-wrap">
    <label v-for="val in options" :key="val">
      <input type="radio" :name="name" :value="val" v-model="model" />
      {{ val }}
    </label>
  </div>
</template>

<style scoped>
.select-wrap {
  display: flex;
  background-color: var(--da-gray);
  align-self: flex-start;
}

label {
  color: var(--lightest-gray);
  padding: 5px 15px;
  border-radius: 1px;
  cursor: pointer;
  transition: color 0.2s ease, background-color 0.2s ease;
  position: relative;
}

label input {
  position: fixed;
  opacity: 0;
  pointer-events: none;
}

label:hover,
label:focus-within {
  background-color: var(--gray);
}

label:focus-within::after {
  content: "";
  position: absolute;
  bottom: -4px;
  left: 0;
  width: 100%;
  height: 2px;
  background-color: var(--striking-blue);
}

label:has(input:checked) {
  color: var(--lightest);
  background-color: var(--striking-blue);
}
</style>
