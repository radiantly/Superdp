<script setup>
import WorkArea from "./components/WorkArea.vue";
import { everythingKey } from "./keys";
import {
  provideData,
  useKeyedEventHandler,
  useWebMessages,
} from "./composables";
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
  // TODO: Create new window if tab is dragged!
  if (dragManager.props.tab instanceof Tab) {
    interopQueen.CreateNewDraggedWindow("");
  }

  dragManager.clear();
};

const handler = useKeyedEventHandler(everythingKey);

// TODO: Handle Resize
</script>

<template>
  <div
    class="everything"
    @mouseup.passive="handleMouseUp"
    @mouseleave.passive="handleMouseLeave"
    @mousemove="handler.handleEvent"
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
