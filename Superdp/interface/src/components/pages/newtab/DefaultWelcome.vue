<script setup>
import { inject, onMounted, ref, watchEffect } from "vue";
import {
  clientManager,
  interopQueen,
  windowIsMaximized,
} from "../../../globals";
import { Tab } from "../../../classes/Tab";
import { sidePropsKey, tabManagerKey } from "../../../keys";
import {
  VscChromeCloseVue,
  VscChromeMinimizeVue,
  VscChromeRestoreVue,
} from "../../icons";
import { handleMinimize, handleRestore, handleClose } from "../../../utils";
import RecentClients from "./RecentClients.vue";

const tabManager = inject(tabManagerKey);
const sideProps = inject(sidePropsKey);
const quickInputRef = ref(null);
const quickHost = ref("");
const handleKeydown = (e) => {
  if (e.key === "Enter") {
    quickHost.value = quickHost.value.trim();
    const [username, host] = quickHost.value.includes("@")
      ? quickHost.value.split("@", 2)
      : ["", quickHost.value];
    const client = clientManager.createFloatingClient({
      username,
      host,
    });
    interopQueen.CheckOpenPorts(host).then((port) => {
      if (!port) return;
      if (client.props.type !== null) return;
      client.props.type = port === 22 ? "ssh" : "rdp";
    });
    const tab = new Tab({ client });
    client.addTab(tabManager, tab);
    quickHost.value = "";
  }
};

// TODO: this seems sorta quirky?
onMounted(() => {
  quickInputRef.value.focus();
});
watchEffect(() => {
  if (tabManager.activeTab === null && sideProps.activeEntry === null)
    quickInputRef.value?.focus();
});
</script>

<template>
  <div class="container">
    <nav>
      <div
        class="spacer"
        @mousedown="
          () => {
            interopQueen.MouseDownWindowDrag();
          }
        "
      ></div>
      <div
        class="title-bar-btn"
        @click="handleMinimize"
        v-if="windowIsMaximized"
      >
        <VscChromeMinimizeVue className="react-icon" />
      </div>
      <div
        class="title-bar-btn"
        @click="handleRestore"
        v-if="windowIsMaximized"
      >
        <VscChromeRestoreVue className="react-icon" />
      </div>
      <div
        class="title-bar-btn close"
        @click="handleClose"
        v-if="windowIsMaximized"
      >
        <VscChromeCloseVue className="react-icon" />
      </div>
    </nav>
    <input
      ref="quickInputRef"
      class="quick-input"
      placeholder="root@172.48.1.104"
      @keydown="handleKeydown"
      v-model="quickHost"
    />
    <RecentClients />
  </div>
</template>
<style scoped>
.container {
  display: flex;
  flex-direction: column;
}
nav {
  height: 35px;

  display: flex;
}
nav .spacer {
  flex-grow: 1;
}
nav .title-bar-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 50px;
  height: 35px;
  transition: background-color 0.1s ease;
}
nav .title-bar-btn:hover {
  background-color: var(--gray);
}
nav .title-bar-btn.close:hover {
  background-color: red;
}
.quick-input {
  font-size: 24px;
  font-family: var(--ui-font);
  border: none;
  background-color: var(--da-gray);
  border-radius: 3px;
  padding: 8px 15px;
  outline: none;
  margin: 0 50px;
}
.quick-input::placeholder {
  color: var(--lighter-gray);
}
</style>
