namespace BacklogApp.Models.Projects
{
    public record CreateProjectModel
    {
        public string Name { get; init; } = default!;
    }
}
