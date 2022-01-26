import { watch } from 'vue';
import store from '../store';
import { ROUTE_LOGIN, ROUTE_HOME } from './routeNames';

/** Wait until authentication action is completed and check auth state. */
const isUserAuthenticated = () =>
  new Promise((resolve) => {
    if (store.getters.isAuthLoading) {
      watch(
        () => store.getters.isAuthLoading,
        (val) => {
          if (!val) resolve(store.getters.isAuthenticated);
        }
      );

      return;
    }

    resolve(store.getters.isAuthenticated);
  });

async function beforeEachHandler(to) {
  if (!(to.meta.requireAuth || to.meta.requireAnon)) return;

  const isAuthenticated = await isUserAuthenticated(to);
  if (to.meta.requireAuth && !isAuthenticated) return { name: ROUTE_LOGIN };
  if (to.meta.requireAnon && isAuthenticated) return { name: ROUTE_HOME };
}

export { beforeEachHandler };
