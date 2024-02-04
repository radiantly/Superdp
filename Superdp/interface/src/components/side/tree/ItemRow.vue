<script setup>
import { inject } from "vue";
import IconCircle from "../../icons/IconCircle.vue";
import IconSortDown from "../../icons/IconSortDown.vue";
import { Entry } from "../../../classes/Entry.js";
import { DirEntry } from "../../../classes/DirEntry.js";
import { clientManager, contextMenu, dragManager } from "../../../globals";
import IconPlay from "../../icons/IconPlay.vue";
import { ClientEntry } from "../../../classes/ClientEntry";
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

const sideProps = inject("sideProps");

const chosenIcon = props.entry.isDir() ? "collapse" : "circle";
const handleMouseDown = (e) => {
  // mark active
  console.log(props.entry.root);
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
  const menuItems = {
    dir: [
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
    ],
    leaf: [
      {
        label: "Connect",
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
    ],
  };
  contextMenu.show(e, menuItems[props.entry.type]);
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
      <component :is="icons[chosenIcon]" />
    </div>
    <div class="label">
      {{ entry.label }}
    </div>
    <div class="hover-icons">
      <div
        class="icon play"
        title="Connect"
        @click="props.entry.client.createTab(clientManager.session.children[0])"
        v-show="entry instanceof ClientEntry"
      >
        <IconPlay />
      </div>
    </div>
  </div>
</template>

<style scoped>
.row {
  --row-bg: transparent;
  display: flex;
  height: 22px;
  align-items: center;
  cursor: pointer;
  position: relative;
  flex-shrink: 0;
  background: var(--row-bg);
}

.row > * {
  flex-shrink: 0;
}

.row:hover {
  --row-bg: #282828;
}

.row.active {
  --row-bg: #303030;
}

.row .label {
  user-select: none;
  margin-left: 3px;
  white-space: nowrap;
  text-overflow: clip;
  position: relative;
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

.icon.play {
  --icon-size: 17px;
  width: 30px;
  display: flex;
  justify-content: center;
  align-items: center;
}

.icon.play > svg {
  width: var(--icon-size);
  height: var(--icon-size);
  fill: #505050;
}
.icon.play:hover {
  background-color: rgba(255, 255, 255, 0.05);
}
.icon.play:hover > svg {
  fill: #aaa;
}
.hover-icons {
  position: absolute;
  top: 0;
  right: 0;
  height: 100%;
  display: flex;
  align-items: stretch;
  opacity: 0;
  background: inherit;
}
.row:hover .hover-icons {
  opacity: 1;
}
.hover-icons::before {
  content: "";
  position: absolute;
  top: 0;
  width: 15px;
  background-image: linear-gradient(to right, rgba(0, 0, 0, 0), var(--row-bg));
  height: 100%;
  right: 100%;
}
</style>
