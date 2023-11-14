<script setup>
import { tabManagerKey } from "../keys";
import { provide, ref } from "vue";
import NewTabPage from "./pages/newtab/NewTabPage.vue";
import NavBar from "./nav/NavBar.vue";
import ConnectionPage from "./pages/ConnectionPage.vue";
import { useResizeObserver } from "@vueuse/core";
import { TabManager } from "../classes/TabManager";

const props = defineProps({
  tabManager: {
    type: TabManager,
    required: true,
  },
});

provide(tabManagerKey, props.tabManager);

const workAreaElem = ref(null);

useResizeObserver(workAreaElem, () =>
  props.tabManager.setSize(workAreaElem.value.getBoundingClientRect())
);
</script>

<template>
  <NavBar />
  <div ref="workAreaElem" class="workArea">
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
