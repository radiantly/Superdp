<script setup>
import { Tab } from "../../classes/Tab.js";
import { UseTimeAgo } from "@vueuse/components";
import Terminal from "../Terminal.vue";
import ResizableSideBar from "../side/ResizableSideBar.vue";
import ClientEditSide from "../side/ClientEditSide.vue";
import { computed, provide, ref } from "vue";
const props = defineProps({
  tab: {
    type: Tab,
    required: true,
  },
});

const client = computed(() => props.tab.client);

const setDefaultClientName = (hostname) => {
  if (client.value.props.name) return;
  client.value.props.name = hostname;
};

const sidebarWidth = ref(300);
provide("tab", props.tab);
</script>
<template>
  <div
    class="page-stack"
    :style="{
      '--side-width': `${sidebarWidth}px`,
    }"
    :class="{ [tab.props.state]: true, [tab.props.type ?? 'none']: true }"
  >
    <div
      class="term-wrap"
      :class="{
        invisible: props.tab.props.type !== 'ssh' || tab.props.buffer === null,
      }"
    >
      <Terminal
        class="terminal"
        :tab="tab"
        :buffer="tab.props.buffer"
        @resize="(rows, cols) => props.tab.setTerminalSize(rows, cols)"
        @input="(data) => props.tab.sshInput(data)"
        @hostname="setDefaultClientName"
      />
      <div class="label-overlay">
        <div class="text">{{ tab.props.state }}</div>
      </div>
    </div>
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
          <template v-for="({ date, content }, index) of tab.logs" :key="index">
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
.terminal {
  position: relative;
  z-index: 42;
}
.term-wrap {
  z-index: 42;
  position: relative;
  transform: translateX(var(--side-width));
  transition: transform 0.2s ease;

  display: flex;
  min-height: 0;
  min-width: 0;
}
.connected .term-wrap {
  transform: translateX(0);
}
.term-wrap .label-overlay {
  opacity: 1;
  background-color: rgba(0, 0, 0, 0.75);
  position: absolute;
  top: 0;
  left: 0;
  height: 100%;
  width: 100%;
  z-index: 43;
  display: flex;
  justify-content: center;
  align-items: center;
  pointer-events: none;
  transition: opacity 0.2s ease;
}
.term-wrap .label-overlay .text {
  transform: translateX(calc(0px - var(--side-width) / 2));
  text-transform: uppercase;
  opacity: 0.9;
  font-size: 2em;
  font-weight: 700;
}

.connected .term-wrap .label-overlay {
  opacity: 0;
}
</style>
