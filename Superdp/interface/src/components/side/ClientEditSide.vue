<!-- eslint-disable vue/multi-word-component-names -->
<script setup>
import { Entry } from "../../classes/Entry";
import SideTitle from "./SideTitle.vue";
import ClientEntryEdit from "../pages/newtab/ClientEntryEdit.vue";
import { VscSaveVue } from "../icons";
import { clientManager } from "../../globals";
const props = defineProps({
  entry: {
    type: Entry,
    required: true,
  },
});
</script>
<template>
  <SideTitle :label="entry.label">
    <div
      class="icon"
      v-if="!clientManager.isSaved(entry)"
      @click="() => clientManager.root.addChild(entry)"
      title="Save"
    >
      <VscSaveVue className="react-icon" />
    </div>
  </SideTitle>
  <ClientEntryEdit v-if="!entry.isDir()" :entry="entry" :full-size="false" />
</template>
<style scoped>
.icon {
  padding: 4px;
  border-radius: 3px;
  display: flex;
  justify-content: center;
  align-items: center;
  cursor: pointer;
}
.icon:hover {
  background-color: var(--gray);
}
</style>
