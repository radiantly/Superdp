<script setup>
import { ref } from "vue";

// This is a div that has a child div that covers the entire viewport when the
// actively clicked. This allows for various mouse events to be captured during
// drag without dependence on the parent

const active = ref(false);
</script>

<template>
  <div>
    <div
      @mousedown="(e) => (active = e.button === 0)"
      @mouseup="(e) => (active = false)"
      @mouseleave="(e) => (active = false)"
      class="capturable-overlay"
      :class="{ active }"
    ></div>
  </div>
</template>

<style scoped>
.capturable-overlay {
  width: 100%;
  height: 100%;
  /* background-color: rgba(255, 0, 0, 0.5); */
}

.capturable-overlay.active {
  position: fixed;
  top: 0;
  left: 0;
  height: 100vh;
  width: 100vw;
  z-index: 1337;
}
</style>
