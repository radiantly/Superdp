<script setup>
defineOptions({
  inheritAttrs: false,
});

const model = defineModel();

const handleFileDrop = async (event) => {
  if (!event.dataTransfer.files.length) return;
  const file = event.dataTransfer.files[0];
  model.value = await file.text();
};

const handleFileChoose = async () => {
  const [fileHandle] = await window.showOpenFilePicker();
  const file = await fileHandle.getFile();
  model.value = await file.text();
};
</script>

<template>
  <textarea
    v-model="model"
    v-bind="$attrs"
    @drop.prevent="handleFileDrop"
  ></textarea>
  <div class="file-line">
    Drop or <span class="choose" @click="handleFileChoose">choose a file</span>
  </div>
</template>

<style scoped>
textarea {
  resize: vertical;
  border: none;
  outline: none;
  min-width: min(100%, 275px);
  width: 0;
  background-color: var(--da-gray);
  padding: 5px 10px;
  transition: background-color 0.2s ease;
}

::-webkit-resizer {
  display: none;
}

textarea:hover,
textarea:focus {
  background-color: var(--gray);
}
.file-line {
  color: var(--lightest-gray);
  font-size: 11px;
}
.file-line > .choose {
  cursor: pointer;
  text-decoration: underline solid var(--lighter-gray);
  text-underline-offset: 3px;
  transition: color 0.2s ease;
}
.file-line > .choose:hover {
  color: var(--light);
}
</style>
