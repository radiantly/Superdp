<script setup>
import { tabManagerKey, sidePropsKey } from "../keys";
import { provide, ref, shallowReactive } from "vue";
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

// if we don't use a shallowRef here, the assigned entry object is proxied
// and no longer equal to the original
const newTabSideProps = shallowReactive({});
provide(sidePropsKey, newTabSideProps);

useResizeObserver(workAreaElem, () =>
  props.tabManager.setWorkAreaSize(workAreaElem.value.getBoundingClientRect())
);
</script>

<template>
  <div ref="workAreaElem" class="workArea">
    <template v-for="tab of tabManager.tabs" :key="tab.client.id">
      <ConnectionPage :class="{ invisible: !tab.isActive.value }" :tab="tab" />
    </template>
    <NewTabPage
      :class="{ invisible: tabManager.activeTab !== TabManager.NEW_TAB }"
    />
  </div>
  <NavBar />
</template>

<style scoped>
.workArea {
  flex-grow: 1;
  min-height: 0;
  display: grid;
  grid-template-columns: 1fr;
  grid-template-rows: 1fr;
}

.workArea > * {
  grid-column: 1;
  grid-row: 1;
  min-width: 0;
  min-height: 0;
}

.invisible {
  visibility: hidden;
}
</style>
