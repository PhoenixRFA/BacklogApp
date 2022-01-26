<template>
  <main class="form-signin">
    <form @submit.prevent="handleLogin">
      <div class="text-center mb-4">
        <img src="../assets/logo.png" alt="" />
      </div>
      <h1 class="h3 mb-3 fw-normal">Restore password</h1>

      <p class="alert alert-primary" v-if="showRestoreInstruction">Check your email for the next step.</p>

      <template v-else>
        <div class="form-floating mb-3">
          <input type="email" class="form-control" id="floatingInput" placeholder="name@example.com" v-model="email" />
          <label for="floatingInput">Email</label>
        </div>

        <div v-if="formError" class="alert alert-danger">{{ formError }}</div>
        <button class="w-100 btn btn-lg btn-primary" type="submit">Restore</button>
        <router-link class="btn btn-link" to="/register">Create new account</router-link>
      </template>
    </form>
  </main>
</template>

<script>
import { restorePassword } from '../services/users.service';

export default {
  name: 'Restore',
  data() {
    return {
      email: '',
      loading: false,
      formError: '',
      showRestoreInstruction: false,
    };
  },
  methods: {
    validateForm() {
      this.email = this.email.trim();
      if (!this.email) {
        this.formError = 'Email is empty';
        return false;
      }

      this.formError = '';
      return true;
    },
    async handleLogin() {
      this.loading = true;

      if (!this.validateForm()) return;

      try {
        await restorePassword(this.email);
        this.showRestoreInstruction = true;
      } catch (er) {
        this.formError = 'Wrong Username or/and Password';
      }
    },
  },
};
</script>

<style lang="scss">
.form-signin {
  width: 100%;
  max-width: 330px;
  padding: 15px;
  margin: auto;

  .form-floating:focus-within {
    z-index: 2;
  }
}
</style>
