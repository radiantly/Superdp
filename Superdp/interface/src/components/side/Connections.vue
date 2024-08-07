<!-- eslint-disable vue/multi-word-component-names -->
<script setup>
import { provide, shallowReactive } from "vue";
import TreeManager from "./tree/TreeManager.vue";
import SideTitle from "./SideTitle.vue";

import { VscNewFileVue, VscNewFolderVue } from "../icons";
import { clientManager } from "../../globals";

const props = defineProps({
  sideProps: {
    default: shallowReactive({}),
  },
});

provide("sideProps", props.sideProps);
</script>
<template>
  <SideTitle label="Connection manager">
    <div
      class="icon"
      @click="
        () => (sideProps.activeEntry = clientManager.createClient().entry)
      "
      title="Create a new connection"
    >
      <VscNewFileVue className="react-icon" />
    </div>
    <div
      class="icon"
      @click="() => (sideProps.activeEntry = clientManager.createDirEntry())"
      title="Create directory"
    >
      <VscNewFolderVue className="react-icon" />
    </div>
  </SideTitle>
  <TreeManager />
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
