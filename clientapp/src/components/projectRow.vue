<template>
  <div class="list-group-item list-group-item-action d-flex justify-content-between align-items-center">
    <input v-if="canEdit" :checked="isChecked" @input="handleChecked" type="checkbox" class="form-check-input mt-0 me-3" />
    <router-link :to="tasksListRoute" class="flex-grow-1 text-start btn p-0 me-3">{{ name }}</router-link>
    <router-link v-once v-if="canEdit" :to="editRoute" class="text-primary me-2"><font-awesome-icon icon="edit" /></router-link>
    <span v-if="hasTasks" class="badge bg-primary">{{ tasksCount }}</span>
  </div>
</template>

<script>
import { ROUTE_PROJECT, ROUTE_TASKS_LIST } from '../router/routeNames';

export default {
  name: 'Project row',
  props: {
    id: String,
    name: String,
    canEdit: Boolean,
    tasksCount: Number,
    checkedItems: Array,
  },
  emits: ['update:checked'],
  computed: {
    hasTasks() {
      return this.tasksCount > 0;
    },
    isChecked() {
      return this.checkedItems.includes(this.id);
    },
    editRoute() {
      return { name: ROUTE_PROJECT, params: { id: this.id } };
    },
    tasksListRoute() {
      return { name: ROUTE_TASKS_LIST, params: { id: this.id } };
    },
  },
  methods: {
    handleChecked(e) {
      this.$emit('update:checked', { state: e.target.checked, id: this.id });
    },
  },
};
</script>
