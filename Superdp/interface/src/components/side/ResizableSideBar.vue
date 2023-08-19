<script setup>
import { ref } from "vue";
import CapturableDiv from "../CapturableDiv.vue";

const sidebarElem = ref(null);

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
  <div class="sidebar" ref="sidebarElem">
    <div class="top-row">
      <div class="title">Connection manager</div>
      <!-- TODO: Add/Teleport icons here -->
    </div>
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
