<script setup>
import { Terminal } from "xterm";
import { FitAddon } from "xterm-addon-fit";
import { ref, onMounted, onBeforeUnmount } from "vue";
import { Tab } from "../classes/Tab";
import { useRafFn, useResizeObserver } from "@vueuse/core";

const props = defineProps({
  tab: {
    type: Tab,
    required: true,
  },
  buffer: {
    type: Object,
  },
});

const emit = defineEmits(["resize", "input"]);

const divElem = ref(null);

let terminal;
let fitAddon;

let readTill = 0;

const focusTerminal = () => terminal?.focus();

onMounted(() => {
  terminal = new Terminal({
    theme: {
      background: "#2e3440",
      red: "#bf616a",
      green: "#a3be8c",
      yellow: "#ebcb8b",
      blue: "#5e81ac",
      magenta: "#b48ead",
      cyan: "#88c0d0",
      white: "#eceff4",
    },
    fontFamily:
      'ui-monospace, Menlo, Monaco, "Cascadia Mono", "Segoe UI Mono", "Roboto Mono", "Oxygen Mono", "Ubuntu Monospace", "Source Code Pro", "Fira Code", "Droid Sans Mono", "Courier New", monospace',
  });
  fitAddon = new FitAddon();
  terminal.loadAddon(fitAddon);

  terminal.open(divElem.value);
  fitAddon.fit();

  terminal.onData((data) => emit("input", data));
  terminal.onBinary((data) => emit("input", data));
  terminal.attachCustomKeyEventHandler((arg) => {
    if (arg.ctrlKey && arg.code === "KeyC" && arg.type === "keydown") {
      const selection = terminal.getSelection();
      if (selection) {
        navigator.clipboard.writeText(selection);
        terminal.clearSelection();
        return false;
      }
    } else if (arg.ctrlKey && arg.code === "KeyV" && arg.type === "keydown") {
      navigator.clipboard.readText().then((text) => emit("input", text));
    }
    return true;
  });

  props.tab.addEventListener("focus", focusTerminal);
});

useRafFn(() => {
  if (props.buffer === null) return;
  const writtenTill = props.buffer.size[0];

  if (readTill == writtenTill) return;
  if (readTill == props.buffer.display.length) readTill = 0;
  const till =
    readTill < writtenTill ? writtenTill : props.buffer.display.length;

  terminal.write(props.buffer.display.subarray(readTill, till));

  readTill = till;
});

useResizeObserver(divElem, () => {
  fitAddon.fit();
  emit("resize", terminal.rows, terminal.cols);
});

onBeforeUnmount(() => {
  props.tab.removeEventListener("focus", focusTerminal);
  terminal.dispose();
});
</script>

<template>
  <div class="term" ref="divElem"></div>
</template>

<style scoped>
.term {
  width: 100%;
  height: 100%;
  overflow: hidden;
  background-color: var(--dark-gray);
}
.term :deep(.xterm) {
  /* there may be extra space left at the bottom (not enough space for a line),
  and there is extra space on the right because of the scrollbar */
  padding: 12px 8px 6px 18px;
}
</style>
