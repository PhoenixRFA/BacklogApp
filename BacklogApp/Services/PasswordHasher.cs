using BacklogApp.Models.Options;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace BacklogApp.Services
{
    public interface IPasswordHasher
    {
        public string HashPassword(string password);
        public bool VerifyPassword(string password, string hash);
    }

    public class PasswordHasher : IPasswordHasher
    {
        private readonly RandomNumberGenerator _rng;
        private readonly PasswordOptions _opts;
        private const int HEADER_SIZE = 13;

        public PasswordHasher(IOptions<PasswordOptions> options)
        {
            _opts = options.Value;
            _rng = RandomNumberGenerator.Create();
        }

        public string HashPassword(string password)
        {
            byte[] salt = new byte[_opts.SaltSize];
            _rng.GetBytes(salt);

            byte[] subkey = KeyDerivation.Pbkdf2(password, salt, _opts.Prf, _opts.IterationCount, _opts.SubkeySize);

            var outputBytes = new byte[HEADER_SIZE + salt.Length + subkey.Length];
            outputBytes[0] = 0x01;
            WriteNetworkByteOrder(outputBytes, 1, (uint)_opts.Prf);
            WriteNetworkByteOrder(outputBytes, 5, (uint)_opts.IterationCount);
            WriteNetworkByteOrder(outputBytes, 9, (uint)_opts.SaltSize);
            Buffer.BlockCopy(salt, 0, outputBytes, HEADER_SIZE, salt.Length);
            Buffer.BlockCopy(subkey, 0, outputBytes, HEADER_SIZE + salt.Length, subkey.Length);

            return Convert.ToBase64String(outputBytes);
        }

        public bool VerifyPassword(string password, string hash)
        {
            bool res = false;

            byte[] hashBytes = Convert.FromBase64String(hash);

            if (hashBytes[0] != 0x01) return res;

            KeyDerivationPrf passPrf = (KeyDerivationPrf)ReadNetworkByteOrder(hashBytes, 1);
            if (passPrf != _opts.Prf) return res;

            int passIterCount = (int)ReadNetworkByteOrder(hashBytes, 5);
            if (passIterCount != _opts.IterationCount) return res;

            int passSaltSize = (int)ReadNetworkByteOrder(hashBytes, 9);
            if (passSaltSize != _opts.SaltSize) return res;
            byte[] salt = new byte[_opts.SaltSize];
            Buffer.BlockCopy(hashBytes, HEADER_SIZE, salt, 0, salt.Length);

            int passSubkeySize = hashBytes.Length - salt.Length - HEADER_SIZE;
            if (passSubkeySize < _opts.SubkeySize) return res;

            byte[] subkey = new byte[passSubkeySize];
            Buffer.BlockCopy(hashBytes, HEADER_SIZE + salt.Length, subkey, 0, passSubkeySize);

            byte[] realSubkey = KeyDerivation.Pbkdf2(password, salt, _opts.Prf, _opts.IterationCount, _opts.SubkeySize);

            res = subkey.SequenceEqual(realSubkey);
            return res;
        }

        private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value >> 0);
        }
        private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
        {
            return ((uint)(buffer[offset + 0]) << 24)
                 | ((uint)(buffer[offset + 1]) << 16)
                 | ((uint)(buffer[offset + 2]) << 8)
                 | ((uint)(buffer[offset + 3]));
        }
    }
}
