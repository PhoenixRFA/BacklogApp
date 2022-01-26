namespace BacklogApp.Models.Projects
{
    public record ChangeProjectNameModel
    {
        public string Name { get; init; } = default!;
    }
}
