<script setup>
import TreeView from "./TreeView.vue";
import { inject, computed, provide } from "vue";
import { clientManager, contextMenu, dragManager } from "../../../globals";
import { DirEntry } from "../../../classes/DirEntry";
import { Entry } from "../../../classes/Entry";

const sideProps = inject("sideProps");

const handleContextMenu = (e) => {
  contextMenu.show(e, [
    {
      label: "New connection...",
      handler: () =>
        (sideProps.activeEntry = clientManager.createClient().entry),
    },
    {
      label: "New directory group...",
      handler: () => (sideProps.activeEntry = clientManager.createDirEntry()),
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
    @click.self="() => (sideProps.activeEntry = null)"
  />
</template>

<style scoped>
.tree {
  min-height: 0;
  overflow: hidden auto;
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
