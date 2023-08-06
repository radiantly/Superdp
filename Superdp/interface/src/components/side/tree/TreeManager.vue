<script setup>
import TreeView from "./TreeView.vue";
import { inject, computed, provide } from "vue";
import { focusedItemIdSidebarKey } from "../../../keys";
import { useContextMenu } from "../../contextmenu/ContextMenuHelper";
import { clientManager, dragManager } from "../../../globals";
import { DirEntry } from "../../../classes/DirEntry";
import { Entry } from "../../../classes/Entry";

const menu = useContextMenu();
const focusedEntry = inject(focusedItemIdSidebarKey);

const handleContextMenu = (e) => {
  menu.show(e, [
    {
      label: "New connection...",
      handler: () => (focusedEntry.value = clientManager.createClient().entry),
    },
    {
      label: "New directory group...",
      handler: () =>
        (focusedEntry.value = new DirEntry({
          manager: clientManager,
          parentEntry: clientManager.root,
        })),
    },
  ]);
};

const validDrag = computed(() => dragManager.props.entry instanceof Entry);
provide("validDrop", validDrag);
</script>

<template>
  <TreeView
    @contextmenu="handleContextMenu"
    class="tree"
    :class="{ 'drag-active': validDrag }"
    :tree="clientManager.root"
  />
</template>

<style scoped>
.tree {
  min-height: 0;
  overflow-y: auto;
  flex-grow: 1;
}

/* For drop */
/* Of course, I could have used some js, but what's the fun in that */
.tree.drag-active :deep(.dir:hover),
.tree.drag-active :deep(.dir:hover + .entries),
.tree.drag-active :deep(.entries:has(> .leaf:hover)),
.tree.drag-active :deep(.dir:has(+ .entries > .leaf:hover)) {
  background-color: #282828;
}
</style>
