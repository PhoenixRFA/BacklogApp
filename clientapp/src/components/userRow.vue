<template>
  <div class="list-group-item list-group-item-action d-flex justify-content-between align-items-center">
    <input :checked="isChecked" @input="handleChecked" type="checkbox" class="form-check-input mt-0 me-3" />
    <button class="flex-grow-1 text-start btn p-0 me-3"><img class="rounded-circle avatar-sm" :src="photoUrl" alt="" /> {{ name }}</button>
    <button @click="$emit('remove', { userId: id })" type="button" class="btn text-danger me-2"><font-awesome-icon icon="trash-alt" /></button>
  </div>
</template>

<script>
export default {
  name: 'User row',
  props: {
    id: String,
    name: String,
    photo: String,
    checkedItems: Array,
  },
  emits: ['update:checked', 'remove'],
  computed: {
    isChecked() {
      return this.checkedItems.includes(this.id);
    },
    photoUrl() {
      return this.photo ? this.photo : '@/assets/avatar.svg';
    },
  },
  methods: {
    handleChecked(e) {
      this.$emit('update:checked', { state: e.target.checked, id: this.id });
    },
  },
};
</script>
