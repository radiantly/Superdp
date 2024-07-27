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
  active: {
    type: Boolean,
    required: true,
  },
  connected: {
    type: Boolean,
    required: true,
  },
});

defineEmits(["close"]);
</script>

<template>
  <NavBarItem
    class="labeled-tab"
    :class="{
      active,
      connected,
    }"
  >
    <div class="text">{{ tab.client.label.value }}</div>
    <div class="close" @mousedown="$emit('close')">
      <VscChromeCloseVue className="react-icon" />
    </div>
  </NavBarItem>
</template>

<style scoped>
.labeled-tab {
  --bg-color: var(--darker-gray);
  --text-color: var(--light);

  flex: 1 0 auto;
  position: relative;

  background-color: var(--bg-color);

  display: flex;
  justify-content: space-evenly;
  color: var(--text-color);
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

.labeled-tab.active::after {
  content: "";
  position: absolute;
  bottom: 0;
  left: 0;
  width: 100%;
  height: 1px;
  background-color: var(--striking-blue);
}

.labeled-tab.connected {
  --text-color: var(--green);
}

/* help for e.offsetX for dragover event. */
.labeled-tab:not(:hover) > * {
  pointer-events: none;
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
