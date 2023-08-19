<script setup>
import { useTabManager } from "../composables";
import { tabManagerKey } from "../keys";
import { provide, ref } from "vue";
import NewTabPage from "./pages/newtab/NewTabPage.vue";
import NavBar from "./nav/NavBar.vue";
import ConnectionPage from "./pages/ConnectionPage.vue";
import { useResizeObserver } from "@vueuse/core";

// TODO: Refactor this. Currently, the view is creating the state. Needs to be
// the other way around.
const tabManager = useTabManager();
provide(tabManagerKey, tabManager);

const workAreaElem = ref(null);

useResizeObserver(workAreaElem, () =>
  tabManager.setSize(workAreaElem.value.getBoundingClientRect())
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
