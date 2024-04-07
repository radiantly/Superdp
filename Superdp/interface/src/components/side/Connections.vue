<!-- eslint-disable vue/multi-word-component-names -->
<script setup>
import { provide, shallowReactive } from "vue";
import TreeManager from "./tree/TreeManager.vue";
import SideTitle from "./SideTitle.vue";

import { VscNewFile, VscNewFolder } from "react-icons/vsc";
import { applyPureReactInVue } from "veaury";
import { clientManager } from "../../globals";
import { DirEntry } from "../../classes/DirEntry";
const VscNewFileVue = applyPureReactInVue(VscNewFile);
const VscNewFolderVue = applyPureReactInVue(VscNewFolder);

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
        () => (sideProps.focusedEntry = clientManager.createClient().entry)
      "
    >
      <VscNewFileVue className="react-icon" />
    </div>
    <div
      class="icon"
      @click="
        () =>
          (sideProps.focusedEntry = new DirEntry({
            manager: clientManager,
            parentEntry: clientManager.root,
          }))
      "
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
