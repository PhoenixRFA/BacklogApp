<template>
  <main class="form-signup">
    <form @submit.prevent="handleRegister">
      <div class="text-center mb-4">
        <img src="../assets/logo.png" alt="" />
      </div>
      <h1 class="h3 mb-3 fw-normal">Register</h1>

      <div class="form-floating">
        <input class="form-control" id="floatingName" placeholder="Name" v-model="username" />
        <label for="floatingPassword">Name</label>
      </div>
      <div class="form-floating my-3">
        <input type="email" class="form-control" id="floatingInput" placeholder="name@example.com" v-model="email" />
        <label for="floatingInput">Email</label>
      </div>

      <div class="checkbox mb-3">
        <label class="cursor-pointer">
          <input type="checkbox" value="remember-me" v-model="licenseAgreement" />
          License agreement
        </label>
      </div>

      <div v-if="formError" class="alert alert-danger">{{ formError }}</div>

      <button class="w-100 btn btn-lg btn-primary" type="submit">Register</button>
      <router-link class="btn btn-link" to="/login">Already registered? Log in</router-link>
    </form>
  </main>
</template>

<script>
import { register } from '../services/auth.service';
import { ROUTE_LOGIN_REGISTER } from '../router/routeNames';

export default {
  name: 'Register',
  data() {
    return {
      username: '',
      email: '',
      formError: '',
      licenseAgreement: false,
      isLoading: false,
    };
  },
  methods: {
    validateForm() {
      if (!this.licenseAgreement) {
        this.formError = 'Accept license agreement';
        return false;
      }

      this.username = this.username.trim();
      this.email = this.email.trim();

      if (!this.username) {
        this.formError = 'Name is empty';
        return false;
      }
      if (!this.email) {
        this.formError = 'Email is empty';
        return false;
      }

      this.formError = '';
      return true;
    },
    buildModel() {
      return {
        username: this.username,
        email: this.email,
      };
    },
    async handleRegister() {
      if (!this.validateForm()) return;

      const model = this.buildModel();
      try {
        const res = await register(model.username, model.email);
        this.$router.push({ name: ROUTE_LOGIN_REGISTER, params: { user: res.email } });
      } catch (er) {
        this.formError = 'Register error';
      }
    },
  },
};
</script>

<style lang="scss">
.form-signup {
  width: 100%;
  max-width: 330px;
  padding: 15px;
  margin: auto;

  .checkbox {
    font-weight: 400;
  }
  .form-floating:focus-within {
    z-index: 2;
  }
}
</style>
