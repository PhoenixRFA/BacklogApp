<template>
  <router-link :to="to" exact-active-class="active" class="nav-link"><slot></slot></router-link>
</template>

<script>
export default {
  name: 'navLink',
  props: {
    to: String,
  },
};
</script>
