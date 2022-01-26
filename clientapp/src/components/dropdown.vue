<template>
  <div v-click-outside="close" class="btn-group me-3">
    <button @click="toggle" :class="[dropdownClass]" class="btn btn-sm dropdown-toggle">{{ dropdownText }}</button>
    <ul :class="{ show: isOpen }" class="dropdown-menu">
      <li v-for="{ value, className, text } of options" :key="value">
        <a @click.prevent="select(value)" :class="[{ active: value === modelValue }, className]" class="dropdown-item" href="#">{{ text }}</a>
      </li>
    </ul>
  </div>
</template>

<script>
export default {
  name: 'Dropdown',
  emits: ['update:modelValue'],
  props: {
    options: Array,
    modelValue: String,
  },
  data() {
    return {
      //currentValue: this.modelValue,
      isOpen: false,
    };
  },
  computed: {
    selected() {
      return this.options.find((x) => x.value === this.modelValue);
    },
    dropdownClass() {
      return this.selected ? this.selected.dropdownClass : '';
    },
    dropdownText() {
      return this.selected ? this.selected.text : '';
    },
  },
  methods: {
    select(value) {
      //this.currentValue = value;
      this.close();
      this.$emit('update:modelValue', value);
    },
    toggle() {
      this.isOpen = !this.isOpen;
    },
    close() {
      this.isOpen = false;
    },
  },
};
</script>

<style lang="scss">
.dropdown-menu.show {
  top: 100%;
  bottom: auto;
}
</style>
