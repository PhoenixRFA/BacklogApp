<template>
  <div class="container">
    <div class="row">
      <breadcrumbs />
    </div>

    <div class="row">
      <div class="col-12">
        <h3 class="mb-4 fw-bold">Project: {{ projectName }}</h3>
      </div>
    </div>

    <div class="row pb-3">
      <div class="col-5">
        <input v-model="search" class="form-control" placeholder="Search" />
      </div>
      <div class="col-1 text-start">
        <button v-if="searchNotEmpty" @click="clearSearch" class="btn btn-outline-secondary" title="Clear"><font-awesome-icon icon="times" /></button>
      </div>
    </div>

    <div class="row">
      <toolbar v-if="showToolbar" class="mb-3">
        <toolbar-item @click="handleSelectAll" className="btn-outline-secondary" icon="check-square">Select all</toolbar-item>
        <toolbar-item @click="handleDelete" className="btn-outline-danger" icon="trash-alt">Delete</toolbar-item>
        <toolbar-item @click="handleClose" className="btn-outline-secondary" icon="times">Close</toolbar-item>
      </toolbar>
    </div>

    <div class="row">
      <div class="col-12">
        <div class="list-group">
          <!-- <div class="list-group-item text-center">
            <button @click="createTask" :disabled="isCreatingTask" type="button" class="btn text-primary p-0 w-100"><font-awesome-icon icon="plus-square"/>&nbsp;Add Task</button>
          </div> -->
          <add-control>
            <template #button> <font-awesome-icon icon="plus-square" />&nbsp;Add task </template>
            <template #default="{ toggle }">
              <div class="col-12 col-lg-10 mb-2 mb-lg-0">
                <input v-model="newTaskName" v-focus @keyup.enter="createTask(toggle)" @keyup.esc="cancelTaskCreation(), toggle()" class="form-control" placeholder="New task name" />
              </div>
              <div class="col-12 col-lg-2 d-flex justify-content-around align-items-center lh-1">
                <a @click.prevent="createTask(toggle)" href="#" class="d-none d-lg-block text-decoration-none text-muted"
                  ><small>Press <font-awesome-icon class="ms-1" icon="level-down-alt" rotation="90" /><br />to submit</small></a
                >
                <button @click="createTask(toggle)" class="d-block d-lg-none px-4 btn btn-primary">Submit</button>
                <a @click.prevent="cancelTaskCreation(), toggle()" href="#" class="d-none d-lg-block text-decoration-none text-muted"
                  ><small>Press <kbd class="px-1 py-0">Esc</kbd><br />to cancel</small></a
                >
                <button @click="cancelTaskCreation(), toggle()" class="d-block d-lg-none px-4 btn btn-secondary">Cancel</button>
              </div>
            </template>
          </add-control>
          <task-row
            v-for="{ id, name, description, deadline, assessment, priority, status, canEdit } of filteredTasks"
            :key="id"
            :id="id"
            :name="name"
            :description="description"
            :deadline="deadline"
            :assessment="assessment"
            :priority="priority"
            :status="status"
            :canEdit="canEdit"
            :checkedItems="selectedTasks"
            @update:checked="handleTaskSelection"
            @update:status="handleTaskStatusChange"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { PROJECT_SELECT } from '../store/projects.actions';
import { TASK_GET_ALL, TASK_DELETE, TASK_EDIT_STATUS, TASK_CREATE } from '../store/tasks.actions';
import Toolbar from '../components/toolbar';
import ToolbarItem from '../components/toolbar.item';
import TaskRow from '../components/taskRow';
import AddControl from '../components/addControl';
//import { debounce } from '../utils/helpers';

export default {
  name: 'Tasks list',
  props: {
    id: String,
  },
  components: { Toolbar, ToolbarItem, TaskRow, AddControl },
  data() {
    return {
      selectedTasks: [],
      search: '',
      isCreatingTask: false,
      newTaskName: '',
      //filteredTasks: [],
    };
  },
  computed: {
    project() {
      return this.$store.getters.currentProject;
    },
    projectName() {
      return this.project ? this.project.name : '';
    },
    showToolbar() {
      return this.selectedTasks.length > 0;
    },
    tasks() {
      return this.$store.state.tasks.tasks;
    },
    filteredTasks() {
      if (this.searchNotEmpty) {
        return this.tasks.filter((x) => x.name.includes(this.search));
      }

      return this.tasks;
    },
    searchNotEmpty() {
      return this.search.length > 0;
    },
  },
  async mounted() {
    await this.$store.dispatch(PROJECT_SELECT, { id: this.id });

    await this.$store.dispatch(TASK_GET_ALL, { projectId: this.id });

    //setTimeout(() => this.filteredTasks = this.tasks);
  },
  // watch: {
  //   search: debounce(function(val) {
  //     if(this.searchNotEmpty){
  //       this.filteredTasks = this.tasks.filter(x => x.name.includes(val));
  //       return;
  //     }

  //     this.filteredTasks = this.tasks;
  //   }, 500),
  // },
  methods: {
    handleSelectAll() {
      for (let t of this.tasks) {
        if (!t.canEdit || this.selectedTasks.includes(t.id)) continue;

        this.selectedTasks.push(t.id);
      }
    },
    async handleDelete() {
      if (this.selectedTasks.length === 0) return;

      if (!confirm(`${this.selectedTasks.length} task(-s) will be deleted. Are you sure?`)) return;

      //TODO: optimize as one request
      for (let id of this.selectedTasks) {
        await this.$store.dispatch(TASK_DELETE, { id });
      }

      this.selectedTasks = [];
    },
    handleClose() {
      this.selectedTasks = [];
    },

    handleTaskSelection({ id, state }) {
      if (state) {
        this.selectedTasks.push(id);
      } else {
        const idx = this.selectedTasks.indexOf(id);
        if (~idx) this.selectedTasks.splice(idx, 1);
      }
    },
    async handleTaskStatusChange({ id, status }) {
      await this.$store.dispatch(TASK_EDIT_STATUS, { id, status });
    },

    clearSearch() {
      this.search = '';
    },

    async createTask(callback) {
      if (!this.newTaskName) return;

      let deadline = new Date();
      deadline.setDate(deadline.getDate() + 7);
      deadline.setHours(0, 0, 0, 0);

      await this.$store.dispatch(TASK_CREATE, { projectId: this.id, name: this.newTaskName, assessment: '', deadline, description: '', priority: 'medium' });

      this.cancelTaskCreation();
      callback && callback();
    },
    cancelTaskCreation() {
      this.newTaskName = '';
    },
  },
};
</script>

<style lang="scss">
.priority-icon {
  width: 18px;
}
</style>
