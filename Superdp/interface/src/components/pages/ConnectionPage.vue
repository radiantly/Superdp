<script setup>
import { computed, reactive, ref } from "vue";
import { Tab } from "../../classes/Tab.js";
import { UseTimeAgo } from "@vueuse/components";
import Terminal from "../Terminal.vue";
const props = defineProps({
  tab: {
    type: Tab,
    required: true,
  },
});
const client = computed(() => props.tab.client);

const actionBtnText = computed(() =>
  props.tab.props.state === "connecting" ? "Connecting" : "Connect"
);

// state

// For RDP
// "connected" Hide everything
// else Show everything if not

// For SSH
// "attached" Hide everything
// "connected" Hide everything
// else Show sidebar
// const visibleSideBar
</script>
<template>
  <div class="page-stack">
    <Terminal
      class="terminal"
      v-if="tab.props.buffer"
      :buffer="tab.props.buffer"
      @resize="(rows, cols) => props.tab.setTerminalSize(rows, cols)"
      @input="(data) => props.tab.sshInput(props.tab.id, data)"
    />
    <div
      class="page-container"
      :class="{ visible: tab.props.state !== 'connected' }"
    >
      <div class="scrollable">
        <div class="connection-log">
          <template v-for="({ date, content }, index) of tab.logs" :key="index">
            <UseTimeAgo v-slot="{ timeAgo }" :time="date">
              <div class="reltime" :title="date.toLocaleString()">
                {{ timeAgo }}
              </div>
            </UseTimeAgo>
            <div>{{ content }}</div>
          </template>
        </div>
        <div class="anchor"></div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-stack {
  display: grid;
  grid-template: 1fr / 1fr;
  flex-grow: 1;
}
.page-stack > * {
  grid-area: 1 / 1 / span 1 / span 1;
}
.page-container {
  display: flex;
  flex-direction: column;
  background-color: var(--dark-gray);
  opacity: 0;
  pointer-events: none;
}

.page-container.visible {
  opacity: 1;
  pointer-events: auto;
}

.terminal {
  z-index: 42;
}

.action-btn {
  font-family: var(--ui-font);
  border: none;
  background-color: #454545;
  width: 100px;
  color: #eee;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.action-btn:hover {
  background-color: #555;
}

.action-btn:disabled {
  color: #aaa;
  background-color: #222;
}

/* https://css-tricks.com/books/greatest-css-tricks/pin-scrolling-to-bottom/ */
.scrollable {
  overflow-y: scroll;
}
.scrollable * {
  overflow-anchor: none;
}
.anchor {
  height: 1px;
  overflow-anchor: auto;
}
.connection-log {
  flex-grow: 1;
  display: grid;
  grid-template-columns: minmax(15ch, max-content) fit-content(100%);
  grid-auto-rows: min-content;
  font-family: var(--ui-font-mono);
  white-space: pre;
  gap: 5px 20px;
  padding: 10px;
  color: #ddd;
}
.reltime {
  text-align: right;
  color: #aaa;
}
</style>
