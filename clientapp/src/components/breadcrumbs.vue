<template>
  <nav>
    <ol class="breadcrumb">
      <li v-for="{ url, name } of linkItems" :key="url" class="breadcrumb-item">
        <a :href="url">{{ name }}</a>
      </li>
      <li class="breadcrumb-item active">{{ lastItemName }}</li>
    </ol>
  </nav>
</template>

<script>
import { mapGetters } from 'vuex';
import { ROUTE_PROJECTS_LIST, ROUTE_PROJECT, ROUTE_TASKS_LIST, ROUTE_TASK } from '../router/routeNames';

export default {
  name: 'Breadcrumbs',
  data() {
    return {
      breadcrumbItems: [],
    };
  },
  computed: {
    routeName() {
      return this.$router.currentRoute.value.name;
    },
    isRouteWaitForProject() {
      return this.routeName === ROUTE_PROJECT || this.routeName === ROUTE_TASKS_LIST || this.routeName === ROUTE_TASK;
    },
    linkItems() {
      return this.breadcrumbItems.slice(0, -1);
    },
    lastItemName() {
      if (this.breadcrumbItems.length === 0) return '';

      return this.breadcrumbItems[this.breadcrumbItems.length - 1].name;
    },
    ...mapGetters({
      project: 'currentProject',
    }),
  },
  created() {
    if (!this.isRouteWaitForProject) {
      this.init();
      return;
    }

    const unwatchProject = this.$watch('project', (val) => {
      if (!val) return;

      this.init();
      unwatchProject();
    });
  },
  methods: {
    init() {
      this.breadcrumbItems.push({ url: '/', name: 'Home' });
      this.breadcrumbItems.push({ url: '/projects', name: 'Projects' });
      if (this.routeName === ROUTE_PROJECTS_LIST) return;

      const projectId = this.project.id;
      const projectName = this.project.name;
      this.breadcrumbItems.push({ url: `/project/${projectId}`, name: projectName });
      if (this.routeName === ROUTE_PROJECT) return;

      this.breadcrumbItems.push({ url: `/project/${projectId}/tasks`, name: 'Tasks' });
      if (this.routeName === ROUTE_TASKS_LIST) return;

      const taskId = this.$router.currentRoute.value.params.id;
      const task = this.$store.state.tasks.tasks.find((x) => x.id === taskId);

      this.breadcrumbItems.push({ url: `/tasks/${taskId}`, name: task.name });
      if (this.routeName === ROUTE_TASK) return;
    },
  },
};
</script>
