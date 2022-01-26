<template>
  <div class="container">
    <div class="row">
      <div class="col-12">
        <h3 class="mb-4 fw-bold">Profile</h3>
      </div>
    </div>
    <div class="row">
      <div class="col-12">
        <div class="card">
          <div class="card-body">
            <div class="row mb-5 align-items-center">
              <div class="col-sm-4">
                <span>Avatar</span>
              </div>
              <div class="col-sm-8">
                <div class="d-flex align-items-center">
                  <div class="me-3">
                    <img class="rounded-circle avatar" :src="photo" alt="" />
                  </div>
                  <div>
                    <template v-if="hasPhoto">
                      <button @click="setPhoto" class="btn btn-primary me-2">Change</button>
                      <button @click="removePhoto" class="btn btn-danger">Remove</button>
                    </template>
                    <button v-else @click="setPhoto" class="btn btn-primary me-2">Upload</button>
                  </div>
                </div>
              </div>
            </div>

            <div class="row">
              <form @submit.prevent="saveUser">
                <div class="row mb-3">
                  <label for="name" class="col-sm-4">Name</label>
                  <div class="col-sm-8">
                    <input v-model="name" id="name" class="form-control" placeholder="Name" />
                  </div>
                </div>
                <div class="row">
                  <label for="email" class="col-sm-4">Email</label>
                  <div class="col-sm-8">
                    <input readonly :value="email" id="email" type="email" class="form-control" placeholder="Email" />
                  </div>
                  <div class="offset-sm-4 col-sm-8 mt-4">
                    <button :disabled="isFormSaving" type="submit" class="btn btn-primary">Save</button>
                  </div>
                </div>
              </form>
            </div>

            <div class="row mt-4">
              <form @submit.prevent="changePassword">
                <div class="row mb-3">
                  <label for="oldPassword" class="col-sm-4">Current password</label>
                  <div class="col-sm-8">
                    <input v-model="oldPassword" id="oldPassword" type="password" class="form-control" placeholder="Current password" />
                  </div>
                </div>
                <div class="row">
                  <label for="newPasswoed" class="col-sm-4">New password</label>
                  <div class="col-sm-8">
                    <input v-model="newPassword" id="newPassword" type="password" class="form-control" placeholder="New password" />
                  </div>
                  <div class="offset-sm-4 col-sm-8 mt-4">
                    <button :disabled="isPassSaving" class="btn btn-primary">Change password</button>
                  </div>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { mapGetters } from 'vuex';
import { getCurrentUser } from '../services/auth.service';
import { saveName, uploadPhoto, deletePhoto } from '../services/users.service';
import { changePassword } from '../services/auth.service';
import { AUTH_REFRESH } from '../store/auth.actions';

export default {
  name: 'Profile',
  data() {
    return {
      user: null,
      name: '',
      oldPassword: '',
      newPassword: '',
      isFormSaving: false,
      isPassSaving: false,
    };
  },
  computed: {
    email: (state) => (state.user ? state.user.email : null),
    hasPhoto: (state) => state.user && state.user.photo,
    ...mapGetters(['photo']),
  },
  async mounted() {
    const user = await getCurrentUser();
    this.user = user;
    this.name = user.username;
  },
  methods: {
    async saveUser() {
      this.isFormSaving = true;

      try {
        await saveName(this.name);
        await this.$store.dispatch(AUTH_REFRESH);
      } catch (er) {
        console.warn(er);
      }

      this.isFormSaving = false;
    },
    async changePassword() {
      this.isPassSaving = true;

      try {
        await changePassword(this.oldPassword, this.newPassword);
        this.oldPassword = '';
        this.newPassword = '';
        await this.$store.dispatch(AUTH_REFRESH);
      } catch (er) {
        console.warn(er);
      }

      this.isPassSaving = false;
    },

    setPhoto() {
      const input = document.createElement('input');
      input.type = 'file';

      const callback = async () => {
        const user = await getCurrentUser();
        this.user = user;
        await this.$store.dispatch(AUTH_REFRESH);
      };
      input.onchange = async function () {
        if (this.files.length === 0) return;

        const file = this.files[0];
        await uploadPhoto(file);

        input.remove();

        callback();
      };

      input.click();
    },
    async removePhoto() {
      await deletePhoto();
    },
  },
};
</script>

<style lang="scss">
.avatar {
  width: 80px;
}
</style>
