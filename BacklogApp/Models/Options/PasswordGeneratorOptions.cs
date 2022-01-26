namespace BacklogApp.Models.Options
{
    public record PasswordGeneratorOptions
    {
        public int RequiredLength { get; init; } = 8;
        public bool RequireNonAlphanumeric { get; init; } = true;
        public bool RequireDigit { get; init; } = true;
        public bool RequireLowercase { get; init; } = true;
        public bool RequireUppercase { get; init; } = true;
        public int RequiredUniqueChars { get; init; } = 4;
    }
}
