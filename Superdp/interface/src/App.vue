<script setup>
import WorkArea from "./components/WorkArea.vue";
import { everythingKey } from "./keys";
import {
  provideData,
  useKeyedEventHandler,
  useWebMessages,
} from "./composables";
import { useContextMenu } from "./components/contextmenu/ContextMenuHelper";
import ContextMenu from "./components/contextmenu/ContextMenu.vue";
import { dragManager } from "./globals";
import Overlay from "./components/Overlay.vue";
provideData();
useWebMessages();

// const clients = reactive({});

// const tabs = reactive({
//   NEW_TAB: {
//     component: NewTabPage,
//   },
// });
// tabs.active = tabs.NEW_TAB;
// provide(tabsKey, tabs);

const contextMenuHelper = useContextMenu({ provide: true });

const handleMouseUp = (e) => {
  dragManager.clear();
};

const handleMouseLeave = (e) => {
  // TODO: Create new window if tab is dragged!

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
    @mousedown="contextMenuHelper.hide"
    @mousemove="handler.handleEvent"
  >
    <WorkArea />
  </div>
  <Overlay />
  <ContextMenu :menu="contextMenuHelper.menu" />
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
