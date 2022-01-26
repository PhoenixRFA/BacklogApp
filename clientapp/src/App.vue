<template>
  <nav class="navbar navbar-expand-lg navbar-light bg-light">
    <div class="container-fluid">
      <a class="navbar-brand" href="/">
        <img src="@/assets/logo.png" alt="" class="d-inline-block align-text-top logo-icon" />
        Backlog App
      </a>

      <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbar-main">
        <span class="navbar-toggler-icon"></span>
      </button>

      <div class="collapse navbar-collapse" id="navbar-main">
        <ul class="navbar-nav me-lg-auto">
          <li class="nav-item">
            <nav-link to="/">Home</nav-link>
          </li>
          <li class="nav-item">
            <nav-link to="/about">About</nav-link>
          </li>
          <li v-if="isAuthenticated" class="nav-item">
            <nav-link to="/projects">Projects</nav-link>
          </li>
        </ul>

        <div v-if="isAuthenticated" class="dropdown text-center text-lg-end">
          <a href="#" class="d-block link-dark text-decoration-none dropdown-toggle" data-bs-toggle="dropdown">
            <img :src="photo" alt="" class="rounded-circle avatar-md" />
            {{ username }}
          </a>

          <ul class="dropdown-menu dropdown-menu-start dropdown-menu-end text-small text-center text-lg-start">
            <li><router-link class="dropdown-item" to="/profile">Profile</router-link></li>
            <li><hr class="dropdown-divider" /></li>
            <li><a @click="logout" class="dropdown-item" href="#">Log out</a></li>
          </ul>
        </div>

        <div v-else class="col-lg-3 d-flex flex-column flex-lg-row justify-content-lg-end">
          <router-link v-if="!isLoginPage" class="btn btn-primary mb-3 mb-lg-0 me-lg-2" to="/login">Login</router-link>
          <router-link v-if="!isRegisterPage" class="btn btn-outline-primary" to="/register">Register</router-link>
        </div>
      </div>
    </div>
  </nav>

  <main>
    <router-view />
  </main>
</template>

<script>
import { mapGetters } from 'vuex';
import navLink from './components/navLink';
import { AUTH_LOGOUT } from './store/auth.actions';
import { ROUTE_LOGIN, ROUTE_REGISTER } from './router/routeNames';

export default {
  components: {
    navLink,
  },
  computed: {
    username() {
      const user = this.$store.state.auth.user;
      return user ? user.username : '';
    },
    isLoginPage() {
      return this.$router.currentRoute.value.name === ROUTE_LOGIN;
    },
    isRegisterPage() {
      return this.$router.currentRoute.value.name === ROUTE_REGISTER;
    },
    ...mapGetters(['isAuthenticated', 'photo']),
  },
  methods: {
    logout() {
      this.$store.dispatch(AUTH_LOGOUT);
    },
  },
};
</script>

<style lang="scss">
.navbar {
  .logo-icon {
    width: 28px;
  }
}

.avatar-md {
  width: 32px;
}

#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  // text-align: center;
  color: #2c3e50;
}

.cursor-pointer {
  cursor: pointer;
}
</style>
