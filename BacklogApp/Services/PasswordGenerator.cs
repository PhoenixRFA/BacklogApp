using BacklogApp.Models.Options;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace BacklogApp.Services
{
    public interface IPasswordGenerator
    {
        string GeneratePassword();
        bool ValidatePassword(string password);
        string GenerateRefreshToken(string seed);
    }

    public class PasswordGenerator : IPasswordGenerator
    {
        private readonly PasswordGeneratorOptions _opts;
        private readonly RandomNumberGenerator _rng;

        public PasswordGenerator(IOptions<PasswordGeneratorOptions> options)
        {
            _rng = RandomNumberGenerator.Create();
            _opts = options.Value;
        }

        private const string UppercaseChars = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
        private const string LowercaseChars = "abcdefghijkmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private const string SpecialChars = "!@$?_-";

        public string GeneratePassword()
        {
            if (_opts.RequiredLength < 4) throw new Exception("Can't generate password. Minimum allowed length: 4 symbols");

            var chars = new char[_opts.RequiredLength];

            int i = 0;
            if (_opts.RequireUppercase)
                chars[i++] = NextChar(UppercaseChars);

            if (_opts.RequireLowercase)
                chars[i++] = NextChar(LowercaseChars);

            if (_opts.RequireDigit)
                chars[i++] = NextChar(Digits);

            if (_opts.RequireNonAlphanumeric)
                chars[i++] = NextChar(SpecialChars);

            while (i < chars.Length)
            {
                string randomChars;
                switch (Next(0, 3))
                {
                    case 0:
                        randomChars = SpecialChars;
                        break;
                    case 1:
                        randomChars = Digits;
                        break;
                    case 2:
                        randomChars = LowercaseChars;
                        break;
                    default:
                        randomChars = UppercaseChars;
                        break;
                }

                chars[i++] = NextChar(randomChars);
            }

            Shuffle(chars);

            return new string(chars);
        }

        public bool ValidatePassword(string password)
        {
            if(password == null || password.Length < _opts.RequiredLength) return false;

            bool hasLowercase = false;
            bool hasUppercase = false;
            bool hasDigit = false;
            bool hasNonAlphaNumerical = false;

            for(int i=0; i<password.Length; i++)
            {
                char c = password[i];
                if(!hasLowercase) {
                    hasLowercase = char.IsLower(c);
                    if(!LowercaseChars.Contains(c)) return false;
                }
                if(!hasUppercase) {
                    hasUppercase = char.IsUpper(c);
                    if(!UppercaseChars.Contains(c)) return false;
                }
                if(!hasDigit) {
                    hasDigit = char.IsDigit(c);
                    if(!Digits.Contains(c)) return false;
                }
                if(!hasNonAlphaNumerical) {
                    hasNonAlphaNumerical = !char.IsLetter(c) && !char.IsDigit(c);
                    if(!SpecialChars.Contains(c)) return false;
                }

                if(hasLowercase && hasUppercase && hasDigit && hasNonAlphaNumerical) break;
            }

            if(_opts.RequireLowercase && !hasLowercase) return false;
            if(_opts.RequireUppercase && !hasUppercase) return false;
            if(_opts.RequireDigit && !hasDigit) return false;
            if(_opts.RequireNonAlphanumeric && !hasNonAlphaNumerical) return false;

            if(password.Distinct().Count() < _opts.RequiredUniqueChars) return false;

            return true;
        }

        private const int RefreshTokenSeedSize = 24;
        private const int RefreshTokenSize = 64;

        public string GenerateRefreshToken(string seed)
        {
            if(seed == null) throw new ArgumentNullException(nameof(seed));
            if(seed.Length > RefreshTokenSize) throw new ArgumentException($"seed length can't be greater then {RefreshTokenSeedSize}", nameof(seed));

            byte[] seedBytes = Encoding.UTF8.GetBytes(seed);

            var randomBytes = new byte[RefreshTokenSize - RefreshTokenSeedSize];
            _rng.GetBytes(randomBytes);
            
            var resultBytes = new byte[RefreshTokenSize];
            Array.Copy(seedBytes, 0, resultBytes, RefreshTokenSeedSize - seedBytes.Length, seedBytes.Length);
            Array.Copy(randomBytes, 0, resultBytes, RefreshTokenSeedSize, randomBytes.Length);

            return Convert.ToBase64String(resultBytes);
        }

        private int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue), $"{nameof(maxValue)} must be greater then {nameof(minValue)}");
            if (minValue == maxValue) return minValue;

            var bytes = new byte[4];
            _rng.GetBytes(bytes);

            uint value = BitConverter.ToUInt32(bytes, 0);

            int diff = maxValue - minValue;

            int res = minValue + (int)(value % diff);

            return res;
        }
        private char NextChar(string source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Length == 0) throw new ArgumentException(nameof(source), $"{nameof(source)} is empty string");

            return source[Next(0, source.Length)];
        }
        public void Shuffle(char[] chars)
        {
            if (chars == null) throw new ArgumentNullException(nameof(chars));

            int i = chars.Length;

            while (i > 1)
            {
                int j = Next(0, i--);
                char t = chars[i];
                chars[i] = chars[j];
                chars[j] = t;
            }
        }

        //public string GeneratePassword()
        //{
        //    var chars = new List<char>();

        //    if (_generatorOpts.RequireUppercase)
        //        chars.Insert(
        //            Next(0, chars.Count),
        //            NextChar(UppercaseChars)
        //        );

        //    if (_generatorOpts.RequireLowercase)
        //        chars.Insert(
        //            Next(0, chars.Count),
        //            NextChar(LowercaseChars)
        //        );

        //    if (_generatorOpts.RequireDigit)
        //        chars.Insert(
        //            Next(0, chars.Count),
        //            NextChar(Digits)
        //        );

        //    if (_generatorOpts.RequireNonAlphanumeric)
        //        chars.Insert(
        //            Next(0, chars.Count),
        //            NextChar(SpecialChars)
        //        );

        //    uint iterCount = 0;
        //    for (int i = chars.Count; i < _generatorOpts.RequiredLength || chars.Distinct().Count() < _generatorOpts.RequiredUniqueChars; i++)
        //    {
        //        string randomChars;
        //        switch (Next(0, 3))
        //        {
        //            case 0:
        //                randomChars = SpecialChars;
        //                break;
        //            case 1:
        //                randomChars = Digits;
        //                break;
        //            case 2:
        //                randomChars = LowercaseChars;
        //                break;
        //            default:
        //                randomChars = UppercaseChars;
        //                break;
        //        }

        //        chars.Insert(
        //            Next(0, chars.Count),
        //            NextChar(randomChars)
        //        );

        //        iterCount++;
        //        if (iterCount > 1000) throw new Exception("Password generator exceede max iteration count: 1 000");
        //    }

        //    return new string(chars.ToArray());
        //}
    }
}
