<script setup>
import { inject } from "vue";
import { Entry } from "../../../classes/Entry.js";
import { DirEntry } from "../../../classes/DirEntry.js";
import { clientManager, contextMenu, dragManager } from "../../../globals";
import { VscChevronDownVue, VscCircleFilledVue } from "../../icons";

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

const icons = { circle: VscCircleFilledVue, collapse: VscChevronDownVue };

const sideProps = inject("sideProps");

const chosenIcon = props.entry.isDir() ? "collapse" : "circle";
const handleMouseDown = (e) => {
  sideProps.focusedEntry = props.entry;
};

const validDrop = inject("validDrop");

const handleClick = (e) => {
  if (props.entry.isDir()) props.entry.toggleCollapse();
};

const handleMouseEnter = (e) => {
  sideProps.hoveredEntry = props.entry;
};

const handleMouseLeave = (e) => {
  // Start drag
  if (dragManager.props.isDragging) return;
  dragManager.start(e, (dragProps) => {
    dragProps.entry = props.entry;
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

const handleContextMenu = (e) => {
  const menuItems = props.entry.isDir()
    ? [
        {
          label: "New connection...",
          handler: () =>
            (sideProps.focusedEntry = clientManager.createClient({
              parentEntry: props.entry,
            }).entry),
        },
        {
          label: "New directory group...",
          handler: () =>
            (sideProps.focusedEntry = new DirEntry({
              manager: clientManager,
              parentEntry: props.entry,
            })),
        },
        {
          label: "Delete",
          handler: () => props.entry.parent.removeChild(props.entry),
        },
      ]
    : [
        {
          label: "Connect",
          disabled: !props.entry.client.valid.value,
          handler: () =>
            props.entry.client.createTab(clientManager.session.children[0]),
        },
        {
          label: "Duplicate",
          handler: () =>
            (sideProps.focusedEntry = clientManager.createClient({
              parentEntry: props.entry.parent,
              clientProps: props.entry.client.props,
            }).entry),
        },
        {
          label: "Delete",
          handler: () => props.entry.parent.removeChild(props.entry),
        },
      ];
  contextMenu.show(e, menuItems);
};
</script>

<template>
  <div
    class="row"
    @mousedown.passive="handleMouseDown"
    @mouseenter.passive="handleMouseEnter"
    @mouseleave.passive="handleMouseLeave"
    @mouseup.passive="handleMouseUp"
    @click="handleClick"
    @contextmenu.stop="handleContextMenu"
    @dragover.prevent
    @drop="handleDrop"
    :class="{
      collapsed: entry.props?.collapsed,
      active: entry === sideProps.activeEntry,
      [props.entry.type]: true,
    }"
  >
    <div :style="{ width: indent }"></div>
    <div class="icon" :class="chosenIcon">
      <component className="react-icon" :is="icons[chosenIcon]" />
    </div>
    <div class="label">
      {{ entry.label }}
    </div>
  </div>
</template>

<style scoped>
.row {
  --bg-color: transparent;
  --text-color: var(--light);
  display: flex;
  height: 22px;
  align-items: center;
  cursor: pointer;
  position: relative;
  flex-shrink: 0;
  background: var(--bg-color);
  color: var(--text-color);
}

.row > * {
  flex-shrink: 0;
}

.row:hover {
  --bg-color: var(--da-gray);
}

.row.active {
  --bg-color: var(--gray);
  --text-color: var(--lightest);
}

.row .label {
  user-select: none;
  margin-left: 3px;
  white-space: nowrap;
  text-overflow: clip;
  position: relative;
}

.icon {
  fill: var(--light-gray);
  display: flex;
}

.icon {
  transition: transform 0.2s ease;
}

.icon.circle {
  color: var(--lighter-gray);
}

.collapsed > .icon.collapse {
  transform: rotate(-90deg);
}
</style>
