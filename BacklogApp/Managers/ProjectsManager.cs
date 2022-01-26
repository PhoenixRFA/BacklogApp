using BacklogApp.Models.Auth;
using BacklogApp.Models.Db;
using BacklogApp.Models.Projects;
using BacklogApp.Models.Tasks;
using BacklogApp.Repository;
using MongoDB.Bson;

namespace BacklogApp.Managers
{

    public class ProjectsManager
    {
        private readonly ITaskRepository _tasksRepo;
        private readonly IProjectRepository _projectsRepo;
        private readonly IUsersRepository _usersRepo;

        public ProjectsManager(IProjectRepository projectsRepo, ITaskRepository tasksRepo, IUsersRepository usersRepo)
        {
            _projectsRepo = projectsRepo;
            _tasksRepo = tasksRepo;
            _usersRepo = usersRepo;
        }

        public ProjectViewModel? Get(string id, string userId)
        {
            if (!_projectsRepo.IsUserInProject(id, userId)) return null;

            ProjectModel? project = _projectsRepo.GetById(id);
            if (project == null) return null;

            int count = _tasksRepo.CountTasksInProject(project.Id);

            string[] userIds = project.Users.Select(x => x.Id.ToString()!).ToArray();
            List<UserViewModel> usersInProject = _usersRepo.GetByIds(userIds).Select(x => new UserViewModel(x)).ToList();

            return new ProjectViewModel(project, project.Owner.Id == ObjectId.Parse(userId), count, usersInProject);
        }

        public List<ProjectViewModel> GetUserProjects(string userId)
        {
            List<ProjectModel> items = _projectsRepo.GetUserProjects(userId);

            string[] projectIds = items.Select(x=>x.Id).ToArray();
            List<TaskCountInProjectModel> counts = _tasksRepo.CountTasksInProjects(projectIds);

            return items.Select(x => {
                bool canEdit = x.Owner.Id == ObjectId.Parse(userId);
                int? tasksCount = counts.FirstOrDefault(c => c.Id.Id == ObjectId.Parse(x.Id))?.Count;

                return new ProjectViewModel(x, canEdit, tasksCount ?? 0);
            }).ToList();
        }

        public ProjectViewModel Create(string name, string ownerId)
        {
            ProjectModel item = _projectsRepo.Create(name, ownerId);

            return new ProjectViewModel(item, canEdit: true, tasksCount: 0);
        }

        public void ChangeName(string id, string newName, string ownerId)
        {
            ProjectModel? item = _projectsRepo.GetById(id);
            if (item == null) return;

            if (!_isOwner(item, ownerId)) return;

            if (item.Name == newName) return;

            _projectsRepo.UpdateName(item.Id, newName);
        }

        public void ChangeUsers(string id, string[] userIds, string ownerId)
        {
            ProjectModel? item = _projectsRepo.GetById(id);
            if (item == null) return;

            if (!_isOwner(item, ownerId)) return;

            //TODO: check user ids

            _projectsRepo.UpdateUsers(item.Id, userIds);
        }

        public void AddUser(string id, string userId, string ownerId)
        {
            ProjectModel? item = _projectsRepo.GetById(id);
            if (item == null) return;

            if (!_isOwner(item, ownerId)) return;

            string[] userIds = item.Users.Select(x => x.Id.ToString()!).Append(userId).ToArray();

            _projectsRepo.UpdateUsers(item.Id, userIds);
        }

        public void RemoveUser(string id, string userId, string ownerId)
        {
            ProjectModel? item = _projectsRepo.GetById(id);
            if (item == null) return;

            if (!_isOwner(item, ownerId)) return;

            string[] userIds = item.Users.Where(x => x.Id != ObjectId.Parse(userId)).Select(x => x.Id.ToString()!).ToArray();

            _projectsRepo.UpdateUsers(item.Id, userIds);
        }

        public void Delete(string id, string ownerId)
        {
            ProjectModel? item = _projectsRepo.GetById(id);
            if (item == null) return;

            if (!_isOwner(item, ownerId)) return;

            //TODO: remove all tasks from project

            _projectsRepo.Delete(item.Id);
        }

        private bool _isOwner(ProjectModel proj, string ownerId) => proj.Owner.Id == ObjectId.Parse(ownerId);
    }
}
