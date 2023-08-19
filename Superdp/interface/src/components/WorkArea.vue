<script setup>
import { useTabManager } from "../composables";
import { tabManagerKey } from "../keys";
import { onBeforeUnmount, onMounted, provide, ref } from "vue";
import NewTabPage from "./pages/newtab/NewTabPage.vue";
import NavBar from "./nav/NavBar.vue";
import ConnectionPage from "./pages/ConnectionPage.vue";

const tabManager = useTabManager();

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
