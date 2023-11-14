<!-- eslint-disable vue/multi-word-component-names -->
<script setup>
import { provide, reactive, shallowReactive } from "vue";
import TreeManager from "./tree/TreeManager.vue";
import SideTitle from "./SideTitle.vue";

const props = defineProps({
  sideProps: {
    default: shallowReactive({}),
  },
});

provide("sideProps", props.sideProps);
</script>
<template>
  <SideTitle label="Connection manager" />
  <!-- TODO: Add/Teleport icons here -->
  <TreeManager />
</template>
