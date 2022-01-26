using BacklogApp.Models.Db;
using BacklogApp.Services;

namespace BacklogApp.Models.Tasks
{
    public record TaskViewModel
    {
        public TaskViewModel() { }

        public TaskViewModel(TaskModel task, bool canEdit)
        {
            Id = task.Id;
            Assessment = task.Assessment;
            Priority = TaskModelConverter.ToPriorityCode(task.Priority);
            Deadline = task.Deadline;
            Description = task.Description;
            Name = task.Name;
            Status = TaskModelConverter.ToStatusCode(task.Status);
            ProjectId = task.Project.Id.ToString();
            
            CanEdit = canEdit;
        }

        public string Id { get; init; } = default!;
        public string? Assessment { get; set; }
        public string? Priority { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Description { get; set; }
        public string Name { get; set; } = default!;
        public string? Status { get; set; }
        public string? ProjectId { get; set; }
        
        public bool CanEdit { get; set; }
    }
}
