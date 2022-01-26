namespace BacklogApp.Models.Auth
{
    public record LoginModel
    {
        public string? Username { get; init; }
        public string? Password { get; init; }
        public bool Remember { get; set; }
    }
}
