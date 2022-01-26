import apiClient from '../utils/apiClient';
import { projects as projectsUrl } from '../urlBase';
import { nullReject } from '../utils/helpers';

async function get() {
  const resp = await apiClient.get(projectsUrl);

  return resp.data;
}

async function getOne(id) {
  if (!id) return nullReject('id');

  const resp = await apiClient.get(`${projectsUrl}/${id}`);

  return resp.data;
}

async function add(name) {
  const resp = await apiClient.post(projectsUrl, { name });

  return resp.data;
}

async function editName(id, name) {
  if (!id) return nullReject('id');

  await apiClient.put(`${projectsUrl}/${id}/name`, { name });
}

async function addUser(id, userEmail) {
  if (!id) return nullReject('id');
  if (!userEmail) return nullReject('userEmail');

  await apiClient.post(`${projectsUrl}/${id}/users/${userEmail}`);
}

async function removeUser(id, userId) {
  if (!id) return nullReject('id');
  if (!userId) return nullReject('userId');

  await apiClient.delete(`${projectsUrl}/${id}/users/${userId}`);
}

async function deleteOne(id) {
  if (!id) return nullReject('id');

  await apiClient.delete(`${projectsUrl}/${id}`);
}

export { get, getOne, add, editName, addUser, removeUser, deleteOne };
