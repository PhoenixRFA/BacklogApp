import { PROJECT_GET_ALL, PROJECT_GET, PROJECT_SELECT, PROJECT_CREATE, PROJECT_EDIT, PROJECT_ADD_USER, PROJECT_DELETE_USER, PROJECT_DELETE } from './projects.actions';
import { get, getOne, add, editName, addUser, removeUser, deleteOne } from '../services/projects.service';

export const projects = {
  namespaced: false,
  state: {
    projects: [],
    currentProjectId: '',
  },
  getters: {
    projectsSorted: (state) => state.projects.sort((a, b) => a.name.localeCompare(b.name)),
    currentProject: (state) => state.projects.find((x) => x.id === state.currentProjectId),
  },
  mutations: {
    [PROJECT_GET_ALL]: (state, projects) => (state.projects = projects),
    [PROJECT_GET]: (state, project) => {
      const idx = state.projects.findIndex((x) => x.id === project.id);
      if (~idx) {
        state.projects.splice(idx, 1, project);
      } else {
        state.projects.push(project);
      }
    },
    [PROJECT_SELECT]: (state, id) => (state.currentProjectId = id),
    [PROJECT_CREATE]: (state, newProject) => state.projects.push(newProject),
    [PROJECT_DELETE]: (state, id) => {
      const idx = state.projects.findIndex((x) => x.id === id);
      if (~idx) {
        state.projects.splice(idx, 1);
      }
    },
  },
  actions: {
    [PROJECT_GET_ALL]: async ({ commit }) => {
      try {
        const { items: projects } = await get();
        commit(PROJECT_GET_ALL, projects);
      } catch (er) {
        console.warn(er);
        //ignore errors
      }
    },
    [PROJECT_GET]: async ({ commit }, { id }) => {
      try {
        const { item: project } = await getOne(id);
        commit(PROJECT_GET, project);
      } catch (er) {
        console.warn(er);
        //ignore errors
      }
    },
    [PROJECT_SELECT]: async ({ commit, dispatch, state }, { id }) => {
      await dispatch(PROJECT_GET, { id });

      const isProjectExists = ~state.projects.findIndex((x) => x.id === id);
      if (!isProjectExists) {
        throw new Error(`Project with id: ${id}, not found`);
      }

      commit(PROJECT_SELECT, id);
    },
    [PROJECT_CREATE]: async ({ commit }, { name }) => {
      try {
        const newProject = await add(name);
        commit(PROJECT_CREATE, newProject);
        return newProject;
      } catch (er) {
        console.warn(er);
      }
    },
    [PROJECT_EDIT]: async ({ dispatch }, { id, name }) => {
      try {
        await editName(id, name);
        await dispatch(PROJECT_GET, { id });
      } catch (er) {
        console.warn(er);
      }
    },
    [PROJECT_ADD_USER]: async ({ dispatch }, { id, userEmail }) => {
      try {
        await addUser(id, userEmail);
        await dispatch(PROJECT_GET, { id });
      } catch (er) {
        console.warn(er);
      }
    },
    [PROJECT_DELETE_USER]: async ({ dispatch }, { id, userId }) => {
      try {
        await removeUser(id, userId);
        await dispatch(PROJECT_GET, { id });
      } catch (er) {
        console.wart(er);
      }
    },
    [PROJECT_DELETE]: async ({ commit }, { id }) => {
      try {
        await deleteOne(id);
        commit(PROJECT_DELETE, id);
      } catch (er) {
        console.warn(er);
      }
    },
  },
};
