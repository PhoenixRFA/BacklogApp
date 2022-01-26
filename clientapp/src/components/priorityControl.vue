<template>
  <div class="btn-group priority-control">
    <button @click="select('lowest')" type="button" :class="{ active: modelValue === 'lowest' }" class="btn btn-outline-primary" title="Lowest">
      <img class="priority-icon" src="../assets/priorities/lowest.svg" />
    </button>
    <button @click="select('low')" type="button" :class="{ active: modelValue === 'low' }" class="btn btn-outline-primary" title="Low">
      <img class="priority-icon" src="../assets/priorities/low.svg" />
    </button>
    <button @click="select('medium')" type="button" :class="{ active: modelValue === 'medium' }" class="btn btn-outline-primary" title="Medium">
      <img class="priority-icon" src="../assets/priorities/medium.svg" />
    </button>
    <button @click="select('high')" type="button" :class="{ active: modelValue === 'high' }" class="btn btn-outline-primary" title="High">
      <img class="priority-icon" src="../assets/priorities/high.svg" />
    </button>
    <button @click="select('highest')" type="button" :class="{ active: modelValue === 'highest' }" class="btn btn-outline-primary" title="Highest">
      <img class="priority-icon" src="../assets/priorities/highest.svg" />
    </button>
  </div>
</template>

<script>
export default {
  name: 'PriorityControl',
  emits: ['update:modelValue'],
  props: {
    modelValue: String,
  },
  methods: {
    select(value) {
      if (this.modelValue == value) return;

      this.$emit('update:modelValue', value);
    },
  },
};
</script>

<style lang="scss">
.priority-control .btn-outline-primary.active {
  background-color: #b3d1ff;
}
</style>
