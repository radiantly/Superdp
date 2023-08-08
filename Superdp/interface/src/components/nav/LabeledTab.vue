<script setup>
import IconCross from "../icons/IconCross.vue";
import NavBarItem from "./NavBarItem.vue";
const props = defineProps({
  label: String,
  active: Boolean,
});
</script>

<template>
  <NavBarItem
    class="labeled-tab"
    :class="{ active: props.active }"
    @mousedown.stop="(e) => e.button == 0 && $emit('tabClick')"
  >
    <div class="text">{{ props.label }}</div>
    <div class="close" @click.stop="$emit('tabClose')">
      <IconCross />
    </div>
  </NavBarItem>
</template>

<style scoped>
.labeled-tab {
  --bgcolor: var(--gray);
  --hover-close-bgcolor: #3b3c3c;

  color: #bbb;
  background-color: var(--bgcolor);
}

.labeled-tab.active {
  color: #ffffff;
  --bgcolor: var(--dark-gray);
  --hover-close-bgcolor: #313232;
}

.labeled-tab .text {
  padding: 0 0 0 13px;
  white-space: nowrap;
}

/* I don't like this much. Can we maybe have a circle connection indicator and
   then maybe replace it with the close symbol on hover? */
/* Update: Done. */
.labeled-tab .close {
  padding: 3px;
  margin: 0 4px;
  border-radius: 3px;
  background-color: inherit;
  opacity: 0;
}

.labeled-tab .close > svg {
  fill: #bbb;
  display: block;
  width: 14px;
  height: 14px;
}

.labeled-tab:hover .close,
.labeled-tab.active .close {
  opacity: 1;
}

.labeled-tab .close:hover {
  background-color: var(--hover-close-bgcolor);
}
</style>
