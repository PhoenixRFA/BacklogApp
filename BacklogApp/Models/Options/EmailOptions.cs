namespace BacklogApp.Models.Options
{
    public record EmailOptions
    {
        public string? Host { get; init; }
        public int SmtpPort { get; init; }
        public string? Sender { get; init; }
        public string? Username { get; init; }
        public string? Password { get; init; }
    }
}
