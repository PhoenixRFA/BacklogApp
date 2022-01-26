import { createRouter, createWebHistory } from 'vue-router';
import { ROUTE_HOME, ROUTE_LOGIN, ROUTE_LOGIN_REGISTER, ROUTE_REGISTER, ROUTE_PROJECTS_LIST, ROUTE_PROJECT, ROUTE_TASKS_LIST, ROUTE_TASK, ROUTE_PROFILE } from './routeNames';
import { beforeEachHandler } from './handlers';
import Home from '../views/Home.vue';
import Login from '../views/Login.vue';
import Register from '../views/Register';
import ProjectsList from '../views/ProjectsList';
import TasksList from '../views/TasksList';
import Task from '../views/Task';
import Restore from '../views/Restore';
import Profile from '../views/Profile';

const routes = [
  {
    path: '/',
    name: ROUTE_HOME,
    component: Home,
  },
  {
    path: '/about',
    name: 'About',
    // this generates a separate chunk (about.[hash].js) for this route
    // which is lazy-loaded when the route is visited.
    component: () => import(/* webpackChunkName: 'about' */ '../views/About.vue'),
  },
  {
    path: '/login/:user?',
    name: ROUTE_LOGIN,
    component: Login,
    props: true,
    meta: { requireAnon: true },
  },
  {
    path: '/login/:user?',
    name: ROUTE_LOGIN_REGISTER,
    component: Login,
    props: (route) => ({ user: route.params.user, infoText: 'Check email for login info' }),
    meta: { requireAnon: true },
  },
  {
    path: '/register',
    name: ROUTE_REGISTER,
    component: Register,
    meta: { requireAnon: true },
  },
  {
    path: '/restore',
    name: 'Restore',
    component: Restore,
    meta: { requireAnon: true },
  },
  {
    path: '/profile',
    name: ROUTE_PROFILE,
    component: Profile,
    meta: { requireAuth: true },
  },

  {
    path: '/projects',
    name: ROUTE_PROJECTS_LIST,
    component: ProjectsList,
    meta: { requireAuth: true },
  },
  {
    path: '/project/:id',
    name: ROUTE_PROJECT,
    props: true,
    component: () => import('../views/Project'),
    meta: { requireAuth: true },
  },
  {
    path: '/project/:id/tasks',
    name: ROUTE_TASKS_LIST,
    props: true,
    component: TasksList,
    meta: { requireAut: true },
  },
  {
    path: '/tasks/:id',
    name: ROUTE_TASK,
    props: true,
    component: Task,
    meta: { requireAut: true },
  },
];

const router = createRouter({
  history: createWebHistory(), //process.env.BASE_URL),
  routes,
});

router.beforeEach(beforeEachHandler);

export default router;
