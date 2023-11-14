<!-- eslint-disable vue/multi-word-component-names -->
<script setup>
import { useWindowFocus } from "@vueuse/core";
import { contextMenu, overlayVisible } from "../globals";
import NavLogo from "./nav/NavLogo.vue";
import Connections from "./side/Connections.vue";
import ResizableSideBar from "./side/ResizableSideBar.vue";
import { shallowReactive, watch, watchEffect } from "vue";
import ClientEditSide from "./side/ClientEditSide.vue";
import { ClientEntry } from "../classes/ClientEntry";
import { Entry } from "../classes/Entry";
import { vOnClickOutside } from "@vueuse/components";
import { computed } from "@vue/reactivity";

const hide = () => {
  overlayVisible.value = false;
  contextMenu.hide();
};

const focused = useWindowFocus();

watch(focused, (isFocused) => !isFocused && hide());

const connectionSideProps = shallowReactive({});
const activeEntry = computed(
  () => connectionSideProps.focusedEntry || connectionSideProps.hoveredEntry
);
</script>

<template>
  <div class="overlay" :class="{ visible: overlayVisible }">
    <NavLogo class="overlay-logo" />
    <div class="overlay-contents" v-on-click-outside="hide">
      <ResizableSideBar>
        <Connections :side-props="connectionSideProps" />
      </ResizableSideBar>
      <ResizableSideBar v-if="activeEntry instanceof Entry">
        <ClientEditSide :entry="activeEntry" />
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
  gap: 1px;
  align-self: flex-start;
}
</style>
