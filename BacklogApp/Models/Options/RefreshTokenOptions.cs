namespace BacklogApp.Models.Options
{
    public record RefreshTokenOptions
    {
        public int Lifetime { get; set; } = 7;
        public int DeleteTokensOlderThenDays { get; set; } = 14;
        public string CookieName { get; set; } = "refresh-token";
        public string SessionLifetimeCookieName { get; set; } = "extendedSession";
    }
}
