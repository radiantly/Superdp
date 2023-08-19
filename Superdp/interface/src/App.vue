<script setup>
import WorkArea from "./components/WorkArea.vue";
import { provideData, useWebMessages } from "./composables";
import ContextMenu from "./components/contextmenu/ContextMenu.vue";
import { contextMenu, dragManager, interopQueen } from "./globals";
import Overlay from "./components/Overlay.vue";
import { Tab } from "./classes/Tab";
provideData();
useWebMessages();

const handleMouseUp = (e) => {
  dragManager.clear();
};

const handleMouseLeave = (e) => {
  // Create new window if tab is dragged
  if (dragManager.props.tab instanceof Tab) {
    const tab = dragManager.props.tab;
    tab.props.parent.remove(tab);
    interopQueen.CreateNewDraggedWindow(JSON.stringify(tab.serializeMsg()));
  }

  dragManager.clear();
};
</script>

<template>
  <div
    class="everything"
    @mouseup.passive="handleMouseUp"
    @mouseleave.passive="handleMouseLeave"
  >
    <WorkArea />
  </div>
  <Overlay />
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
