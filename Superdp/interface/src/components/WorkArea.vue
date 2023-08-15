<script setup>
import { useKeyedEventHandler } from "../composables";
import { tabManagerKey, workAreaKey } from "../keys";
import {
  computed,
  inject,
  onBeforeUnmount,
  onMounted,
  provide,
  ref,
  shallowReactive,
} from "vue";
import NewTabPage from "./pages/newtab/NewTabPage.vue";
import NavBar from "./nav/NavBar.vue";
import ConnectionPage from "./pages/ConnectionPage.vue";
import { TabManager } from "../classes/TabManager.js";

const handler = useKeyedEventHandler(workAreaKey);

const tabManager = new TabManager();

provide(tabManagerKey, tabManager);

const workAreaElem = ref(null);
let observer;
onMounted(() => {
  observer = new ResizeObserver(() => {
    tabManager.setSize(workAreaElem.value.getBoundingClientRect());
  });
  observer.observe(workAreaElem.value);
});
onBeforeUnmount(() => {
  observer.disconnect();
});
</script>

<template>
  <NavBar />
  <div
    ref="workAreaElem"
    class="workArea"
    @mousemove="handler.handleEvent"
    @mouseup="handler.handleEvent"
    @mouseleave="handler.handleEvent"
  >
    <template v-for="tab of tabManager.tabs" :key="tab.client.id">
      <ConnectionPage v-show="tab.isActive.value" :tab="tab" />
    </template>
    <NewTabPage v-show="tabManager.props.active === null" />
  </div>
</template>

<style scoped>
.workArea {
  flex-grow: 1;
  display: flex;
  min-height: 0;
}
</style>
