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

    <div class="row">
      <div class="col-12">
        <div class="card">
          <div class="card-body">
            <div class="row">
              <form @submit.prevent="save">
                <div class="row">
                  <label for="name" class="col-sm-4">Name</label>
                  <div class="col-sm-8">
                    <input v-model="name" id="name" class="form-control" placeholder="Name" />
                  </div>
                  <div class="offset-sm-4 col-sm-8 mt-4">
                    <button type="submit" class="btn btn-primary">Save</button>
                  </div>
                </div>
              </form>
            </div>

            <div class="row mt-4">
              <h4 class="col-12">Users</h4>
            </div>

            <div class="row">
              <div class="col-12 pb-3">
                <button v-if="!addUserMode" @click="startAddUser" class="btn btn-outline-primary"><font-awesome-icon icon="user-plus" />&nbsp;Add</button>
              </div>
              <template v-if="addUserMode">
                <label class="col-4">Find user</label>
                <div class="col-4">
                  <input v-model="searchUser" class="form-control" placeholder="user@email.com" />
                </div>
                <div class="col-4">
                  <template v-if="isSearchingForUsers">
                    <button v-if="findAny" @click="addUser" class="btn btn-primary">Add user</button>
                    <button v-else disabled class="btn btn-secondary">No users found</button>
                  </template>
                  <button @click="cancelAddUser" class="ms-2 float-end btn btn-outline-secondary"><font-awesome-icon icon="times" />&nbsp;Cancel</button>
                </div>
              </template>
            </div>
            <div class="row">
              <toolbar v-if="showToolbar">
                <toolbar-item @click="handleSelectAll" className="btn-outline-secondary" icon="check-square">Select all</toolbar-item>
                <toolbar-item @click="handleDeleteSelected" className="btn-outline-danger" icon="trash-alt">Delete</toolbar-item>
                <toolbar-item @click="handleClose" className="btn-outline-secondary" icon="times">Close</toolbar-item>
              </toolbar>
            </div>
            <div class="row mt-2">
              <div class="col-12">
                <div class="list-group">
                  <user-row
                    v-for="{ id, username, photo } of users"
                    :key="id"
                    :id="id"
                    :name="username"
                    :photo="photo"
                    :checkedItems="selectedUsers"
                    @update:checked="handleUserSelection"
                    @remove="removeUser"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import Toolbar from '../components/toolbar';
import ToolbarItem from '../components/toolbar.item';
import UserRow from '../components/userRow';
import { mapGetters } from 'vuex';
import { PROJECT_SELECT, PROJECT_EDIT, PROJECT_ADD_USER, PROJECT_DELETE_USER } from '../store/projects.actions';
import { isEmailExists } from '../services/users.service';
import { debounce, isEmailValid } from '../utils/helpers';

export default {
  components: { Toolbar, ToolbarItem, UserRow },
  props: {
    id: String,
  },
  data() {
    return {
      selectedUsers: [],
      name: '',
      searchUser: '',
      findAny: false,
      addUserMode: false,
    };
  },
  computed: {
    showToolbar() {
      return this.selectedUsers.length > 0;
    },
    projectName() {
      return this.project ? this.project.name : '';
    },
    users() {
      return this.project ? this.project.users : [];
    },
    isSearchingForUsers() {
      return this.searchUser.length > 0;
    },
    ...mapGetters({
      project: 'currentProject',
    }),
  },
  async mounted() {
    await this.$store.dispatch(PROJECT_SELECT, { id: this.id });

    //this.projectName = this.project.name;
    //BUG:?
    setTimeout(() => (this.name = this.project.name));
  },
  watch: {
    searchUser: debounce(async function (val) {
      if (!isEmailValid(val)) return;

      this.findAny = await isEmailExists(val);
    }, 500),
  },
  methods: {
    handleUserSelection({ id, state }) {
      if (state) {
        this.selectedUsers.push(id);
      } else {
        const idx = this.selectedUsers.indexOf(id);
        if (~idx) this.selectedUsers.splice(idx, 1);
      }
    },
    handleSelectAll() {
      for (let u of this.users) {
        if (this.selectedUsers.includes(u.id)) continue;

        this.selectedUsers.push(u.id);
      }
    },
    async handleDeleteSelected() {
      if (this.selectedUsers.length === 0) return;

      if (!confirm(`${this.selectedUsers.length} user(-s) will be removed from project. Are you sure?`)) return;

      //TODO: optimize as one request
      for (let id of this.selectedUsers) {
        await this.$store.dispatch(PROJECT_DELETE_USER, { id: this.id, userId: id });
      }

      this.selectedUsers = [];
    },
    handleClose() {
      this.selectedUsers = [];
    },

    async save() {
      await this.$store.dispatch(PROJECT_EDIT, { id: this.id, name: this.name });
    },
    async addUser() {
      await this.$store.dispatch(PROJECT_ADD_USER, { id: this.id, userEmail: this.searchUser });
    },
    async removeUser({ userId }) {
      await this.$store.dispatch(PROJECT_DELETE_USER, { id: this.id, userId });
    },

    startAddUser() {
      this.addUserMode = true;
    },
    cancelAddUser() {
      this.searchUser = '';
      this.addUserMode = false;
    },
  },
};
</script>

<style lang="scss">
.avatar-sm {
  width: 18px;
}
</style>
