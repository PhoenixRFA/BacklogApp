namespace BacklogApp.Models.Auth
{
    public record RegisterModel
    {
        public string? Name { get; init; }
        public string? Email { get; init; }
    }
}
