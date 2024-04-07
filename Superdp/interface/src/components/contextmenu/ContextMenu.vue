<script setup>
import { vOnClickOutside } from "@vueuse/components";
import { ContextMenu } from "../../classes/ContextMenu.js";
import { computed, reactive, ref, watchEffect } from "vue";
import { useElementSize } from "@vueuse/core";
const props = defineProps({
  menu: {
    type: ContextMenu,
    required: true,
  },
});

const menuEl = ref(null);
const { width, height } = useElementSize(menuEl);
const translate = reactive({ x: "0", y: "0" });
const menuStyle = computed(() => ({
  top: `${props.menu.props.pos.y}px`,
  left: `${props.menu.props.pos.x}px`,
  transform: `translate(${translate.x}, ${translate.y})`,
}));
watchEffect(
  () => {
    translate.x =
      props.menu.props.pos.x + width.value < window.innerWidth ? "0" : "-100%";
    translate.y =
      props.menu.props.pos.y + height.value < window.innerHeight
        ? "0"
        : "-100%";
  },
  { flush: "post" }
);

const handleClick = (e, callback) => {
  props.menu.hide();
  if (callback) callback(e);
};
</script>

<template>
  <div
    ref="menuEl"
    class="menu"
    :class="{
      visible: menu.props.visible,
      reversed: translate.y !== '0',
    }"
    v-on-click-outside="() => menu.hide()"
    :style="menuStyle"
  >
    <div
      @click="(e) => handleClick(e, handler)"
      v-for="{ label, handler, disabled } in menu.props.items"
      :key="label"
      :class="{
        disabled,
      }"
    >
      {{ label }}
    </div>
  </div>
</template>

<style scoped>
.menu {
  position: fixed;
  z-index: 1331;

  display: flex;
  flex-direction: column;
  padding: 4px;
  min-width: 200px;

  background-color: var(--dark-gray);
  border: 1px solid var(--gray);

  color: var(--light);
  white-space: nowrap;

  opacity: 0;
  pointer-events: none;
}

.menu.reversed {
  flex-direction: column-reverse;
}

.menu.visible {
  opacity: 1;
  pointer-events: auto;
}

.menu > div {
  display: flex;
  align-items: center;
  padding-left: 25px;
  height: 22px;
  border-radius: 2px;
  user-select: none;
}

.menu > div:hover {
  background-color: var(--gray);
}

.menu > div.disabled {
  pointer-events: none;
  color: var(--lighter-gray);
}
</style>
