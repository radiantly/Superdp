<script setup>
import { Terminal } from "xterm";
import { FitAddon } from "xterm-addon-fit";
import { ref, onMounted, onBeforeUnmount } from "vue";
import { Tab } from "../classes/Tab";
import { useRafFn, useResizeObserver } from "@vueuse/core";
import { interopQueen } from "../globals";

const props = defineProps({
  tab: {
    type: Tab,
    required: true,
  },
});

const divElem = ref(null);

const terminal = new Terminal();
const fitAddon = new FitAddon();
terminal.loadAddon(fitAddon);

let readTill = 0;
useRafFn(() => {
  if (props.tab.displayBuffer === null) return;

  const writtenTill = props.tab.sizeBuffer[0];

  if (readTill == writtenTill) return;
  if (readTill == props.tab.displayBuffer.length) readTill = 0;
  const till =
    readTill < writtenTill ? writtenTill : props.tab.displayBuffer.length;

  terminal.write(props.tab.displayBuffer.subarray(readTill, till));

  readTill = till;
});

useResizeObserver(divElem, () => {
  if (!props.tab.isActive.value || props.tab.props.type !== "ssh") return;
  fitAddon.fit();
  props.tab.resizeTerminal(terminal.rows, terminal.cols);
});

onMounted(() => {
  terminal.open(divElem.value);
  setTimeout(() => fitAddon.fit(), 0);
  terminal.onData((data) => interopQueen.SSHInput(props.tab.id, data));
  terminal.attachCustomKeyEventHandler((arg) => {
    if (arg.ctrlKey && arg.code === "KeyC" && arg.type === "keydown") {
      const selection = terminal.getSelection();
      if (selection) {
        navigator.clipboard.writeText(selection);
        terminal.clearSelection();
        return false;
      }
    } else if (arg.ctrlKey && arg.code === "KeyV" && arg.type === "keydown") {
      navigator.clipboard
        .readText()
        .then((text) => interopQueen.SSHInput(props.tab.id, text));
    }
    return true;
  });
});

onBeforeUnmount(() => {
  console.log("Disposing terminal from tab", props.tab.id);
  terminal.dispose();
});
</script>

<template>
  <div class="term" ref="divElem"></div>
</template>

<style>
.term {
  background-color: red;
  width: 100%;
  height: 100%;
  overflow: hidden;
}
</style>
