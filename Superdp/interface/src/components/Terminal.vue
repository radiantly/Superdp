<script setup>
import { Terminal } from "xterm";
import { FitAddon } from "xterm-addon-fit";
import { ref, onMounted, onBeforeUnmount, nextTick } from "vue";
import { Tab } from "../classes/Tab";
import { useRafFn, useResizeObserver } from "@vueuse/core";
import { interopQueen } from "../globals";

const props = defineProps({
  buffer: {
    type: Object,
    required: true,
  },
});

const emit = defineEmits(["resize", "input"]);

const divElem = ref(null);

let terminal;
let fitAddon;

let readTill = 0;

onMounted(() => {
  terminal = new Terminal({
    theme: {
      background: "#1e1e1e",
    },
    fontFamily:
      'ui-monospace, Menlo, Monaco, "Cascadia Mono", "Segoe UI Mono", "Roboto Mono", "Oxygen Mono", "Ubuntu Monospace", "Source Code Pro", "Fira Code", "Droid Sans Mono", "Courier New", monospace',
  });
  fitAddon = new FitAddon();
  terminal.loadAddon(fitAddon);

  terminal.open(divElem.value);
  fitAddon.fit();

  terminal.onData((data) => emit("input", data));
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
});

useRafFn(() => {
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

onBeforeUnmount(() => terminal.dispose());
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
