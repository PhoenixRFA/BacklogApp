namespace BacklogApp.Models.Tasks
{
    public record CreateTaskModel
    {
        public string Name { get; init; } = default!;
        public string ProjectId { get; init; } = default!;
        public string? Description { get; init; }
        public DateTime Deadline { get; init; }
        public string Priority { get; set; } = default!;
        public string? Assessment { get; init; }
    }
}
