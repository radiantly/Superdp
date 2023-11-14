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

const client = computed(() => props.entry.client);
</script>
<template>
  <div class="client-col">
    <SideTitle :label="entry.label" />
    <ClientEntryEdit v-if="!entry.isDir()" :entry="entry" :full-size="false" />
  </div>
</template>
