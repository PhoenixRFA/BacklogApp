<template>
  <div class="list-group-item text-center">
    <button v-if="isStandby" @click="toggle" type="button" class="btn text-primary p-0 w-100">
      <slot name="button">Add new item</slot>
    </button>
    <div v-else class="row">
      <slot :toggle="toggle"></slot>
    </div>
  </div>
</template>

<script>
export default {
  name: 'AddControl',
  emits: ['toggle'],
  data() {
    return {
      isStandby: true,
    };
  },
  methods: {
    toggle() {
      this.isStandby = !this.isStandby;
      this.$emit('toggle', { isStandby: this.isStandby });
    },
  },
};
</script>
