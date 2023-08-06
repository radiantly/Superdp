<script setup>
import { computed, inject } from "vue";
import { DirEntry } from "../../../classes/DirEntry";
import { tabManagerKey } from "../../../keys";
import LargeInput from "./LargeInput.vue";

const props = defineProps({
  entry: {
    type: DirEntry,
    required: true,
  },
});

const dirProps = computed(() => props.entry.props);
</script>

<template>
  <form>
    <LargeInput v-model="dirProps.name" placeholder="Untitled directory" />
  </form>
</template>

<style scoped>
form {
  display: flex;
  flex-direction: column;
  gap: 30px;
  padding: 35px 50px;
}

form input {
  border: none;
  outline: none;
  min-width: 0;
  font-family: var(--ui-font);
  width: 0;
}
</style>
