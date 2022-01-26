<template>
  <button :class="className" class="btn">
    <template v-if="icon"> <font-awesome-icon font-awesome-icon :icon="icon" />&nbsp; </template>
    <slot></slot>
  </button>
</template>

<script>
export default {
  name: 'Toolbar item',
  props: {
    className: String,
    icon: String,
  },
};
</script>
