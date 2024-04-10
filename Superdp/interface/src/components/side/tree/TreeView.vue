<script setup>
import { DirEntry } from "../../../classes/DirEntry";
import ItemRow from "./ItemRow.vue";

const props = defineProps({
  tree: {
    type: DirEntry,
    required: true,
  },
  depth: {
    type: Number,
    default: 0,
  },
});

// these values are just set by hand
const indent = `${16 + props.depth * 17}px`;
const lineIndent = `${16 + props.depth * 17 - 10}px`;
</script>

<template>
  <div class="entries" :class="{ root: !depth }" v-show="!tree.props.collapsed">
    <template v-for="entry in tree.children.value" :key="entry.id">
      <ItemRow :entry="entry" :indent="indent" />
      <TreeView
        v-if="entry.isDir()"
        :tree="entry"
        :depth="depth + 1"
      ></TreeView>
    </template>
  </div>
</template>

<style scoped>
.entries {
  display: flex;
  flex-direction: column;
  position: relative;
}

/* This is the thin line that indicates an expanded dir */
.entries::after {
  content: "";
  position: absolute;
  top: 0;
  left: v-bind(lineIndent);
  height: 100%;
  width: 1px;
  background-color: var(--light-gray);
}

.entries.root::after {
  opacity: 0;
}
</style>
