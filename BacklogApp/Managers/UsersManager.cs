using BacklogApp.Models;
using BacklogApp.Models.Db;
using BacklogApp.Models.Options;
using BacklogApp.Models.Users;
using BacklogApp.Repository;
using BacklogApp.Services;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BacklogApp.Managers
{
    public class UsersManager
    {
        private readonly IUsersRepository _repo;
        private readonly IPasswordGenerator _passGenerator;
        private readonly IPasswordHasher _passHasher;
        private readonly RefreshTokenOptions _refreshTokenOpts;
        private readonly IDateTimeProvider _dt;
        private readonly IResourceRepository _resourcesRepo;

        public UsersManager(IUsersRepository repo, IResourceRepository resourcesRepo, IPasswordGenerator passGenerator, IPasswordHasher passHasher, IOptions<RefreshTokenOptions> refreshTokenOpts, IDateTimeProvider dt)
        {
            _repo = repo;
            _resourcesRepo = resourcesRepo;
            _passGenerator = passGenerator;
            _passHasher = passHasher;
            _refreshTokenOpts = refreshTokenOpts.Value;
            _dt = dt;
        }

        public UserModel? GetById(string id)
        {
            UserModel? user = _repo.GetById(id);
            
            return user;
        }
        public UserModel? GetByEmail(string email)
        {
            string emailNormalized = _normalizeEmail(email);
            UserModel? user = _repo.GetByEmail(emailNormalized);
            
            return user;
        }
        public UserModel? GetByRefreshToken(string token)
        {
            UserModel? user = _repo.GetByRefreshToken(token);

            return user;
        }

        public bool CheckPassword(UserModel user, string password)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            if (user.PasswordHash == null) return false;

            return _passHasher.VerifyPassword(password, user.PasswordHash);
        }

        public UserModel? Add(string name, string email, string? password = null)
        {
            string emailNormalized = _normalizeEmail(email);

            if (_isEmailExists(emailNormalized, normalizeInput: false)) return null;

            if (string.IsNullOrEmpty(password))
            {
                password = _passGenerator.GeneratePassword();
            }

            string passwordHash = _passHasher.HashPassword(password);
            var newUser = new UserModel
            {
                Name = name,
                Email = email,
                EmailNormalized = emailNormalized,
                PasswordHash = passwordHash
            };

            _repo.Create(newUser);

            return newUser;
        }

        public bool IsEmailExists(string email)
        {
            return _isEmailExists(email);
        }

        private bool _isEmailExists(string email, bool normalizeInput = true)
        {
            string emailNormalized = normalizeInput ? _normalizeEmail(email) : email;

            return _repo.GetByEmail(emailNormalized) != null;
        }
        private string _normalizeEmail(string email) => email.ToLowerInvariant();

        public RefreshToken? AddRefreshToken(string userId, string ip)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(ip)) throw new ArgumentNullException(nameof(ip));

            UserModel? user = _repo.GetById(userId);
            if (user == null) return null;

            user.RefreshTokens = _removeOldRefreshTokens(user.RefreshTokens);

            RefreshToken? newToken = _generateRefreshToken(userId, ip);
            user.RefreshTokens.Add(newToken);

            _repo.UpdateRefreshTokens(userId, user.RefreshTokens);

            return newToken;
        }

        private RefreshToken _generateRefreshToken(string userId, string ip)
        {
            return new RefreshToken
            {
                Created = _dt.FixedUtcNow,
                CreatedFromIp = ip,
                Expires = _dt.FixedUtcNow.AddDays(_refreshTokenOpts.Lifetime),
                Token = _passGenerator.GenerateRefreshToken(userId)
            };
        }

        public RefreshTokenResult? RotateRefreshToken(string token, string ip)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrEmpty(ip)) throw new ArgumentNullException(nameof(ip));

            UserModel? user = _repo.GetByRefreshToken(token);
            if (user == null) return null;

            RefreshToken refreshToken = user.RefreshTokens.First(x => x.Token == token);

            if (refreshToken.IsRevoked)
            {
                _revokeDescendantTokens(refreshToken, user.RefreshTokens, ip, $"Attempted to reuse of revoked ancestor token: {token}");
                _repo.UpdateRefreshTokens(user.Id, user.RefreshTokens);
                return null;
            }

            if (!refreshToken.IsActive)
            {
                return null;
            }

            user.RefreshTokens = _removeOldRefreshTokens(user.RefreshTokens);

            RefreshToken newToken = _rotateRefreshToken(refreshToken, user.Id, ip);
            user.RefreshTokens.Add(newToken);

            _repo.UpdateRefreshTokens(user.Id, user.RefreshTokens);

            return new RefreshTokenResult(newToken, user);
        }

        private void _revokeDescendantTokens(RefreshToken token, ICollection<RefreshToken> tokens, string ip, string reason)
        {
            if (string.IsNullOrEmpty(token.ReplacedBy)) return;

            RefreshToken? childToken = tokens.FirstOrDefault(x => x.Token == token.ReplacedBy);
            if (childToken == null) return;

            if (childToken.IsActive)
            {
                childToken.Revoke(ip, reason);
            }
            else
            {
                _revokeDescendantTokens(childToken, tokens, ip, reason);
            }
        }

        private List<RefreshToken> _removeOldRefreshTokens(ICollection<RefreshToken> tokens)
        {
            DateTime removeOlderThen = _dt.UtcNow.AddDays(-_refreshTokenOpts.DeleteTokensOlderThenDays);

            return tokens.Where(x => x.IsActive || removeOlderThen > x.Created).ToList();
        }

        private RefreshToken _rotateRefreshToken(RefreshToken token, string userId, string ip)
        {
            RefreshToken newRefreshToken = _generateRefreshToken(userId, ip);

            token.Revoke(ip, "replaced by new", newRefreshToken.Token);

            return newRefreshToken;
        }

        public void RevokeToken(string token, string ip)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrEmpty(ip)) throw new ArgumentNullException(nameof(ip));

            UserModel? user = _repo.GetByRefreshToken(token);
            if (user == null) return;

            user.RefreshTokens = _removeOldRefreshTokens(user.RefreshTokens);

            RefreshToken refreshToken = user.RefreshTokens.First(x => x.Token == token);
            refreshToken.Revoke(ip, "revoke without replacement");

            _repo.UpdateRefreshTokens(user.Id, user.RefreshTokens);
        }

        public void ChangeName(string id, string newName)
        {
            UserModel? user = _repo.GetById(id);
            if (user == null) return;

            if (user.Name == newName) return;

            _repo.UpdateName(id, newName);
        }

        public void ChangeEmail(string id, string newEmail)
        {
            UserModel? user = _repo.GetById(id);
            if (user == null) return;

            if (user.Email == newEmail) return;

            _repo.UpdateEmail(id, newEmail, _normalizeEmail(newEmail));
        }

        public void ChangePassword(string id, string oldPassword, string newPassword, string ip, out RefreshToken? refreshToken)
        {
            refreshToken = null;
            UserModel? user = _repo.GetById(id);
            if (user == null) return;

            if (oldPassword == newPassword || user.PasswordHash == null) return;

            if (!_passHasher.VerifyPassword(oldPassword, user.PasswordHash)) return;

            user.RefreshTokens = _removeOldRefreshTokens(user.RefreshTokens);

            foreach (RefreshToken token in user.RefreshTokens.Where(x => x.IsActive))
            {
                token.Revoke(ip, "revoke by password change");
            }
            
            RefreshToken newRefreshToken = _generateRefreshToken(user.Id, ip);
            refreshToken = newRefreshToken;
            user.RefreshTokens.Add(newRefreshToken);

            string hash = _passHasher.HashPassword(newPassword);
            _repo.UpdatePassword(id, hash, user.RefreshTokens);
        }
        public void RestorePassword(string email, string ip)
        {
            UserModel? user = _repo.GetByEmail(email);
            if (user == null) return;

            user.RefreshTokens = _removeOldRefreshTokens(user.RefreshTokens);

            foreach (RefreshToken token in user.RefreshTokens.Where(x => x.IsActive))
            {
                token.Revoke(ip, "revoke by password change");
            }

            RefreshToken newRefreshToken = _generateRefreshToken(user.Id, ip);
            user.RefreshTokens.Add(newRefreshToken);

            string password = _passGenerator.GeneratePassword();
            string hash = _passHasher.HashPassword(password);
            _repo.UpdatePassword(user.Id, hash, user.RefreshTokens);

            //TODO: send email with new password
        }

        public void AttachPhoto(string userId, string resourceId)
        {
            UserModel? user = _repo.GetById(userId);
            if (user == null) return;

            ResourceModel? resource = _resourcesRepo.GetById(resourceId);
            if (resource == null) return;

            _repo.UpdatePhoto(user.Id, resource.Id);
        }

        public void DeletePhoto(string userId)
        {
            UserModel? user = _repo.GetById(userId);
            if (user == null) return;

            _repo.UpdatePhoto(user.Id, null);
        }

        //public void Delete(string id)
        //{
        //    UsersCollection.FindOneAndDelete(x => x.Id == id);
        //}
    }
}
