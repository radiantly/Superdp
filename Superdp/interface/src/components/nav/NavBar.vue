<script setup>
import { sidePropsKey, tabManagerKey } from "../../keys";
import LabeledTab from "./LabeledTab.vue";
import NewTab from "./NewTab.vue";
import { inject, ref } from "vue";
import { TabManager } from "../../classes/TabManager.js";
import { interopQueen, contextMenu, windowIsMaximized } from "../../globals";
import { useResizeObserver } from "@vueuse/core";
import {
  VscChromeRestoreVue,
  VscChromeMinimizeVue,
  VscChromeMaximizeVue,
} from "../icons";
import { handleMaximize, handleMinimize, handleRestore } from "../../utils";
/** @type {TabManager} */
const tabManager = inject(tabManagerKey);

const handleTabClose = (...tabs) => {
  for (const tab of tabs) {
    tabManager.remove(tab);
    tab.disconnect();
  }
};
const sideProps = inject(sidePropsKey);

const handleNewTabMouseDown = (e) => {
  // drag window if there is nothing else to do
  if (
    tabManager.props.active === TabManager.NEW_TAB &&
    sideProps.activeEntry === null
  ) {
    interopQueen.MouseDownWindowDrag();
    return;
  }
  tabManager.setActive(TabManager.NEW_TAB);
  sideProps.activeEntry = null;
};

const handleContextMenu = (e, tab) => {
  const menuItems = [];

  if (tab.props.state === "disconnected")
    menuItems.push({
      label: "Connect",
      handler: () => tab.connect(),
    });

  if (tab.props.state === "connected")
    menuItems.push({
      label: "Disconnect",
      handler: () => tab.disconnect(),
    });

  menuItems.push({
    label: tab.props.state,
    disabled: true,
  });

  if (tab.props.type === "ssh")
    menuItems.push({
      label: "Duplicate",
      handler: () => tab.client.createTab(tabManager),
    });

  menuItems.push(
    {
      label: "Close",
      handler: () => handleTabClose(tab),
    },
    {
      label: "Close all tabs",
      handler: () => handleTabClose(...tabManager.tabs),
    }
  );

  contextMenu.show(e, menuItems);
};

const navElem = ref(null);
useResizeObserver(navElem, () =>
  tabManager.setNavSize(navElem.value.getBoundingClientRect())
);
</script>

<template>
  <div class="nav" ref="navElem">
    <LabeledTab
      v-for="tab of tabManager.tabs"
      :key="tab.client.id"
      :tab="tab"
      @close="() => handleTabClose(tab)"
      @contextmenu.prevent="(e) => handleContextMenu(e, tab)"
    />
    <NewTab
      :active="tabManager.props.active === TabManager.NEW_TAB"
      @mousedown="handleNewTabMouseDown"
    />
    <div class="title-bar-btn" @click="handleMinimize">
      <VscChromeMinimizeVue className="react-icon" />
    </div>
    <div class="title-bar-btn" @click="handleRestore" v-if="windowIsMaximized">
      <VscChromeRestoreVue className="react-icon" />
    </div>
    <div class="title-bar-btn" @click="handleMaximize" v-else>
      <VscChromeMaximizeVue className="react-icon" />
    </div>
  </div>
</template>
<style scoped>
.nav {
  --tab-height: 35px;

  display: flex;
  flex-wrap: wrap;
  gap: 1px;
  min-width: 0;
  background-color: var(--gray);
  min-height: var(--tab-height);
  padding-top: 1px;
}
.nav > * {
  flex-shrink: 0;
}
.nav .spacer {
  flex-grow: 1;
  background-color: var(--darker-gray);
  /* Not supported in the stable release of webview2 yet */
  /* --webkit-app-region: drag; */
}
.nav .title-bar-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 50px;
  height: var(--tab-height);
  transition: background-color 0.1s ease;
  background-color: var(--darker-gray);
}
.nav .title-bar-btn:hover {
  background-color: var(--darke-gray);
}
</style>
