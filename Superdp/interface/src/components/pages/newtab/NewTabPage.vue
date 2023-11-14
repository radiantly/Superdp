<script setup>
import ResizableSidebar from "../../side/ResizableSidebar.vue";

import { watchEffect, shallowReactive } from "vue";
import ClientEntryEdit from "./ClientEntryEdit.vue";
import { ClientEntry } from "../../../classes/ClientEntry";
import DirEntryEdit from "./DirEntryEdit.vue";
import { clientManager } from "../../../globals";
import Connections from "../../side/Connections.vue";

// if we don't use a shallowRef here, the assigned entry object is proxied
// and no longer equal to the original
const sideProps = shallowReactive({});
watchEffect(() => (sideProps.activeEntry = sideProps.focusedEntry));

// Reset focused entry if the entry is deleted
watchEffect(() => {
  if (!sideProps.activeEntry) return;
  if (sideProps.activeEntry.root === clientManager.root) return;
  sideProps.activeEntry = null;
});
</script>

<template>
  <div class="page-container">
    <ResizableSidebar>
      <Connections :side-props="sideProps" />
    </ResizableSidebar>
    <div style="flex-grow: 1">
      <DirEntryEdit
        :entry="sideProps.activeEntry"
        v-if="sideProps.activeEntry?.isDir()"
      />
      <ClientEntryEdit
        :entry="sideProps.activeEntry"
        :full-size="true"
        v-else-if="sideProps.activeEntry instanceof ClientEntry"
      />
    </div>
  </div>
</template>

<style scoped>
.page-container {
  flex-grow: 1;
  display: flex;
  min-height: 0;
  background-color: var(--dark-gray);
}
</style>
