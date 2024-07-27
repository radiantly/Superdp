<script setup>
import { ref } from "vue";
import CapturableDiv from "../CapturableDiv.vue";
import { useResizeObserver } from "@vueuse/core";

// Handle resizing of sidebar
const actualWidth = defineModel("width", { default: 300 });
const sidebarRef = ref(null);
const wantedWidth = ref(actualWidth.value);

useResizeObserver(sidebarRef, (entries) => {
  actualWidth.value = entries[0].contentRect.width;
});

let originalWidth = 0;
let originalClientX = 0;
const handleMouseDown = (event) => {
  originalWidth = actualWidth.value;
  originalClientX = event.clientX;
};

const handleMouseMove = (event) => {
  if (event.buttons !== 1) return;

  const deltaX = event.clientX - originalClientX;
  wantedWidth.value = originalWidth + deltaX;
};
</script>

<template>
  <div class="sidebar" ref="sidebarRef">
    <div class="sidebar-children">
      <slot></slot>
    </div>
    <CapturableDiv
      class="resize-handle"
      draggable="false"
      @mousedown="handleMouseDown"
      @mousemove="handleMouseMove"
    />
  </div>
</template>

<style scoped>
.sidebar {
  align-self: stretch;
  user-select: none;
  min-width: 10vw;
  width: v-bind("wantedWidth + 'px'");
  max-width: 50vw;
  background-color: var(--dark-gray);

  display: flex;
  align-items: stretch;
}

.sidebar-children {
  flex-grow: 1;

  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.resize-handle {
  position: relative;
  flex-shrink: 0;
  width: 4px;
  cursor: ew-resize;

  border-left: 1px solid var(--gray);
  transition: background-color 0.2s ease, border-left-color 0.2s ease;
}

.resize-handle:hover {
  background-color: var(--striking-blue);
  border-left-color: var(--striking-blue);
}
</style>
