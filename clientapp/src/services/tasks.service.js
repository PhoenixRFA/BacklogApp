import apiClient from '../utils/apiClient';
import { tasks as tasksUrl } from '../urlBase';
import { nullReject } from '../utils/helpers';

async function get(projectId) {
  const resp = await apiClient.get(`${tasksUrl}/project/${projectId}`);

  return resp.data;
}

async function getOne(id) {
  if (!id) return nullReject('id');

  const resp = await apiClient.get(`${tasksUrl}/${id}`);

  return resp.data;
}

async function add(projectId, model) {
  const { name, description, deadline, priority, assessment } = model;
  return await addByFields(projectId, name, description, deadline, priority, assessment);
}
async function addByFields(projectId, name, description, deadline, priority, assessment) {
  const resp = await apiClient.post(tasksUrl, {
    projectId,
    name,
    description,
    deadline,
    priority,
    assessment,
  });

  return resp.data;
}

async function edit(id, model) {
  const { name, description, deadline, priority, assessment } = model;
  return await editByFields(id, name, description, deadline, priority, assessment);
}
async function editByFields(id, name, description, deadline, priority, assessment) {
  if (!id) return nullReject('id');

  await apiClient.put(`${tasksUrl}/${id}`, {
    name,
    description,
    deadline,
    priority,
    assessment,
  });
}

async function editStatus(id, status) {
  if (!id) return nullReject('id');
  if (!status) return nullReject('status');

  await apiClient.put(`${tasksUrl}/${id}/status/${status}`);
}

async function deleteOne(id) {
  if (!id) return nullReject('id');

  await apiClient.delete(`${tasksUrl}/${id}`);
}

export { get, getOne, add, addByFields, edit, editByFields, editStatus, deleteOne };
