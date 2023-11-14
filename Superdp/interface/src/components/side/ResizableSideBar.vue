<script setup>
import { ref } from "vue";
import CapturableDiv from "../CapturableDiv.vue";

// Handle resizing of sidebar
const width = ref(300);
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
    <slot></slot>
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
  background-color: #222;
  position: relative;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.resize-handle {
  position: absolute;
  top: 0;
  right: 0;
  width: 3px;
  height: 100%;
  cursor: ew-resize;
  transition: background-color 0.25s ease;
  z-index: 13;
}

.resize-handle:hover {
  background-color: #007acc;
}

.sidebar::after {
  content: "";
  position: absolute;
  top: 0;
  right: 0;
  width: 15px;
  height: 100%;
  pointer-events: none;
  background: linear-gradient(to right, transparent, #222);
}
</style>
