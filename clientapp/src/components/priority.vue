<template>
  <img v-if="isHighest" title="Highest" src="../assets/priorities/highest.svg" class="priority-icon me-3" />
  <img v-else-if="isHigh" title="High" src="../assets/priorities/high.svg" class="priority-icon me-3" />
  <img v-else-if="isLow" title="Low" src="../assets/priorities/low.svg" class="priority-icon me-3" />
  <img v-else-if="isLowest" title="Lowest" src="../assets/priorities/lowest.svg" class="priority-icon me-3" />
  <img v-else title="Medium" src="../assets/priorities/medium.svg" class="priority-icon me-3" />
</template>

<script>
export default {
  name: 'Priority',
  props: {
    modelValue: String,
  },
  computed: {
    isHighest() {
      return this.modelValue === 'highest';
    },
    isHigh() {
      return this.modelValue === 'high';
    },
    isLow() {
      return this.modelValue === 'low';
    },
    isLowest() {
      return this.modelValue === 'lowest';
    },
  },
};
</script>
