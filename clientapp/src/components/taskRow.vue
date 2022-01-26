<template>
  <div :class="[bgClass]" class="list-group-item list-group-item-action d-flex align-items-center">
    <input v-if="canEdit" :checked="isChecked" @input="handleChecked" type="checkbox" class="form-check-input mt-0 me-3" />
    <div class="flex-grow-1">
      <button class="w-100 text-start btn p-0">{{ name }}</button>
      <small class="d-block w-100 text-start">{{ description }}</small>
    </div>
    <dropdown @update:modelValue="handleStatusChange" :modelValue="status" :options="dropdownOptions" />
    <div class="text-center">
      <div class="text-primary me-3" title="Deadline">{{ deadlineFormatted }}</div>
      <div class="badge bg-primary" title="Assessment">{{ assessment }}</div>
    </div>
    <priority :modelValue="priority" />
    <router-link v-if="canEdit" :to="editRoute" class="ms-4 text-primary"><font-awesome-icon icon="edit" /></router-link>
  </div>
</template>

<script>
import { ROUTE_TASK } from '../router/routeNames';
import Dropdown from '../components/dropdown';
import Priority from './priority';
import { Discussion, InWork, Completed } from '../services/taskStatuses';

const dropdownOptions = [
  { value: Discussion, text: 'Discussion', className: 'text-warning', dropdownClass: 'btn-outline-warning' },
  { value: InWork, text: 'In work', className: 'text-primary', dropdownClass: 'btn-outline-primary' },
  { value: Completed, text: 'Completed', className: 'text-success', dropdownClass: 'btn-outline-success' },
];

export default {
  name: 'Task row',
  components: { Dropdown, Priority },
  emits: ['update:checked', 'update:status'],
  props: {
    id: String,
    name: String,
    description: String,
    deadline: Date,
    priority: String,
    status: String,
    assessment: String,
    canEdit: Boolean,
    checkedItems: Array,
  },
  data() {
    return {
      dropdownOptions,
    };
  },
  computed: {
    isChecked() {
      return this.checkedItems.includes(this.id);
    },
    deadlineFormatted() {
      return this.deadline ? this.deadline.toLocaleDateString() : '';
    },
    editRoute() {
      return { name: ROUTE_TASK, params: { id: this.id } };
    },
    bgClass() {
      switch (this.status) {
        case status.Discussion:
          return 'list-group-item-warning';
        case status.InWork:
          return 'list-group-item-primary';
        case status.Completed:
          return 'list-group-item-success';
        default:
          return '';
      }
    },
  },
  methods: {
    handleChecked(e) {
      this.$emit('update:checked', { state: e.target.checked, id: this.id });
    },
    handleStatusChange(status) {
      this.$emit('update:status', { status, id: this.id });
    },
  },
};
</script>

<style lang="scss">
.dropdown-menu {
  .dropdown-item.active,
  .dropdown-item:active {
    background-color: #e6f0ff;
  }
}
</style>
