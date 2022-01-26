<template>
  <main class="home text-center">
    <img alt="Vue logo" src="../assets/logo.png" />
    <h1 class="display-5 fw-bold">Backlog App</h1>
    <p class="lead">Application for managing your project features</p>

    <div class="d-flex justify-content-center gap-4">
      <router-link v-if="isAuthenticated" class="fs-4" to="/projects">Go to Projects</router-link>
      <template v-else>
        <router-link class="btn btn-lg btn-primary px-5" to="/login">Log In</router-link>
        <router-link class="btn btn-lg btn-outline-primary px-4" to="/register">Register</router-link>
      </template>
    </div>
  </main>
</template>

<script>
// @ is an alias to /src

export default {
  name: 'Home',
  computed: {
    isAuthenticated() {
      return this.$store.getters.isAuthenticated;
    },
  },
};
</script>
