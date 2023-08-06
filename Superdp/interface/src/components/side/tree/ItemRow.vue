<script setup>
import { inject, ref, onMounted, computed } from "vue";
import IconCircle from "../../icons/IconCircle.vue";
import IconSortDown from "../../icons/IconSortDown.vue";
import { Entry } from "../../../classes/Entry.js";
import { DirEntry } from "../../../classes/DirEntry.js";
import { focusedItemIdSidebarKey, tabManagerKey } from "../../../keys";
import { clientManager, dragManager } from "../../../globals";
import { useContextMenu } from "../../contextmenu/ContextMenuHelper";
const props = defineProps({
  entry: {
    type: Entry,
    required: true,
  },
  indent: {
    type: String,
    required: true,
  },
});

const icons = { circle: IconCircle, collapse: IconSortDown };

const focusedEntry = inject(focusedItemIdSidebarKey);

const chosenIcon = props.entry.isDir() ? "collapse" : "circle";
const handleMouseDown = (e) => {
  // mark active
  console.log(props.entry.root);
  focusedEntry.value = props.entry;
};

const validDrop = inject("validDrop");

const handleClick = (e) => {
  if (props.entry.isDir()) props.entry.toggleCollapse();
};

const handleMouseLeave = (e) => {
  // Start drag
  if (dragManager.props.isDragging) return;
  dragManager.start(e, (dragProps) => {
    dragManager.props.entry = props.entry;
    if (props.entry.isDir() && !props.entry.props.collapsed) {
      props.entry.toggleCollapse();
      dragProps.undoCollapse = true;
    }
  });
};

const handleMouseUp = (e) => {
  console.log("Mouse up called.");
  if (!validDrop.value) return;

  dragManager.end(e, (dragProps) => {
    if (props.entry === dragProps.entry) return;
    const dirEntry = props.entry.isDir() ? props.entry : props.entry.parent;
    dragProps.entry.parent.removeChild(dragProps.entry);
    dirEntry.addChild(dragProps.entry);

    if (dragProps.undoCollapse) dragProps.entry.toggleCollapse();
  });
};

const handleDrop = (e) => {
  console.log(props.entry, e);
};

const menu = useContextMenu();
const tabManager = inject(tabManagerKey);
const handleContextMenu = (e) => {
  const menuItems = {
    dir: [
      {
        label: "New connection...",
        handler: () =>
          (focusedEntry.value = clientManager.createClient({
            parentEntry: props.entry,
          }).entry),
      },
      {
        label: "New directory group...",
        handler: () =>
          (focusedEntry.value = new DirEntry({
            manager: clientManager,
            parentEntry: props.entry,
          })),
      },
      {
        label: "Delete",
        handler: () => props.entry.parent.removeChild(props.entry),
      },
    ],
    leaf: [
      {
        label: "Connect",
        handler: () => props.entry.client.createOrSwitchToTab(tabManager),
      },
      {
        label: "Duplicate",
        handler: () =>
          (focusedEntry.value = clientManager.createClient({
            parentEntry: props.entry.parent,
            clientProps: props.entry.client.props,
          }).entry),
      },
      {
        label: "Delete",
        handler: () => props.entry.parent.removeChild(props.entry),
      },
    ],
  };
  menu.show(e, menuItems[props.entry.type]);
};
</script>

<template>
  <div
    class="row"
    @mousedown.passive="handleMouseDown"
    @mouseleave.passive="handleMouseLeave"
    @mouseup.passive="handleMouseUp"
    @click="handleClick"
    @contextmenu.stop="handleContextMenu"
    @dragover.prevent
    @drop="handleDrop"
    :class="{
      collapsed: entry.props?.collapsed,
      active: entry === focusedEntry,
      [props.entry.type]: true,
    }"
  >
    <div :style="{ width: indent }"></div>
    <div class="icon" :class="chosenIcon">
      <component :is="icons[chosenIcon]" />
    </div>
    <div class="label">
      {{ entry.isDir() ? entry.props.name : entry.client.label.value }}
    </div>
  </div>
</template>

<style scoped>
.row {
  display: flex;
  height: 22px;
  align-items: center;
  cursor: pointer;
  position: relative;
  flex-shrink: 0;
}

.row > * {
  flex-shrink: 0;
}

.row:hover {
  background-color: #282828;
}

.row.active {
  background-color: #303030;
}

.row .label {
  user-select: none;
  margin-left: 3px;
  white-space: nowrap;
}

.icon {
  fill: #444;
  width: 13px;
  display: flex;
}

.icon.collapse {
  padding-top: 1px;
}

.icon.collapse > svg {
  width: 13px;
  height: 13px;
  transition: transform 0.2s ease;
}

.icon.circle {
  padding-left: 1px;
}

.icon.circle > svg {
  width: 10px;
  height: 10px;
}

.collapsed > .icon.collapse > svg {
  transform: rotate(-90deg);
}
</style>
