import axios from 'axios';
import { getAccessToken } from '../services/auth.service';
import store from '../store';
import { AUTH_LOGOUT, AUTH_REFRESH } from '../store/auth.actions';

let refreshTokenRequest = null;

const client = axios.create({
  //default: status >= 200 && status < 300;
  validateStatus: (status) => status < 500,
});

function isTokenValid(expires) {
  return Date.now() < expires;
}

async function requestValidAccessToken() {
  let token = getAccessToken();

  if (!isTokenValid(store.state.auth.tokenExpires)) {
    if (refreshTokenRequest === null) {
      refreshTokenRequest = store.dispatch(AUTH_REFRESH);
    }

    token = await refreshTokenRequest;
    refreshTokenRequest = null;
  }

  return token;
}

client.interceptors.request.use(async (config) => {
  if (config.skipAuth) return config;

  const accessToken = await requestValidAccessToken();

  return {
    ...config,
    headers: {
      common: {
        ['Authorization']: `Bearer ${accessToken}`,
      },
    },
  };
});

client.interceptors.response.use(
  async (response) => {
    const {
      data: { errors },
      config: { skipErrors },
      status,
    } = response;

    if (status == 401) {
      store.dispatch(AUTH_LOGOUT);
    } else if (!skipErrors && errors) {
      console.warn({ errors });
    }

    return response;
  },
  (error) => Promise.reject(error)
);

export default client;
