<script setup>
import { ref } from "vue";
import CapturableDiv from "../CapturableDiv.vue";

// Handle resizing of sidebar
const width = ref(300);

const handleMouseMove = (e) => {
  if (e.buttons !== 1) return;

  // We assume that the sidebar is actually at the side of the window.
  // TODO: We will need to subtract with the offset and clamp at 0 if not
  width.value = e.x;
};
</script>

<template>
  <div class="sidebar">
    <slot></slot>
    <CapturableDiv
      class="resize-handle"
      draggable="false"
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
}

.resize-handle:hover {
  background-color: #007acc;
}
</style>
