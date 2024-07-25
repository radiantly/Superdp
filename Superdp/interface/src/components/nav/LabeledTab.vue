<script setup>
import { ref } from "vue";
import { Tab } from "../../classes/Tab";
import { VscChromeCloseVue } from "../icons";
import NavBarItem from "./NavBarItem.vue";
const props = defineProps({
  tab: {
    type: Tab,
    required: true,
  },
});

defineEmits(["close"]);

const tabManager = props.tab.props.parent;
const dropTarget = ref(false);

const handleMouseDown = (e) => {
  if (e.button !== 0) return; // ignore if not primary mouse button
  tabManager.setActive(props.tab);
};

const handleDragStart = (e) => {
  e.dataTransfer.setData("superdp/tab", props.tab.id);
  console.log(props.tab.id);
};
const handleDragOver = (e) => {
  if (e.dataTransfer.types.includes("superdp/tab")) e.preventDefault();
};
const handleDrop = (e) => {
  e.preventDefault();
  const tabId = e.dataTransfer.getData("superdp/tab");
  if (tabId === props.tab.id) return;
  if (tabId) {
    // check if same tab manager
    const tabIdx = tabManager.tabs.findIndex((tab) => tab.id === tabId);
    if (tabIdx === -1) return;
    const tab = tabManager.tabs.splice(tabIdx, 1)[0];
    const thisTabIdx = tabManager.tabs.findIndex(
      (tab) => tab.id === props.tab.id
    );
    tabManager.tabs.splice(thisTabIdx, 0, tab);
  }
};
</script>

<template>
  <NavBarItem
    class="labeled-tab"
    :class="{
      active: tab.isActive.value,
      connected: tab.props.state === 'connected',
      drop: dropTarget,
    }"
    @mousedown.passive="handleMouseDown"
    draggable="true"
    @dragstart="handleDragStart"
    @dragover="handleDragOver"
    @drop="handleDrop"
  >
    <div class="text">{{ tab.client.label.value }}</div>
    <div class="close" @mousedown.stop="$emit('close')">
      <VscChromeCloseVue className="react-icon" />
    </div>
  </NavBarItem>
</template>

<style scoped>
.labeled-tab {
  --bg-color: var(--darker-gray);
  --text-color: var(--light);

  color: var(--text-color);
  background-color: var(--bg-color);
  position: relative;
}

.labeled-tab:hover {
  --bg-color: var(--darke-gray);
}

.labeled-tab.active {
  --bg-color: var(--dark-gray);
  --text-color: var(--lightest);
}

.labeled-tab.active::before {
  content: "";
  position: absolute;
  top: -1px;
  left: 0;
  width: 100%;
  height: 1px;
  background-color: var(--bg-color);
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
