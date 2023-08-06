<script setup>
const props = defineProps({
  menu: {
    type: Object,
    required: true,
  },
});

const handleClick = (e, callback) => {
  props.menu.visible = false;
  if (callback) callback(e);
};
</script>

<template>
  <div v-show="menu.visible" class="menu">
    <div
      @click="(e) => handleClick(e, handler)"
      v-for="{ label, handler } in menu.items"
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
  top: v-bind("menu.pos.y + 'px'");
  left: v-bind("menu.pos.x + 'px'");
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
