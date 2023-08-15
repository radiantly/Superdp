<!-- eslint-disable vue/multi-word-component-names -->
<script setup>
import { overlayVisible, webViewInForeground } from "../globals";
const handleClick = (e) => {
  webViewInForeground.value--;
  overlayVisible.value = false;
};
</script>

<template>
  <div
    class="overlay"
    :class="{ visible: overlayVisible }"
    @click="handleClick"
  ></div>
</template>

<style scoped>
.overlay {
  position: fixed;
  z-index: 666;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  pointer-events: none;
  opacity: 0;
  transition: opacity 0.2s ease;
}

.overlay.visible {
  opacity: 1;
  pointer-events: auto;
}
</style>
