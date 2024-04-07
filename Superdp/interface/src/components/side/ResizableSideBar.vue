<script setup>
import { ref } from "vue";
import CapturableDiv from "../CapturableDiv.vue";

// Handle resizing of sidebar
const width = defineModel("width", { default: 300 });
const sidebar = ref(null);

let offsetLeft = 0;
const handleMouseDown = (e) => {
  offsetLeft = sidebar.value.getBoundingClientRect().left;
};

const handleMouseMove = (e) => {
  if (e.buttons !== 1) return;

  width.value = Math.max(1, e.x - offsetLeft);
};
</script>

<template>
  <div class="sidebar" ref="sidebar">
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
  width: v-bind("width + 'px'");
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
  width: 4px;
  cursor: ew-resize;

  border-right: 1px solid var(--gray);
  transition: background-color 0.2s ease, border-right-color 0.2s ease;
}

.resize-handle:hover {
  background-color: var(--striking-blue);
  border-right-color: var(--striking-blue);
}
</style>
