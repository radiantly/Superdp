<script setup>
import { tabManagerKey } from "../../keys";
import Logo from "../icons/Logo.vue";
import LabeledTab from "./LabeledTab.vue";
import NewTab from "./NewTab.vue";
import NavBarItem from "./NavBarItem.vue";
import { inject } from "vue";
import { TabManager } from "../../classes/TabManager.js";
import {
  overlayVisible,
  webViewInForeground,
  interopQueen,
  contextMenu,
} from "../../globals";

/** @type {TabManager} */
const tabManager = inject(tabManagerKey);

const handleMouseDown = (e) => {
  console.log("Posting message", e.button);
  if (e.button === 0) interopQueen.MouseDownWindowDrag();

  // TODO: Handle context menu
  // else if (e.button === 2)
  //   chrome.webview.hostObjects.sync.interopQueen.TitlebarRightClick();
};

const handleMouseEnter = (e) => {
  webViewInForeground.value++;
  overlayVisible.value = true;
};

const handleTabClose = (...tabs) => {
  for (const tab of tabs) {
    tabManager.remove(tab);
    tab.disconnect();
  }
};

const handleContextMenu = (e, tab) => {
  const menuItems = [
    {
      label: "Close",
      handler: () => handleTabClose(tab),
    },
    {
      label: "Close all tabs",
      handler: () => handleTabClose(...tabManager.tabs),
    },
  ];

  contextMenu.show(e, menuItems);
};
</script>

<template>
  <div
    class="nav"
    @mousedown.prevent.stop="handleMouseDown"
    @contextmenu.prevent
  >
    <div class="tabs">
      <NavBarItem class="logo" @mouseenter.passive="handleMouseEnter">
        <Logo />
      </NavBarItem>
      <LabeledTab
        v-for="tab of tabManager.tabs"
        :key="tab.client.id"
        :tab="tab"
        @tabClose="() => handleTabClose(tab)"
        @contextmenu.prevent="(e) => handleContextMenu(e, tab)"
      />
      <NewTab
        v-show="tabManager.tabs.length > 0"
        @mousedown.stop="() => tabManager.setActive(TabManager.NEW_TAB)"
      />
    </div>
  </div>
</template>
<style scoped>
.nav {
  display: flex;
  background-color: #222;
}

.nav .logo {
  align-self: stretch;
  justify-content: center;
  width: 40px;
}

.nav .logo svg {
  object-fit: contain;
  width: 18px;
  height: 18px;
  fill: #ddd;
}

.nav > .tabs {
  display: flex;
  flex-wrap: wrap;
  gap: 1px;
  min-width: 0;
}
</style>
