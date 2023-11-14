<script setup>
import { tabManagerKey } from "../../keys";
import LabeledTab from "./LabeledTab.vue";
import NewTab from "./NewTab.vue";
import { inject } from "vue";
import { TabManager } from "../../classes/TabManager.js";
import { overlayVisible, interopQueen, contextMenu } from "../../globals";
import NavLogo from "./NavLogo.vue";

/** @type {TabManager} */
const tabManager = inject(tabManagerKey);

const handleMouseDown = (e) => {
  console.log("Posting message", e.button);
  if (e.button === 0) interopQueen.MouseDownWindowDrag();

  // TODO: Handle context menu
  // else if (e.button === 2)
  //   chrome.webview.hostObjects.sync.interopQueen.TitlebarRightClick();
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

const handleMouseEnter = (e) => {
  // if (tabManager.props.active === TabManager.NEW_TAB) return;
  overlayVisible.value = true;
};
</script>

<template>
  <div
    class="nav"
    @mousedown.prevent.stop="handleMouseDown"
    @contextmenu.prevent
  >
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
</style>
