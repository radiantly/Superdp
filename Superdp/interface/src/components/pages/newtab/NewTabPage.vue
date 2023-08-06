<script setup>
import TreeManager from "../../side/tree/TreeManager.vue";
import Sidebar from "../../side/Sidebar.vue";
// import { measureText } from "../utils.js";

import { provide, shallowRef, computed, watchEffect } from "vue";
import { focusedItemIdSidebarKey } from "../../../keys";
import ClientEntryEdit from "./ClientEntryEdit.vue";
import { ClientEntry } from "../../../classes/ClientEntry";
import { DirEntry } from "../../../classes/DirEntry";
import DirEntryEdit from "./DirEntryEdit.vue";
import { clientManager } from "../../../globals";

// if we don't use a shallowRef here, the assigned entry object is proxied
// and no longer equal to the original
const focusedEntry = shallowRef(null);

// Reset focused entry if the entry is deleted
watchEffect(() => {
  if (!focusedEntry.value) return;
  if (focusedEntry.value.root === clientManager.root) return;
  focusedEntry.value = null;
});

provide(focusedItemIdSidebarKey, focusedEntry);
</script>

<template>
  <div class="page-container">
    <Sidebar>
      <TreeManager />
    </Sidebar>
    <div style="flex-grow: 1">
      <DirEntryEdit :entry="focusedEntry" v-if="focusedEntry?.isDir()" />
      <ClientEntryEdit
        :entry="focusedEntry"
        v-else-if="focusedEntry instanceof ClientEntry"
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
