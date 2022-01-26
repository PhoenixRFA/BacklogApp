import apiClient from '../utils/apiClient';
import { users as usersUrl, resources as resourcesUrl } from '../urlBase';
import { nullReject } from '../utils/helpers';

const urls = {
  checkEmail: usersUrl + '/emailExists',
  email: usersUrl + '/email',
  name: usersUrl + '/name',
  restorePassword: usersUrl + '/restorePassword',
  uploadPhoto: resourcesUrl + '/photo',
  photo: usersUrl + '/photo',
};

async function isEmailExists(email) {
  if (!email) nullReject('email');

  const resp = await apiClient.get(`${urls.checkEmail}/${email}`);

  return resp.data.result;
}

async function saveName(name) {
  if (!name) nullReject('name');

  await apiClient.put(urls.name, { name });
}

async function saveEmail(email) {
  if (!email) nullReject('email');

  await apiClient.put(urls.email, { email });
}

async function restorePassword(email) {
  if (!email) nullReject('email');

  await apiClient.put(urls.restorePassword, { email });
}

async function uploadPhoto(file) {
  if (!file) nullReject('file');

  var fd = new FormData();
  fd.append('file', file, file.name);

  const resp = await apiClient.post(urls.uploadPhoto, fd, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });

  const resId = resp.data.resourceId;
  if (!resId) return;

  await apiClient.put(`${urls.photo}/${resId}`);
}

async function deletePhoto() {
  await apiClient.delete(urls.photo);
}

export { isEmailExists, saveName, saveEmail, restorePassword, uploadPhoto, deletePhoto };
