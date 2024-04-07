<!-- eslint-disable vue/multi-word-component-names -->
<script setup>
import { computed, provide, reactive, shallowReactive } from "vue";
import { Entry } from "../../classes/Entry";
import SideTitle from "./SideTitle.vue";
import ClientEntryEdit from "../pages/newtab/ClientEntryEdit.vue";

const props = defineProps({
  entry: {
    type: Entry,
    required: true,
  },
});
</script>
<template>
  <SideTitle :label="entry.label" />
  <ClientEntryEdit v-if="!entry.isDir()" :entry="entry" :full-size="false" />
</template>
