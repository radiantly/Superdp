<script setup>
import Logo from "../icons/Logo.vue";
import NavBarItem from "./NavBarItem.vue";
</script>

<template>
  <NavBarItem class="logo">
    <Logo />
  </NavBarItem>
</template>

<style scoped>
.logo {
  align-self: stretch;
  justify-content: center;
  width: 40px;
}

.logo svg {
  object-fit: contain;
  width: 18px;
  height: 18px;
  fill: #ddd;
}
</style>
