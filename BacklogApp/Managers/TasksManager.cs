using BacklogApp.Models.Db;
using BacklogApp.Models.Tasks;
using BacklogApp.Repository;
using BacklogApp.Services;
using MongoDB.Bson;

namespace BacklogApp.Managers
{
    public class TasksManager
    {
        private readonly ITaskRepository _repo;
        private readonly IProjectRepository _projectRepo;

        public TasksManager(ITaskRepository repo, IProjectRepository projectRepo)
        {
            _repo = repo;
            _projectRepo = projectRepo;
        }

        public TaskViewModel? Get(string id, string userId)
        {
            TaskModel? task = _repo.GetById(id);
            if(task == null) return null;

            if(!IsCanAccess(task, userId)) return null;

            bool canEdit = task.CreatedBy.Id == ObjectId.Parse(userId) || _projectRepo.IsUserProjectOwner(task.Project.Id.ToString()!, userId);
            return new TaskViewModel(task, canEdit);
        }

        public List<TaskViewModel> GetTasksInProject(string projectId, string userId)
        {
            if (!IsCanAccess(projectId, userId)) return new List<TaskViewModel>();

            List<TaskModel> tasks = _repo.GetByProject(projectId);
            
            bool isProjectOwner = _projectRepo.IsUserProjectOwner(projectId, userId);

            return tasks.Select(x => {
                bool canEdit = isProjectOwner || x.CreatedBy.Id == ObjectId.Parse(userId);
                return new TaskViewModel(x, canEdit);
            }).ToList();
        }

        public TaskViewModel? Create(string name, string projectId, string userId, string? description = null, DateTime? deadline = null, string priorityCode = "medium", string? assessment = null)
        {
            if (!IsCanAccess(projectId, userId)) return null;

            var model = new TaskModel
            {
                Assessment = assessment,
                Deadline = deadline,
                Description = description,
                Name = name,
                Priority = TaskModelConverter.ToPriority(priorityCode),
                Status = TaskStatuses.Discussion
            };

            TaskModel res = _repo.Create(projectId, userId, model);

            return new TaskViewModel(res, canEdit: true);
        }

        public void Change(string id, string userId, EditTaskModel model)
        {
            TaskModel? dbTask = _repo.GetById(id);
            if(dbTask == null) return;

            if (!IsCanAccess(dbTask, userId)) return;

            dbTask.Assessment = model.Assessment;
            dbTask.Deadline = model.Deadline;
            dbTask.Description = model.Description;
            dbTask.Name = model.Name;
            dbTask.Priority = TaskModelConverter.ToPriority(model.Priority);
            
            string status = TaskModelConverter.ToStatus(model.Status);
            if (TaskModelConverter.IsStatusValid(status))
            {
                dbTask.Status = status;
            }

            _repo.Update(dbTask);
        }

        public void ChangeStatus(string id, string userId, string statusCode)
        {
            TaskModel? task = _repo.GetById(id);
            if (task == null) return;

            string projectId = task.Project.Id.ToString()!;
            if (!IsCanAccess(projectId, userId)) return;

            string status = TaskModelConverter.ToStatus(statusCode);

            if(string.IsNullOrEmpty(status)) return;

            _repo.UpdateStatus(id, status);
        }

        public void Delete(string id, string userId)
        {
            TaskModel? task = _repo.GetById(id);
            if(task == null) return;

            string projectId = task.Project.Id.ToString()!;
            bool canDelete = _projectRepo.IsUserProjectOwner(projectId, userId);

            if(!canDelete) return;

            _repo.Delete(id);
        }

        private bool IsCanAccess(string projectId, string userId) => _projectRepo.IsUserInProject(projectId, userId);
        private bool IsCanAccess(TaskModel task, string userId) => IsCanAccess(task.Project.Id.ToString()!, userId);
    }
}
