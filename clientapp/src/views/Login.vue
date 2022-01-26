<template>
  <main class="form-signin">
    <form @submit.prevent="handleLogin">
      <div class="text-center mb-4">
        <img src="../assets/logo.png" alt="" />
      </div>
      <h1 class="h3 mb-3 fw-normal">Log in</h1>

      <div v-if="infoText" class="alert alert-info">{{ infoText }}</div>

      <div class="form-floating">
        <input type="email" class="form-control" id="floatingInput" placeholder="name@example.com" v-model="username" />
        <label for="floatingInput">Username</label>
      </div>
      <div class="form-floating">
        <input type="password" class="form-control" id="floatingPassword" placeholder="Password" v-model="password" />
        <label for="floatingPassword">Password</label>
      </div>

      <div class="checkbox mb-3">
        <label> <input type="checkbox" value="remember-me" v-model="remember" /> Remember me </label>
      </div>

      <div v-if="formError" class="alert alert-danger">{{ formError }}</div>
      <button class="w-100 btn btn-lg btn-primary" type="submit">Sign in</button>

      <router-link class="btn btn-link" to="/restore">Forgot password?</router-link> |
      <router-link class="btn btn-link" to="/register">Register</router-link>

      <!-- <p class="mt-5 mb-3 text-muted">&copy; {{year}}</p> -->
    </form>
  </main>
</template>

<script>
import { AUTH_REQUEST } from '../store/auth.actions';
import { ROUTE_HOME } from '../router/routeNames';

export default {
  name: 'Login',
  props: {
    user: String,
    infoText: String,
  },
  data() {
    return {
      username: this.user || '',
      password: '',
      remember: false,
      year: new Date().getFullYear(),
      loading: false,
      formError: '',
    };
  },
  methods: {
    validateForm() {
      this.username = this.username.trim();
      this.password = this.password.trim();
      if (!this.username) {
        this.formError = 'Username is empty';
        return false;
      }
      if (!this.password) {
        this.formError = 'Password is empty';
        return false;
      }

      this.formError = '';
      return true;
    },
    buildModel() {
      return {
        username: this.username,
        password: this.password,
        remember: this.remember,
      };
    },
    async handleLogin() {
      this.loading = true;

      if (!this.validateForm()) return;

      const model = this.buildModel();
      try {
        await this.$store.dispatch(AUTH_REQUEST, model);

        this.$router.push({ name: ROUTE_HOME });
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

  input[type='email'] {
    margin-bottom: -1px;
    border-bottom-right-radius: 0;
    border-bottom-left-radius: 0;
  }
  input[type='password'] {
    margin-bottom: 10px;
    border-top-left-radius: 0;
    border-top-right-radius: 0;
  }
  .checkbox {
    font-weight: 400;
  }
  .form-floating:focus-within {
    z-index: 2;
  }
}
</style>
