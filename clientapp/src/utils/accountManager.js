import { auth as authUrl } from '../urlBase';
import { nullReject } from './helpers';
import axios from 'axios';
import store from '../store';
import { AUTH_SUCCESS, AUTH_LOGOUT } from '../store/auth';

const urls = {
  login: authUrl + '/login',
  logout: authUrl + '/logout',
  register: authUrl + '/register',
  refreshToken: authUrl + '/refresh-token',
  user: authUrl + '/me',
};

async function login(username, password) {
  if (!username) nullReject('username');
  if (!password) nullReject('password');

  const resp = await axios.post(urls.login, { username, password });

  _saveToken(resp.data.token.bearer);
  _scheduleRefreshToken(resp.data.token.expired);

  return resp.data;
}

async function register(name, email) {
  if (!name) nullReject('name');
  if (!email) nullReject('email');

  const resp = await axios.post(urls.register, { name, email });

  return resp.data;
}

async function logout() {
  await axios.post(urls.logout);

  _deleteToken();
}

async function getCurrentUser() {
  const resp = await axios.get(urls.user);

  return resp.data;
}

async function _refreshToken() {
  try {
    const resp = await axios.post(urls.refreshToken);

    _saveToken(resp.data.token.bearer);
    _scheduleRefreshToken(resp.data.token.expired);

    return resp.data;
  } catch (er) {
    if (er.status >= 500) {
      throw er;
    }

    console.log({ msg: 'can`t refresh token', error: er });
    store.commit(AUTH_LOGOUT);
    _deleteToken();
  }
}

async function init() {
  const token = _getToken();

  if (!token) {
    store.commit(AUTH_LOGOUT);
    return;
  }

  const json = localStorage.getItem(tokenExpiredKey);
  if (!json) {
    await refreshTokenAndCommit();
    return;
  }

  const date = new Date(json);
  if (isNaN(+date) || date < Date.now()) {
    await refreshTokenAndCommit();
    return;
  }

  _saveToken(token);
  const user = await getCurrentUser();

  store.commit(AUTH_SUCCESS, user.username);
  _scheduleRefreshToken(date);

  return Promise.resolve();

  async function refreshTokenAndCommit() {
    const res = await _refreshToken();
    store.commit(AUTH_SUCCESS, res.user.username);
    return;
  }
}

const tokenExpiredKey = 'token-expired';
let tokenRefreshTimeout = null;
/** Plan silent token refresh */
function _scheduleRefreshToken(expiredDate) {
  const date = new Date(expiredDate);
  const expired = date - Date.now() - 5000;

  localStorage.setItem(tokenExpiredKey, date.toJSON());

  tokenRefreshTimeout = setTimeout(async () => await _refreshToken(), expired);
}

/** Unschedule token refresh if any is planned */
function _cancelSheduledTokenRefresh() {
  if (tokenRefreshTimeout) clearTimeout(tokenRefreshTimeout);
}

const accessTokenKey = 'access-token';
/** Saves token in browser */
function _saveToken(token) {
  localStorage.setItem(accessTokenKey, token);
  axios.defaults.headers.common['Authorization'] = 'Bearer ' + token;
}
/** Retrieves token from browser */
function _getToken() {
  return localStorage.getItem(accessTokenKey);
}
/** Removes token from browser */
function _deleteToken() {
  localStorage.removeItem(accessTokenKey);
  //   if (axios.defaults.headers.common.hasOwnProperty("Authorization")) {
  //     delete axios.defaults.headers.common["Authorization"];
  //   }

  _cancelSheduledTokenRefresh();
}

export default { init, getCurrentUser, login, logout, register };
