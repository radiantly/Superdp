<script setup>
import { watchEffect, inject } from "vue";

import { clientManager } from "../../../globals";
import { ClientEntry } from "../../../classes/ClientEntry";
import ClientEntryEdit from "./ClientEntryEdit.vue";
import DirEntryEdit from "./DirEntryEdit.vue";
import Connections from "../../side/Connections.vue";
import ResizableSideBar from "../../side/ResizableSideBar.vue";
import DefaultWelcome from "./DefaultWelcome.vue";
import { sidePropsKey } from "../../../keys";

const sideProps = inject(sidePropsKey);

// Reset focused entry if the entry is deleted
watchEffect(() => {
  if (sideProps.activeEntry?.root === clientManager.root) return;
  sideProps.activeEntry = null;
});
</script>

<template>
  <div class="page-container">
    <ResizableSideBar>
      <Connections :side-props="sideProps" />
    </ResizableSideBar>
    <DirEntryEdit
      :entry="sideProps.activeEntry"
      v-if="sideProps.activeEntry?.isDir()"
      class="editor"
    />
    <ClientEntryEdit
      :entry="sideProps.activeEntry"
      :full-size="true"
      v-else-if="sideProps.activeEntry instanceof ClientEntry"
      class="editor"
    />
    <DefaultWelcome v-else class="editor" />
  </div>
</template>

<style scoped>
.page-container {
  flex-grow: 1;
  display: flex;
  min-height: 0;
  background-color: var(--dark-gray);
}
.editor {
  flex-grow: 1;
}
</style>
