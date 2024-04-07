<script setup>
import { tabManagerKey } from "../../keys";
import LabeledTab from "./LabeledTab.vue";
import NewTab from "./NewTab.vue";
import { inject, ref } from "vue";
import { TabManager } from "../../classes/TabManager.js";
import { interopQueen, contextMenu, windowIsMaximized } from "../../globals";
import { useResizeObserver } from "@vueuse/core";
import { VscChromeRestore, VscChromeMinimize } from "react-icons/vsc";
import { applyPureReactInVue } from "veaury";
/** @type {TabManager} */
const tabManager = inject(tabManagerKey);

const handleTabClose = (...tabs) => {
  for (const tab of tabs) {
    tabManager.remove(tab);
    tab.disconnect();
  }
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

const handleMinimize = () => {
  interopQueen.Minimize();
};
const handleRestore = () => {
  interopQueen.Restore();
};
const VscChromeRestoreVue = applyPureReactInVue(VscChromeRestore);
const VscChromeMinimizeVue = applyPureReactInVue(VscChromeMinimize);
</script>

<template>
  <div class="nav" ref="navElem">
    <div class="tabs">
      <LabeledTab
        v-for="tab of tabManager.tabs"
        :key="tab.client.id"
        :tab="tab"
        @close="() => handleTabClose(tab)"
        @contextmenu.prevent="(e) => handleContextMenu(e, tab)"
      />
      <NewTab
        v-show="tabManager.tabs.length"
        @mousedown.stop="() => tabManager.setActive(TabManager.NEW_TAB)"
      />
    </div>
    <div
      class="spacer"
      @dragover.prevent=""
      @drop.prevent="(e) => console.log(e.dataTransfer.getData('text/plain'))"
    ></div>
    <div
      class="title-bar-btn"
      @click="handleMinimize"
      v-show="windowIsMaximized"
    >
      <VscChromeMinimizeVue className="react-icon" />
    </div>
    <div
      class="title-bar-btn"
      @click="handleRestore"
      v-show="windowIsMaximized"
    >
      <VscChromeRestoreVue className="react-icon" />
    </div>
  </div>
</template>
<style scoped>
.nav {
  --tab-height: 35px;

  display: flex;
  background-color: var(--dark-gray);
  min-height: var(--tab-height);
}

.nav > .tabs {
  display: flex;
  flex-wrap: wrap;
  gap: 1px;
  min-width: 0;
}
.nav > .spacer {
  flex-grow: 1;
}
.nav > .title-bar-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 50px;
  transition: background-color 0.1s ease;
}
.nav > .title-bar-btn:hover {
  background-color: var(--da-gray);
}
</style>
