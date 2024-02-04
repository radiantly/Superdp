<script setup>
import { tabManagerKey } from "../../keys";
import LabeledTab from "./LabeledTab.vue";
import NewTab from "./NewTab.vue";
import { inject, ref } from "vue";
import { TabManager } from "../../classes/TabManager.js";
import {
  overlayVisible,
  interopQueen,
  contextMenu,
  windowIsMaximized,
} from "../../globals";
import NavLogo from "./NavLogo.vue";
import { useResizeObserver } from "@vueuse/core";
import { VscChromeRestore, VscChromeMinimize } from "react-icons/vsc";
import { applyPureReactInVue } from "veaury";
/** @type {TabManager} */
const tabManager = inject(tabManagerKey);

const handleMouseDown = (e) => {
  if (e.button === 0) interopQueen.MouseDownWindowDrag();
};

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

const handleMouseEnter = (e) => {
  // if (tabManager.props.active === TabManager.NEW_TAB) return;
  overlayVisible.value = true;
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
      <NavLogo @mouseenter.passive="handleMouseEnter" />
      <LabeledTab
        v-for="tab of tabManager.tabs"
        :key="tab.client.id"
        :tab="tab"
        @close="() => handleTabClose(tab)"
        @contextmenu.prevent="(e) => handleContextMenu(e, tab)"
      />
      <NewTab
        v-show="tabManager.tabs.length > 0"
        @mousedown.stop="() => tabManager.setActive(TabManager.NEW_TAB)"
      />
    </div>
    <div class="spacer" @mousedown.passive="handleMouseDown"></div>
    <div
      class="title-bar-btn"
      @click="handleMinimize"
      v-show="windowIsMaximized"
    >
      <VscChromeMinimizeVue />
    </div>
    <div
      class="title-bar-btn"
      @click="handleRestore"
      v-show="windowIsMaximized"
    >
      <VscChromeRestoreVue />
    </div>
  </div>
</template>
<style scoped>
.nav {
  display: flex;
  background-color: #222;
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
  transition: all 0.1s ease;
}
.nav > .title-bar-btn:hover {
  background-color: #444;
}
</style>
