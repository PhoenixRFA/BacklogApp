import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import store from './store';
import apiClient from './utils/apiClient';
import { initAuth } from './services/auth.service';

import 'bootstrap';
import 'bootstrap/dist/css/bootstrap.min.css';
import { FontAwesomeIcon } from './plugins/fontAwesome';
import breadcrumns from './components/breadcrumbs';
import ClickOutside from './directives/clickOutside';
import Focus from './directives/focus';

initAuth();

const vm = createApp(App);

vm.use(store).use(router);

vm.component('font-awesome-icon', FontAwesomeIcon).component('breadcrumbs', breadcrumns);

vm.directive('click-outside', ClickOutside).directive('focus', Focus);

vm.mount('#app');

//DBG: only for debug purposes
if (process.env.NODE_ENV === 'development') {
  console.info('App in %c%s %cmode', 'color: red;', process.env.NODE_ENV.toUpperCase(), '');
  console.log({ env: process.env });
  window._$store = store;
  window._$router = router;
  window._apiClient = apiClient;
}
