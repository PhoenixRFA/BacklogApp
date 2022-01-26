<template>
  <div class="container">
    <div class="row">
      <breadcrumbs />
    </div>

    <div class="row">
      <toolbar class="pb-2" v-if="showToolbar">
        <toolbar-item @click="handleSelectAll" className="btn-outline-secondary" icon="check-square">Select all</toolbar-item>
        <toolbar-item @click="handleDelete" className="btn-outline-danger" icon="trash-alt">Delete</toolbar-item>
        <toolbar-item @click="handleClose" className="btn-outline-secondary" icon="times">Close</toolbar-item>
      </toolbar>
    </div>

    <div class="row">
      <div class="col-12">
        <div class="list-group">
          <add-control>
            <template #button> <font-awesome-icon icon="plus-square" />&nbsp;Add project </template>
            <template #default="{ toggle }">
              <div class="col-12 col-lg-10 mb-2 mb-lg-0">
                <input v-model="newProjectName" v-focus @keyup.enter="createProject(toggle)" @keyup.esc="cancelProjectCreating(), toggle()" class="form-control" placeholder="New project name" />
              </div>
              <div class="col-12 col-lg-2 d-flex justify-content-around align-items-center lh-1">
                <a @click.prevent="createProject(toggle)" href="#" class="d-none d-lg-block text-decoration-none text-muted"
                  ><small>Press <font-awesome-icon class="ms-1" icon="level-down-alt" rotation="90" /><br />to submit</small></a
                >
                <button @click="createProject(toggle)" class="d-block d-lg-none px-4 btn btn-primary">Submit</button>
                <a @click.prevent="cancelProjectCreating(), toggle()" href="#" class="d-none d-lg-block text-decoration-none text-muted"
                  ><small>Press <kbd class="px-1 py-0">Esc</kbd><br />to cancel</small></a
                >
                <button @click="cancelProjectCreating(), toggle()" class="d-block d-lg-none px-4 btn btn-secondary">Cancel</button>
              </div>
            </template>
          </add-control>
          <project-row
            v-for="{ id, name, canEdit, tasksCount } of projects"
            :key="id"
            :id="id"
            :name="name"
            :canEdit="canEdit"
            :tasksCount="tasksCount"
            :checkedItems="selectedProjects"
            @update:checked="handleProjectSelection"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { mapGetters } from 'vuex';
import { PROJECT_GET_ALL, PROJECT_DELETE, PROJECT_CREATE } from '../store/projects.actions';
import ProjectRow from '../components/projectRow';
import Toolbar from '../components/toolbar';
import ToolbarItem from '../components/toolbar.item';
import AddControl from '../components/addControl';

export default {
  name: 'ProjectsList',
  components: {
    ProjectRow,
    Toolbar,
    ToolbarItem,
    AddControl,
  },
  data() {
    return {
      selectedProjects: [],
      newProjectName: '',
    };
  },
  computed: {
    showToolbar() {
      return this.selectedProjects.length > 0;
    },
    ...mapGetters({
      projects: 'projectsSorted',
    }),
  },
  async mounted() {
    await this.$store.dispatch(PROJECT_GET_ALL);
  },
  methods: {
    handleSelectAll() {
      for (let p of this.projects) {
        if (!p.canEdit || this.selectedProjects.includes(p.id)) continue;

        this.selectedProjects.push(p.id);
      }
    },
    async handleDelete() {
      if (this.selectedProjects.length === 0) return;

      if (!confirm(`${this.selectedProjects.length} project(-s) will be deleted. Are you sure?`)) return;

      //TODO: optimize as one request
      for (let id of this.selectedProjects) {
        await this.$store.dispatch(PROJECT_DELETE, { id });
      }

      this.selectedProjects = [];
    },
    handleClose() {
      this.selectedProjects = [];
    },

    handleProjectSelection({ id, state }) {
      if (state) {
        this.selectedProjects.push(id);
      } else {
        const idx = this.selectedProjects.indexOf(id);
        if (~idx) this.selectedProjects.splice(idx, 1);
      }
    },

    async createProject(callback) {
      if (!this.newProjectName) return;

      await this.$store.dispatch(PROJECT_CREATE, { name: this.newProjectName });
      this.cancelProjectCreating();
      callback && callback();
    },
    cancelProjectCreating() {
      this.newProjectName = '';
    },
  },
};
</script>
