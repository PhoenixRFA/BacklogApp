import { AUTH_REQUEST, AUTH_SUCCESS, AUTH_ERROR, AUTH_LOGOUT, AUTH_REFRESH, AUTH_RESTORE } from './auth.actions';
import { STATUS_ERROR, STATUS_LOADING, STATUS_LOGOUT, STATUS_SUCCESS, STATUS_TOKEN_REFRESH } from './auth.statuses';
import { ROUTE_LOGIN } from '../router/routeNames';
import { login, logout, refreshToken } from '../services/auth.service';
import router from '../router';

export const auth = {
  namespaced: false,
  state: {
    status: '',
    user: null,
    tokenExpires: 0,
  },
  getters: {
    isAuthenticated: (state) => state.status === STATUS_SUCCESS || state.status === STATUS_TOKEN_REFRESH,
    isAuthLoading: (state) => state.status === STATUS_LOADING || state.status === STATUS_TOKEN_REFRESH,
    photo: (state) => (state.user ? state.user.photo : ''),
  },
  mutations: {
    [AUTH_REQUEST]: (state) => {
      state.status = STATUS_LOADING;
    },
    [AUTH_SUCCESS]: (state, { user, tokenExpires }) => {
      state.status = STATUS_SUCCESS;
      state.user = user;
      state.tokenExpires = Date.parse(tokenExpires);
      localStorage.setItem('wasAuthenticated', 'true');
    },
    [AUTH_ERROR]: (state) => {
      state.status = STATUS_ERROR;
      state.tokenExpires = 0;
      localStorage.removeItem('wasAuthenticated');
    },
    [AUTH_LOGOUT]: (state) => {
      state.status = STATUS_LOGOUT;
      state.user = null;
      state.tokenExpires = 0;

      localStorage.removeItem('wasAuthenticated');
      if (router.currentRoute.name != ROUTE_LOGIN) {
        router.push({ name: ROUTE_LOGIN });
      }
    },
    [AUTH_REFRESH]: (state) => {
      state.status = STATUS_TOKEN_REFRESH;
    },
  },
  actions: {
    [AUTH_REQUEST]: async ({ commit }, { username, password }) => {
      commit(AUTH_REQUEST);
      try {
        const res = await login(username, password);
        commit(AUTH_SUCCESS, {
          user: res.user,
          tokenExpires: res.token.expired,
        });
      } catch (er) {
        commit(AUTH_ERROR);
        throw er;
      }
    },
    [AUTH_LOGOUT]: async ({ commit }) => {
      commit(AUTH_REQUEST);
      await logout();
      commit(AUTH_LOGOUT);
    },
    [AUTH_REFRESH]: async ({ commit }) => {
      commit(AUTH_REFRESH);
      try {
        const res = await refreshToken();
        commit(AUTH_SUCCESS, {
          user: res.user,
          tokenExpires: res.token.expired,
        });
        return res.token.bearer;
      } catch (er) {
        commit(AUTH_ERROR);
        throw er;
      }
    },
    [AUTH_RESTORE]: async ({ commit }) => {
      commit(AUTH_REQUEST);
      try {
        const res = await refreshToken();
        commit(AUTH_SUCCESS, {
          user: res.user,
          tokenExpires: res.token.expired,
        });
      } catch (er) {
        commit(AUTH_ERROR);
        throw er;
      }
    },
  },
};
