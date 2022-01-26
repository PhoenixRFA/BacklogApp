using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace BacklogApp.Models.Options
{
    public record PasswordOptions
    {
        public KeyDerivationPrf Prf { get; init; } = KeyDerivationPrf.HMACSHA1;
        public int IterationCount { get; init; } = 1000;
        public int SaltSize { get; init; } = 128 / 8;
        public int SubkeySize { get; init; } = 256 / 8;
    }
}
