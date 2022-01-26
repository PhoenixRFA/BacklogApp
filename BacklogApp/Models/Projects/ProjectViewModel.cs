using BacklogApp.Models.Auth;
using BacklogApp.Models.Db;

namespace BacklogApp.Models.Projects
{
    public record ProjectViewModel
    {
        public ProjectViewModel() { }
        public ProjectViewModel(ProjectModel proj, bool canEdit, int tasksCount, List<UserViewModel>? users = null)
        {
            Id = proj.Id;
            Name = proj.Name;
            CanEdit = canEdit;
            TasksCount = tasksCount;
            Users = users;
        }

        public string Id { get; init; } = default!;
        public string Name { get; init; } = default!;
        public bool CanEdit { get; set; }
        public int TasksCount { get; set; }
        public List<UserViewModel>? Users { get; set; }
    }
}
