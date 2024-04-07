<script setup>
import WorkArea from "./components/WorkArea.vue";
import { provideData } from "./composables";
import ContextMenu from "./components/contextmenu/ContextMenu.vue";
import {
  clientManager,
  contextMenu,
  dragManager,
  interopQueen,
} from "./globals";
import { Tab } from "./classes/Tab";
provideData();

const handleMouseUp = (e) => {
  dragManager.clear();
};

const handleMouseLeave = (e) => {
  // Create new window if tab is dragged
  if (dragManager.props.tab instanceof Tab)
    interopQueen.CreateNewDraggedWindow(dragManager.props.tab.id);

  dragManager.clear();
};

const handleFocusOut = (e) => {
  if (!e.relatedTarget) setTimeout(() => e.target?.focus(), 0);
};
</script>

<template>
  <div
    class="everything"
    @mouseup.passive="handleMouseUp"
    @mouseleave.passive="handleMouseLeave"
    @focusout="handleFocusOut"
  >
    <WorkArea :tab-manager="clientManager.session.children[0]" />
  </div>
  <ContextMenu :menu="contextMenu" />
</template>

<style scoped>
.everything {
  min-height: 100vh;
  max-height: 100vh;
  width: 100%;
  align-self: stretch;
  display: flex;
  flex-direction: column;
}
</style>
