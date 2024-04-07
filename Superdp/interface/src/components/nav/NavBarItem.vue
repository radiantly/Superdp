<script setup></script>
<template>
  <div class="tab">
    <slot></slot>
  </div>
</template>

<style scope>
.tab {
  user-select: none;
  display: flex;
  align-items: center;
  position: relative;
  height: var(--tab-height);
  min-width: 0;
}
</style>
