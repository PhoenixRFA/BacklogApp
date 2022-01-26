import store from '../store';
import { AUTH_RESTORE } from '../store/auth.actions';
import apiClient from '../utils/apiClient';
import { auth as authUrl } from '../urlBase';
import { nullReject } from '../utils/helpers';

const urls = {
  login: authUrl + '/login',
  logout: authUrl + '/logout',
  register: authUrl + '/register',
  refreshToken: authUrl + '/refresh-token',
  user: authUrl + '/me',
  password: authUrl + '/password',
};

let accessToken = null;

function _saveToken(token) {
  accessToken = token;
}
function _deleteToken() {
  accessToken = null;
}
function getAccessToken() {
  return accessToken;
}

async function login(username, password) {
  if (!username) nullReject('username');
  if (!password) nullReject('password');

  const resp = await apiClient.post(urls.login, { username, password }, { skipAuth: true });

  _saveToken(resp.data.token.bearer);

  return resp.data;
}

async function logout() {
  await apiClient.post(urls.logout, { skipAuth: true });

  _deleteToken();
}

async function register(name, email) {
  if (!name) nullReject('name');
  if (!email) nullReject('email');

  const resp = await apiClient.post(urls.register, { name, email }, { skipAuth: true });

  return resp.data;
}

async function getCurrentUser() {
  const resp = await apiClient.get(urls.user);

  return resp.data;
}

async function refreshToken() {
  const resp = await apiClient.post(urls.refreshToken, null, { skipAuth: true });

  _saveToken(resp.data.token.bearer);

  return resp.data;
}

function initAuth() {
  if (localStorage.getItem('wasAuthenticated') !== 'true') return;

  store.dispatch(AUTH_RESTORE);
}

async function changePassword(oldPassword, newPassword) {
  if (!oldPassword) nullReject('oldPassword');
  if (!newPassword) nullReject('newPassword');

  await apiClient.put(urls.password, { oldPassword, newPassword });
}

export { login, logout, register, getCurrentUser, refreshToken, getAccessToken, initAuth, changePassword };
