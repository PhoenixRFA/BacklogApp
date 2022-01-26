namespace BacklogApp.Models.Tasks
{
    public class EditTaskModel
    {
        public string? Assessment { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public DateTime Deadline { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
    }
}
