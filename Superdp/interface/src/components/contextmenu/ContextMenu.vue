<script setup>
import { vOnClickOutside } from "@vueuse/components";
import { ContextMenu } from "../../classes/ContextMenu.js";
const props = defineProps({
  menu: {
    type: ContextMenu,
    required: true,
  },
});

const handleClick = (e, callback) => {
  props.menu.hide();
  if (callback) callback(e);
};
</script>

<template>
  <div
    v-show="menu.props.visible"
    class="menu"
    v-on-click-outside="() => menu.hide()"
  >
    <div
      @click="(e) => handleClick(e, handler)"
      v-for="{ label, handler } in menu.props.items"
      :key="label"
    >
      {{ label }}
    </div>
  </div>
</template>

<style scoped>
.menu {
  position: fixed;
  background-color: #222;
  border: 1px solid #444;
  padding: 4px;
  border-radius: 4px;
  top: v-bind("menu.props.pos.y + 'px'");
  left: v-bind("menu.props.pos.x + 'px'");
  display: flex;
  flex-direction: column;
  color: #ccc;
  font-size: 13px;
}

.menu > div {
  padding: 2px 30px;
  border-radius: 3px;
  user-select: none;
}

.menu > div:hover {
  background-color: #282828;
}
</style>
