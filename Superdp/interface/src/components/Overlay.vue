<!-- eslint-disable vue/multi-word-component-names -->
<script setup>
import { useWindowFocus } from "@vueuse/core";
import { clientManager, contextMenu, overlayVisible } from "../globals";
import NavLogo from "./nav/NavLogo.vue";
import Connections from "./side/Connections.vue";
import ResizableSideBar from "./side/ResizableSideBar.vue";
import {
  onBeforeUnmount,
  onMounted,
  shallowReactive,
  watch,
  watchEffect,
} from "vue";
import ClientEditSide from "./side/ClientEditSide.vue";
import { Entry } from "../classes/Entry";

const hide = () => {
  overlayVisible.value = false;
  contextMenu.hide();
};

const focused = useWindowFocus();

watch(focused, (isFocused) => !isFocused && hide());

// if we don't use a shallowRef here, the assigned entry object is proxied
// and no longer equal to the original
const connectionSideProps = shallowReactive({});
watchEffect(
  () => (connectionSideProps.activeEntry = connectionSideProps.focusedEntry)
);

// Reset focused entry if the entry is deleted
watchEffect(() => {
  if (!connectionSideProps.activeEntry) return;
  if (connectionSideProps.activeEntry.root === clientManager.root) return;
  connectionSideProps.activeEntry = null;
});

const handleKeyDown = (e) => {
  console.log(e);
  if (e.key === "Escape") {
    if (overlayVisible.value) {
      hide();
      e.preventDefault();
    } else if (e.shiftKey) {
      overlayVisible.value = true;
      e.preventDefault();
    }
  }
};

onMounted(() => {
  document.body.addEventListener("keydown", handleKeyDown);
});

onBeforeUnmount(() => {
  document.body.removeEventListener("keydown", handleKeyDown);
});
</script>

<template>
  <div class="overlay" :class="{ visible: overlayVisible }">
    <div class="nav-row">
      <NavLogo class="overlay-logo" />
      <div class="grow" @click="hide"></div>
    </div>
    <div class="overlay-contents">
      <ResizableSideBar>
        <Connections :side-props="connectionSideProps" />
      </ResizableSideBar>
      <ResizableSideBar v-if="connectionSideProps.activeEntry instanceof Entry">
        <ClientEditSide :entry="connectionSideProps.activeEntry" />
      </ResizableSideBar>
      <div class="grow" @click="hide"></div>
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

.nav-row {
  display: flex;
}

.grow {
  flex-grow: 1;
}

.overlay-contents {
  display: flex;
  flex-grow: 1;
  gap: 1px;
}
</style>
