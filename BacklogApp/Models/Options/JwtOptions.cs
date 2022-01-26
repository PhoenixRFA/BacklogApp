namespace BacklogApp.Models.Options
{
    public record JwtOptions
    {
        public int Lifetime { get; init; } = 5;
        public string? Issuer { get; init; } = "JwtAuthServer";
        public string? Audience { get; init; } = "JwtAuthClient";
        public string? Key { get; init; } = "abcdefjhijklmnopqrstuvwxyz";
    }
}
