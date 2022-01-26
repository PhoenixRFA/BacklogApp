import { TASK_GET_ALL, TASK_GET, TASK_CREATE, TASK_EDIT, TASK_EDIT_STATUS, TASK_DELETE } from './tasks.actions';
import { get, getOne, addByFields, editByFields, editStatus, deleteOne } from '../services/tasks.service';

function parseDate(dateString) {
  return new Date(dateString);
}

export const tasks = {
  namespaced: false,
  state: {
    tasks: [],
  },
  mutations: {
    [TASK_GET_ALL]: (state, tasks) => (state.tasks = tasks),
    [TASK_GET]: (state, task) => {
      const idx = state.tasks.findIndex((x) => x.id === task.id);
      if (~idx) {
        state.tasks.splice(idx, 1, task);
      } else {
        state.tasks.push(task);
      }
    },
    [TASK_CREATE]: (state, newTask) => state.tasks.push(newTask),
    [TASK_DELETE]: (state, id) => {
      const idx = state.tasks.findIndex((x) => x.id === id);
      if (~idx) {
        state.tasks.splice(idx, 1);
      }
    },
  },
  actions: {
    [TASK_GET_ALL]: async ({ commit }, { projectId }) => {
      try {
        const { items: tasks } = await get(projectId);

        for (let t of tasks) {
          t.deadline = parseDate(t.deadline);
        }

        commit(TASK_GET_ALL, tasks);
      } catch (er) {
        console.warn(er);
      }
    },
    [TASK_GET]: async ({ commit }, { id }) => {
      try {
        const { item: task } = await getOne(id);

        task.deadline = parseDate(task.deadline);

        commit(TASK_GET, task);
      } catch (er) {
        console.warn(er);
      }
    },
    [TASK_CREATE]: async ({ commit }, { projectId, assessment, deadline, description, name, priority }) => {
      try {
        const newTask = await addByFields(projectId, name, description, deadline, priority, assessment);

        newTask.deadline = parseDate(newTask.deadline);

        commit(TASK_CREATE, newTask);
        return newTask;
      } catch (er) {
        console.warn(er);
      }
    },
    [TASK_EDIT]: async ({ dispatch }, { id, assessment, deadline, description, name, priority }) => {
      try {
        await editByFields(id, name, description, deadline, priority, assessment);
        await dispatch(TASK_GET, { id });
      } catch (er) {
        console.warn(er);
      }
    },
    [TASK_EDIT_STATUS]: async ({ dispatch }, { id, status }) => {
      try {
        await editStatus(id, status);
        await dispatch(TASK_GET, { id });
      } catch (er) {
        console.warn(er);
      }
    },
    [TASK_DELETE]: async ({ commit }, { id }) => {
      try {
        await deleteOne(id);
        commit(TASK_DELETE, id);
      } catch (er) {
        console.warn(er);
      }
    },
  },
};
