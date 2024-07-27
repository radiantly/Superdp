<script setup>
import { sidePropsKey, tabManagerKey } from "../../keys";
import NavLabeledTab from "./NavLabeledTab.vue";
import NavNewTab from "./NavNewTab.vue";
import { inject, ref, shallowReactive } from "vue";
import { TabManager } from "../../classes/TabManager.js";
import { contextMenu, windowIsMaximized } from "../../globals";
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

const handleTabMouseDown = (tab) => {
  if (tab === TabManager.NEW_TAB) sideProps.activeEntry = null;
  tabManager.setActive(tab);
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

//// Drag
const currentDragOver = shallowReactive({ index: -1 });
const resetCurrentDragOver = () => (currentDragOver.index = -1);

const handleDragStart = (e, tab) => {
  e.dataTransfer.setData("superdp/tab", tab.id);
  console.log(tab.id);
};

const handleDragOver = (e, index) => {
  if (!e.dataTransfer.types.includes("superdp/tab")) return;
  e.preventDefault();
  // console.log(e.offsetX, e.currentTarget.clientWidth);
  if (e.offsetX > e.currentTarget.clientWidth / 2) index += 1;
  currentDragOver.index = index;
};

// this counter tracks events triggered from children
let dragCounter = 0;

const handleDrop = (e) => {
  e.preventDefault();
  dragCounter = 0;

  // check if we actually have a drop index
  if (currentDragOver.index < 0) return;
  let targetIndex = currentDragOver.index;
  resetCurrentDragOver();

  const tabId = e.dataTransfer.getData("superdp/tab");
  if (tabId) {
    // check if same tab manager
    const tabIndex = tabManager.tabs.findIndex((tab) => tab.id === tabId);
    if (tabIndex !== -1) {
      // remove tab from tabManager
      const tab = tabManager.tabs.splice(tabIndex, 1)[0];

      // subtract index by 1 if it is after the removed element
      console.log(targetIndex);
      targetIndex -= targetIndex > tabIndex;
      targetIndex = Math.min(tabManager.tabs.length, targetIndex);

      console.log(targetIndex, tabIndex, tab?.id);
      tabManager.tabs.splice(targetIndex, 0, tab);
    }
  }
};

const handleDragEnter = () => {
  dragCounter += 1;
};
const handleDragLeave = () => {
  dragCounter -= 1;
  if (dragCounter === 0) currentDragOver.index = -1;
};
</script>

<template>
  <div class="nav" ref="navElem" @drop="handleDrop">
    <NavLabeledTab
      v-for="(tab, index) of tabManager.tabs"
      :key="tab.id"
      :tab="tab"
      :active="tab.isActive.value"
      :connected="tab.props.state === 'connected'"
      :highlightLeft="currentDragOver.index === index"
      @close="() => handleTabClose(tab)"
      @mousedown.passive="() => handleTabMouseDown(tab)"
      @contextmenu.prevent="(e) => handleContextMenu(e, tab)"
      draggable="true"
      @dragstart="(e) => handleDragStart(e, tab)"
      @dragover="(e) => handleDragOver(e, index)"
      @dragenter="handleDragEnter"
      @dragleave="handleDragLeave"
    />
    <div class="action-bar">
      <NavNewTab
        :active="tabManager.activeTab === TabManager.NEW_TAB"
        :highlightLeft="currentDragOver.index >= tabManager.tabs.length"
        @mousedown.passive="() => handleTabMouseDown(TabManager.NEW_TAB)"
        @dragover="(e) => handleDragOver(e, tabManager.tabs.length)"
      />
      <div class="title-bar-btn" @click="handleMinimize">
        <VscChromeMinimizeVue className="react-icon" />
      </div>
      <div
        class="title-bar-btn"
        @click="handleRestore"
        v-if="windowIsMaximized"
      >
        <VscChromeRestoreVue className="react-icon" />
      </div>
      <div class="title-bar-btn" @click="handleMaximize" v-else>
        <VscChromeMaximizeVue className="react-icon" />
      </div>
    </div>
  </div>
</template>
<style scoped>
.nav {
  --tab-height: 35px;

  flex-shrink: 0;
  min-height: var(--tab-height);
  min-width: 0;

  background-color: var(--gray);

  display: flex;
  flex-wrap: wrap;
  gap: 1px;
  padding-top: 1px;
}
.nav .action-bar {
  flex: 42 0 auto;

  display: flex;
  gap: 1px;
}
.nav .title-bar-btn {
  flex: 0 0 auto;
  width: 50px;
  height: var(--tab-height);

  display: flex;
  align-items: center;
  justify-content: center;
  transition: background-color 0.1s ease;
  background-color: var(--darker-gray);
}
.nav .title-bar-btn:hover {
  background-color: var(--darke-gray);
}
</style>
