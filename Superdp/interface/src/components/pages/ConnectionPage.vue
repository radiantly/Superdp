<script setup>
import { computed, reactive, ref } from "vue";
import { Tab } from "../../classes/Tab.js";
import { UseTimeAgo } from "@vueuse/components";
import InputBox from "../InputBox.vue";
const props = defineProps({
  tab: {
    type: Tab,
    required: true,
  },
});
const client = computed(() => props.tab.client);

const actionBtnText = computed(() =>
  props.tab.props.ownsRDPControl && props.tab.props.connectInProgress
    ? "Connecting"
    : "Connect"
);
</script>

<template>
  <div
    class="page-container"
    :class="{ visible: !tab.props.rdpControlVisible }"
  >
    <div class="params-wrapper">
      <InputBox v-model="client.props.name" placeholder="Name" />
      <InputBox v-model="client.props.host" placeholder="Hostname" />
      <InputBox v-model="client.props.username" placeholder="Username" />
      <InputBox
        v-model="client.props.password"
        placeholder="Password"
        type="password"
      />
      <input
        class="action-btn"
        type="button"
        :value="actionBtnText"
        :disabled="actionBtnText === 'Connecting'"
        @click="() => tab.connect()"
      />
    </div>
    <div class="scrollable">
      <div class="connection-log" @mousemove="handleMouseMove">
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
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  background-color: var(--dark-gray);
  flex-grow: 1;
  opacity: 0;
  pointer-events: none;
}

.page-container.visible {
  opacity: 1;
  pointer-events: auto;
}

.params-wrapper {
  display: flex;
  padding: 15px 20px 5px;
  gap: 15px;
}

.params-wrapper > :deep(.input-box) {
  flex-grow: 1;
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
