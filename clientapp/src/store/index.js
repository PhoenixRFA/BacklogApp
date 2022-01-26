import { createStore } from 'vuex';
import { auth } from './auth.module';
import { projects } from './projects.module';
import { tasks } from './tasks.module';

export default createStore({
  modules: {
    auth,
    projects,
    tasks,
  },
});
