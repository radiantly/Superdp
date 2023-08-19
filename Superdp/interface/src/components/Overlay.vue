<!-- eslint-disable vue/multi-word-component-names -->
<script setup>
import { useWindowFocus } from "@vueuse/core";
import { contextMenu, overlayVisible, webViewInForeground } from "../globals";
import NavLogo from "./nav/NavLogo.vue";
import Connections from "./side/Connections.vue";
import ResizableSideBar from "./side/ResizableSideBar.vue";
import { watch } from "vue";
const hide = () => {
  // TODO: A race condition may be possible?
  webViewInForeground.value--;
  overlayVisible.value = false;
  contextMenu.hide();
};

const focused = useWindowFocus();

watch(focused, (isFocused) => !isFocused && hide());
</script>

<template>
  <div class="overlay" :class="{ visible: overlayVisible }" @click="hide">
    <NavLogo class="overlay-logo" />
    <div class="overlay-contents">
      <ResizableSideBar>
        <Connections />
      </ResizableSideBar>
    </div>
  </div>
</template>

<style scoped>
.overlay {
  position: fixed;
  display: flex;
  flex-direction: column;
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

.overlay-logo {
  background-color: #222;
}

.overlay-contents {
  display: flex;
  flex-grow: 1;
}
</style>
