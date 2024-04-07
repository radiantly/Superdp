<script setup>
import { Tab } from "../../classes/Tab.js";
import { UseTimeAgo } from "@vueuse/components";
import Terminal from "../Terminal.vue";
import ResizableSideBar from "../side/ResizableSideBar.vue";
import ClientEditSide from "../side/ClientEditSide.vue";
import { provide, ref } from "vue";
const props = defineProps({
  tab: {
    type: Tab,
    required: true,
  },
});

const sidebarWidth = ref(300);
provide("tab", props.tab);
</script>
<template>
  <div class="page-stack">
    <Terminal
      class="terminal"
      :class="{
        invisible: props.tab.props.type !== 'ssh' || tab.props.buffer === null,
      }"
      :tab="tab"
      :buffer="tab.props.buffer"
      @resize="(rows, cols) => props.tab.setTerminalSize(rows, cols)"
      @input="(data) => props.tab.sshInput(data)"
      :style="{
        transform: `translateX(${
          props.tab.props.state === 'connected' ? 0 : sidebarWidth
        }px)`,
      }"
    />
    <div
      class="page-container"
      :class="{
        invisible:
          props.tab.props.type === 'rdp' &&
          props.tab.props.state === 'connected',
      }"
    >
      <ResizableSideBar v-model:width="sidebarWidth">
        <ClientEditSide :entry="tab.client.entry" />
      </ResizableSideBar>
      <div class="scrollable">
        <div class="connection-log">
          <template
            v-for="({ date, content }, index) of tab.logs.slice().reverse()"
            :key="index"
          >
            <UseTimeAgo v-slot="{ timeAgo }" :time="date">
              <div class="reltime" :title="date.toLocaleString()">
                {{ timeAgo }}
              </div>
            </UseTimeAgo>
            <div>{{ content }}</div>
          </template>
        </div>
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
  background-color: var(--dark-gray);
  min-height: 0;
}

.terminal {
  z-index: 42;
  transition: transform 0.2s ease;
  position: relative;
}

.invisible {
  visibility: hidden;
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

.scrollable {
  overflow-y: scroll;
  flex-grow: 1;
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
  color: var(--light);
  text-wrap: wrap;
}
.reltime {
  text-align: right;
  color: var(--lighter-gray);
}
</style>
