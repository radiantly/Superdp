<script setup>
import { VscChromeClose } from "react-icons/vsc";
import { applyPureReactInVue } from "veaury";

import { Tab } from "../../classes/Tab";
import { dragManager, interopQueen } from "../../globals";
import NavBarItem from "./NavBarItem.vue";
const VscChromeCloseVue = applyPureReactInVue(VscChromeClose);
const props = defineProps({
  tab: {
    type: Tab,
    required: true,
  },
});

defineEmits(["close"]);

const tabManager = props.tab.props.parent;

const handleMouseDown = (e) => {
  if (e.button !== 0) return; // ignore if not primary mouse button
  tabManager.setActive(props.tab);

  // If this is the only tab, then drag entire window
  if (tabManager.tabs.length === 1)
    interopQueen.MouseDownWindowDragWithTab(props.tab.id);
};

const handleMouseLeave = (e) => {
  // Start drag
  if (dragManager.props.isDragging) return;
  dragManager.start(e, (dragProps) => {
    dragProps.tab = props.tab;
  });
};
</script>

<template>
  <NavBarItem
    class="labeled-tab"
    :class="{
      active: tab.isActive.value,
      connected: tab.props.state === 'connected',
    }"
    @mousedown.passive="handleMouseDown"
    @mouseleave.passive="handleMouseLeave"
    draggable="true"
    @dragstart="(e) => e.dataTransfer.setData('text/plain', 'hiiiiii')"
  >
    <div class="text">{{ tab.client.label.value }}</div>
    <div class="close" @mousedown.stop="$emit('close')">
      <VscChromeCloseVue className="react-icon" />
    </div>
  </NavBarItem>
</template>

<style scoped>
.labeled-tab {
  --bg-color: var(--dark-gray);
  --text-color: var(--light);

  color: var(--text-color);
  background-color: var(--bg-color);
}

.labeled-tab:hover {
  --bg-color: var(--da-gray);
}

.labeled-tab.active {
  --bg-color: var(--gray);
  --text-color: var(--lightest);
}

.labeled-tab.connected {
  --text-color: var(--green);
}

.labeled-tab .text {
  padding: 0 0 0 13px;
  white-space: nowrap;
  text-overflow: ellipsis;
  white-space: nowrap;
  min-width: 0;
  overflow: hidden;
}

.labeled-tab .close {
  padding: 3px;
  margin: 0 4px;
  border-radius: 3px;
  background-color: inherit;
  opacity: 0;
  flex-shrink: 0;
}

.labeled-tab .close > svg {
  fill: var(--text-color);
  display: block;
}

.labeled-tab:hover .close,
.labeled-tab.active .close {
  opacity: 1;
}

.labeled-tab .close:hover {
  background-color: color-mix(in srgb, var(--bg-color), var(--light) 10%);
}
</style>
