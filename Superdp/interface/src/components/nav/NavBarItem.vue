<script setup>
const props = defineProps({
  highlightLeft: Boolean,
  highlightRight: Boolean,
});
</script>
<template>
  <div class="tab">
    <slot></slot>

    <div class="highlight left" v-show="highlightLeft"></div>
    <div class="highlight right" v-show="highlightRight"></div>
  </div>
</template>

<style scope>
.tab {
  height: var(--tab-height);
  min-width: 0;
  position: relative;

  user-select: none;

  display: flex;
  align-items: center;
}
.highlight {
  position: absolute;
  top: 0;
  height: 100%;
  width: 2px;
  background-color: var(--light);
}
.highlight.left {
  left: 0;
}
.highlight.right {
  right: 0;
}
</style>
