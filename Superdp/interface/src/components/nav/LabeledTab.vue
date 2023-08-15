<script setup>
import { inject } from "vue";
import { Tab } from "../../classes/Tab";
import { dragManager, interopQueen } from "../../globals";
import { tabManagerKey } from "../../keys";
import IconCross from "../icons/IconCross.vue";
import NavBarItem from "./NavBarItem.vue";
const props = defineProps({
  tab: {
    type: Tab,
    required: true,
  },
});

const tabManager = inject(tabManagerKey);

const handleMouseDown = (e) => {
  if (e.button !== 0) return; // ignore if not primary mouse button
  tabManager.setActive(props.tab);

  // If this is the only tab, then drag entire window
  if (tabManager.tabs.length === 1) {
    interopQueen.MouseDownWindowDragWithTab(
      JSON.stringify(props.tab.serializeMsg())
    );
    return;
  }
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
    :class="{ active: tab.isActive.value }"
    @mousedown.stop="handleMouseDown"
    @mouseleave="handleMouseLeave"
  >
    <div class="text">{{ tab.client.label.value }}</div>
    <div class="close" @click.stop="$emit('tabClose')">
      <IconCross />
    </div>
  </NavBarItem>
</template>

<style scoped>
.labeled-tab {
  --bgcolor: var(--gray);
  --hover-close-bgcolor: #3b3c3c;

  color: #bbb;
  background-color: var(--bgcolor);
}

.labeled-tab.active {
  color: #ffffff;
  --bgcolor: var(--dark-gray);
  --hover-close-bgcolor: #313232;
}

.labeled-tab .text {
  padding: 0 0 0 13px;
  white-space: nowrap;
  text-overflow: ellipsis;
  white-space: nowrap;
  min-width: 0;
  overflow: hidden;
}

/* I don't like this much. Can we maybe have a circle connection indicator and
   then maybe replace it with the close symbol on hover? */
/* Update: Done. */
.labeled-tab .close {
  padding: 3px;
  margin: 0 4px;
  border-radius: 3px;
  background-color: inherit;
  opacity: 0;
  flex-shrink: 0;
}

.labeled-tab .close > svg {
  fill: #bbb;
  display: block;
  width: 14px;
  height: 14px;
}

.labeled-tab:hover .close,
.labeled-tab.active .close {
  opacity: 1;
}

.labeled-tab .close:hover {
  background-color: var(--hover-close-bgcolor);
}
</style>
