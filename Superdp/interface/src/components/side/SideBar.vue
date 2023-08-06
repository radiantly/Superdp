<script setup>
import { ref, nextTick, inject, provide } from "vue";
import {
  workAreaKey,
  sidebarKey,
  dragKey,
  everythingKey,
  focusedItemIdSidebarKey,
} from "../../keys";
import { useKeyedEventHandler } from "../../composables";
import { v4 as uuidv4 } from "uuid";

const sidebarElem = ref(null);

// Handle resizing of sidebar
const width = ref(300);

const dragInfo = inject(dragKey);
const handleMouseDown = (e) => {
  if (e.button !== 0) return;
  dragInfo.type = "sidebar-resize";
  dragInfo.props = {
    origX: e.clientX,
    origW: width.value,
    onDragEnd: () =>
      nextTick(
        () => (width.value = sidebarElem.value.getBoundingClientRect().width)
      ),
  };
};

const handleMouseMove = (e) => {
  if (!(dragInfo?.type === "sidebar-resize") || e.button !== 0) return;
  const { origW, origX } = dragInfo.props;
  width.value = origW + e.clientX - origX;
};

useKeyedEventHandler(everythingKey).register("mousemove", handleMouseMove);

const handler = useKeyedEventHandler(sidebarKey);
</script>

<template>
  <div
    @mousemove="handler.handleEvent"
    @mouseleave="handler.handleEvent"
    @mouseup="handler.handleEvent"
    class="sidebar"
    ref="sidebarElem"
  >
    <div class="top-row">
      <div class="title">Connection manager</div>
      <!-- TODO: Add/Teleport icons here -->
    </div>
    <slot></slot>
    <div
      class="resize-handle"
      draggable="false"
      @mousedown="handleMouseDown"
    ></div>
  </div>
</template>

<style scoped>
.sidebar {
  align-self: stretch;
  user-select: none;
  min-width: 35px;
  width: v-bind("width + 'px'");
  background-color: #222;
  position: relative;
  display: flex;
  flex-direction: column;
}

.top-row {
  display: flex;
}
.title {
  flex-grow: 1;
  text-transform: uppercase;
  color: #bbb;
  font-weight: 400;
  font-size: 11px;
  padding: 12px 18px;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.resize-handle {
  position: absolute;
  top: 0;
  right: -3px;
  width: 3px;
  height: 100%;
  cursor: ew-resize;
  transition: background-color 0.25s ease;
}

.resize-handle:hover {
  background-color: #007acc;
}
</style>
