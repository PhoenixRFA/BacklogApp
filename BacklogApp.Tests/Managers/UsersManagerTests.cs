using BacklogApp.Managers;
using BacklogApp.Models.Db;
using BacklogApp.Models.Options;
using BacklogApp.Models.Users;
using BacklogApp.Repository;
using BacklogApp.Services;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace BacklogApp.Tests.Managers
{
    public class UsersManagerTests
    {
        [Theory, AutoMoqData]
        public void GetById_ReturnsUser(UserModel user, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetById(user.Id))
                .Returns(user);

            UsersManager mng = new(repo.Object, resourceRepo.Object, null!, null!, opts.Object, null!);

            UserModel? res = mng.GetById(user.Id);

            Assert.NotNull(res);
            Assert.Equal(user.Id, res!.Id);
            Assert.Equal(user.Email, res.Email);
            Assert.Equal(user.Name, res.Name);
            Assert.Equal(user.PasswordHash, res.PasswordHash);
        }


        [Theory, AutoMoqData]
        public void CheckPassword_Calls_PasswordHasher_Once(UserModel user, string password, Mock<IPasswordHasher> hasher, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            UsersManager mng = new(null!, null!, null!, hasher.Object, opts.Object, null!);

            mng.CheckPassword(user, password);

            hasher.Verify(x => x.VerifyPassword(password, user.PasswordHash!), Times.Once);
        }

        [Theory, AutoMoqData]
        public void CheckPassword_ReturnsTrue_OnCorrectPassword(UserModel user, string password, Mock<IPasswordHasher> hasher, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            hasher.Setup(x => x.VerifyPassword(password, user.PasswordHash!)).Returns(true);
            UsersManager mng = new(null!, null!, null!, hasher.Object, opts.Object, null!);

            bool res = mng.CheckPassword(user, password);

            Assert.True(res);
        }


        [Theory, AutoMoqData]
        public void Add_Checks_IfEmail_Exists(string name, string email, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IPasswordHasher> passHasher, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, null!);

            mng.Add(name, email);

            repo.Verify(x => x.GetByEmail(email), Times.Once());
        }

        [Theory, AutoMoqData]
        public void Add_Check_NormalizedEmail(string name, string email, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IPasswordHasher> passHasher, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            string passedEmail = null!;
            repo.Setup(x => x.GetByEmail(It.IsAny<string>()))
                .Callback((string email) => passedEmail = email);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, null!);

            mng.Add(name, email);

            Assert.NotNull(passedEmail);
            Assert.Equal(passedEmail, email.ToLowerInvariant());
        }

        [Theory, AutoMoqData]
        public void Add_Calls_GeneratePassword_Once(string name, string email, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IPasswordHasher> passHasher, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetByEmail(It.IsAny<string>()))
                .Returns((UserModel?)null);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, null!);

            mng.Add(name, email);

            passGenerator.Verify(x => x.GeneratePassword(), Times.Once());
        }

        [Theory, AutoMoqData]
        public void Add_Calls_HashPassword_Once(string name, string email, string password, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IPasswordHasher> passHasher, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetByEmail(It.IsAny<string>()))
                .Returns((UserModel?)null);
            passGenerator.Setup(x => x.GeneratePassword())
                .Returns(password);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, null!);

            mng.Add(name, email);

            passHasher.Verify(x => x.HashPassword(password), Times.Once());
        }

        [Theory, AutoMoqData]
        public void Add_Insert_CorrectModel(string name, string email, string password, string passHash, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IPasswordHasher> passHasher, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetByEmail(It.IsAny<string>()))
                .Returns((UserModel?)null);
            UserModel passedUser = null!;
            repo.Setup(x => x.Create(It.IsAny<UserModel>()))
                .Callback((UserModel user) => passedUser = user);
            passGenerator.Setup(x => x.GeneratePassword()).Returns(password);
            passHasher.Setup(x => x.HashPassword(password)).Returns(passHash);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, null!);

            UserModel? res = mng.Add(name, email);

            Assert.NotNull(res);
            Assert.Equal(res!.Email, email);
            Assert.Equal(res.Name, name);
            Assert.Equal(res.PasswordHash, passHash);

            Assert.NotNull(passedUser);
            Assert.Equal(passedUser, res);

            repo.Verify(x => x.Create(res), Times.Once);
        }


        [Theory, AutoMoqData]
        public void AddRefreshToken_Calls_GetById_Once(string id, string ip, DateTime now, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IOptions<RefreshTokenOptions>> opts, Mock<IDateTimeProvider> dt)
        {
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, null!, opts.Object, dt.Object);

            mng.AddRefreshToken(id, ip);

            repo.Verify(x => x.GetById(id), Times.Once);
        }

        [Theory, AutoMoqData]
        public void AddRefreshToken_Generates_ValidToken(UserModel user, string token, string ip, DateTime now, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetById(It.IsAny<string>()))
                .Returns(user);
            passGenerator.Setup(x => x.GenerateRefreshToken(It.IsAny<string>()))
                .Returns(token);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);
            dt.SetupGet(x => x.FixedUtcNow)
                .Returns(now);
            opts.SetupGet(x => x.Value)
                .Returns(new RefreshTokenOptions { Lifetime = 7 });

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, null!, opts.Object, dt.Object);

            RefreshToken? res = mng.AddRefreshToken(user.Id, ip);

            Assert.NotNull(res!.Token);
            Assert.Equal(token, res.Token);
            Assert.Equal(now, res.Created);
            Assert.Equal(now.AddDays(7), res.Expires);
            Assert.Equal(ip, res.CreatedFromIp);
            Assert.Null(res.Revoked);
            Assert.Null(res.RevokedFromIp);
            Assert.Null(res.Reason);
        }

        [Theory, AutoMoqData]
        public void AddRefreshToken_PassedCorrectSeed_ToPassGenerator(UserModel user, string ip, DateTime now, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetById(It.IsAny<string>()))
                .Returns(user);
            string passedSeed = null!;
            passGenerator.Setup(x => x.GenerateRefreshToken(It.IsAny<string>()))
                .Callback((string seed) => passedSeed = seed);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);
            dt.SetupGet(x => x.FixedUtcNow)
                .Returns(now);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, null!, opts.Object, dt.Object);

            mng.AddRefreshToken(user.Id, ip);

            Assert.NotNull(passedSeed);
            Assert.Equal(passedSeed, user.Id);
        }

        [Theory, AutoMoqData]
        public void AddRefreshToken_PutsNewRefreshToken_ToUser(UserModel user, string ip, DateTime now, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetById(It.IsAny<string>()))
                .Returns(user);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);
            dt.SetupGet(x => x.FixedUtcNow)
                .Returns(now);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, null!, opts.Object, dt.Object);

            RefreshToken? res = mng.AddRefreshToken(user.Id, ip);

            Assert.Contains(res, user.RefreshTokens);
        }

        [Theory, AutoMoqData]
        public void AddRefreshToken_Calls_UpdateRefreshTokes_Once(UserModel user, string ip, DateTime now, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetById(It.IsAny<string>()))
                .Returns(user);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, null!, opts.Object, dt.Object);

            RefreshToken? res = mng.AddRefreshToken(user.Id, ip);

            repo.Verify(x => x.UpdateRefreshTokens(user.Id, It.IsAny<IEnumerable<RefreshToken>>()), Times.Once());
        }

        [Theory, AutoMoqData]
        public void AddRefreshToken_Removes_OldTokens(DateTime now, string token, string ip, UserModel user, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            SetupForRemovesOldTokens(now, token, user, repo, opts, dt, out RefreshToken expiredToken, out RefreshToken notExpiredToken);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, null!, opts.Object, dt.Object);
            mng.AddRefreshToken(user.Id, ip);

            Assert.Contains(notExpiredToken, user.RefreshTokens);
            Assert.DoesNotContain(expiredToken, user.RefreshTokens);
        }


        [Theory, AutoMoqData]
        public void RotateRefreshToken_Calls_GetByRefreshToken_Once(string token, string ip, UserModel user, RefreshToken refreshToken, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            refreshToken.Token = token;

            repo.Setup(x => x.GetByRefreshToken(token))
                .Returns(user);
            user.RefreshTokens.Add(refreshToken);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, null!, opts.Object, dt.Object);

            mng.RotateRefreshToken(token, ip);

            repo.Verify(x => x.GetByRefreshToken(token), Times.Once());
        }

        [Theory, AutoMoqData]
        public void RotateRefreshToken_Removes_DescendantTokens_IfRevoked(DateTime now, DateTime revoked, string ip, UserModel user, RefreshToken token, RefreshToken childToken, RefreshToken innerChildToken, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            DateTime notExpired = now.AddDays(1);

            token.Revoked = revoked;
            token.ReplacedBy = childToken.Token;
            token.Reason = "reason";

            childToken.Expires = notExpired;
            childToken.Revoked = revoked;
            childToken.ReplacedBy = innerChildToken.Token;
            childToken.Reason = null;

            innerChildToken.Expires = notExpired;
            innerChildToken.Revoked = null;
            innerChildToken.ReplacedBy = null;
            innerChildToken.Reason = null;

            user.RefreshTokens = new List<RefreshToken> { token, childToken, innerChildToken };

            repo.Setup(x => x.GetByRefreshToken(token.Token))
                .Returns(user);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);
            SystemTime.SetDateTimeProvider(dt.Object);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, null!, opts.Object, dt.Object);

            mng.RotateRefreshToken(token.Token, ip);

            Assert.NotNull(token.Revoked);
            Assert.NotNull(childToken.Revoked);
            Assert.NotNull(innerChildToken.Revoked);
        }

        [Theory, AutoMoqData]
        public void RotateRefreshToken_ReturnsNull_IfToken_IsRevoked(DateTime revoked, string ip, UserModel user, RefreshToken token, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            token.Revoked = revoked;
            user.RefreshTokens = new List<RefreshToken> { token };

            repo.Setup(x => x.GetByRefreshToken(token.Token))
                .Returns(user);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, null!, opts.Object, dt.Object);

            RefreshTokenResult? res = mng.RotateRefreshToken(token.Token, ip);

            Assert.Null(res);
        }

        [Theory, AutoMoqData]
        public void RotateRefreshToken_ReturnsNull_IfToken_IsNotActive(DateTime now, string ip, UserModel user, RefreshToken token, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            DateTime expired = now.AddDays(-1);
            token.Revoked = null;
            token.Expires = expired;
            user.RefreshTokens = new List<RefreshToken> { token };

            repo.Setup(x => x.GetByRefreshToken(token.Token))
                .Returns(user);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);
            SystemTime.SetDateTimeProvider(dt.Object);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, null!, opts.Object, dt.Object);

            RefreshTokenResult? res = mng.RotateRefreshToken(token.Token, ip);

            Assert.Null(res);
        }

        [Theory, AutoMoqData]
        public void RotateRefreshToken_Removes_OldTokens(DateTime now, string token, string ip, UserModel user, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            SetupForRemovesOldTokens(now, token, user, repo, opts, dt, out RefreshToken expiredToken, out RefreshToken notExpiredToken);
            repo.Setup(x => x.GetByRefreshToken(token))
                .Returns(user);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, null!, opts.Object, dt.Object);

            mng.RotateRefreshToken(token, ip);

            Assert.Contains(notExpiredToken, user.RefreshTokens);
            Assert.DoesNotContain(expiredToken, user.RefreshTokens);
        }

        [Theory, AutoMoqData]
        public void RotateRefreshToken_RotatesToken(DateTime now, string token, string newToken, string ip, UserModel user, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            DateTime notExpired = now.AddDays(1);

            RefreshToken refreshToken = new()
            {
                Created = now,
                Expires = notExpired,
                Token = token
            };

            user.RefreshTokens = new List<RefreshToken> { refreshToken };

            repo.Setup(x => x.GetByRefreshToken(token))
                .Returns(user);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);
            passGenerator.Setup(x => x.GenerateRefreshToken(It.IsAny<string>()))
                .Returns(newToken);
            SystemTime.SetDateTimeProvider(dt.Object);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, null!, opts.Object, dt.Object);

            RefreshTokenResult? res = mng.RotateRefreshToken(token, ip);

            Assert.NotNull(res);
            Assert.Equal(refreshToken.ReplacedBy, res!.Token.Token);
            Assert.Equal(refreshToken.Revoked, now);
            Assert.NotNull(refreshToken.Reason);
        }

        [Theory, AutoMoqData]
        public void RotateRefreshToken_Add_NewToken_ToUserTokens(DateTime now, string token, string newToken, string ip, UserModel user, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            DateTime notExpired = now.AddDays(1);

            RefreshToken refreshToken = new()
            {
                Created = now,
                Expires = notExpired,
                Token = token
            };

            user.RefreshTokens = new List<RefreshToken> { refreshToken };

            repo.Setup(x => x.GetByRefreshToken(token))
                .Returns(user);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);
            passGenerator.Setup(x => x.GenerateRefreshToken(It.IsAny<string>()))
                .Returns(newToken);
            SystemTime.SetDateTimeProvider(dt.Object);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, null!, opts.Object, dt.Object);

            RefreshTokenResult? res = mng.RotateRefreshToken(token, ip);

            Assert.NotNull(res);
            Assert.Contains(res!.Token, user.RefreshTokens);
        }

        [Theory, AutoMoqData]
        public void RotateRefreshToken_Calls_UpdateRefreshTokens_Once(DateTime now, string token, string newToken, string ip, UserModel user, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            DateTime notExpired = now.AddDays(1);

            RefreshToken refreshToken = new()
            {
                Created = now,
                Expires = notExpired,
                Token = token
            };

            user.RefreshTokens = new List<RefreshToken> { refreshToken };

            repo.Setup(x => x.GetByRefreshToken(token))
                .Returns(user);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);
            passGenerator.Setup(x => x.GenerateRefreshToken(It.IsAny<string>()))
                .Returns(newToken);
            SystemTime.SetDateTimeProvider(dt.Object);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, null!, opts.Object, dt.Object);

            mng.RotateRefreshToken(token, ip);

            repo.Verify(x => x.UpdateRefreshTokens(user.Id, It.IsAny<IEnumerable<RefreshToken>>()), Times.Once());
        }

        [Theory, AutoMoqData]
        public void RotateRefreshToken_Returns_CorrectResult(DateTime now, string token, string newToken, string ip, UserModel user, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            DateTime notExpired = now.AddDays(1);

            RefreshToken refreshToken = new()
            {
                Created = now,
                Expires = notExpired,
                Token = token
            };

            user.RefreshTokens = new List<RefreshToken> { refreshToken };

            repo.Setup(x => x.GetByRefreshToken(token))
                .Returns(user);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);
            passGenerator.Setup(x => x.GenerateRefreshToken(It.IsAny<string>()))
                .Returns(newToken);
            SystemTime.SetDateTimeProvider(dt.Object);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, null!, opts.Object, dt.Object);

            RefreshTokenResult? res = mng.RotateRefreshToken(token, ip);

            RefreshToken? newRefreshToken = user.RefreshTokens.Find(x => x.Token == newToken);

            Assert.NotNull(res);
            Assert.Equal(user, res!.User);
            Assert.Equal(newRefreshToken, res.Token);
        }


        [Theory, AutoMoqData]
        public void RevokeToken_Calls_GetByRefreshToken_Once(UserModel user, string token, string ip, DateTime now, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            user.RefreshTokens.Add(
                new RefreshToken { Token = token, Created = now, CreatedFromIp = ip, Expires = now.AddDays(1) }
            );

            repo.Setup(x => x.GetByRefreshToken(token))
                .Returns(user);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);
            SystemTime.SetDateTimeProvider(dt.Object);

            UsersManager mng = new(repo.Object, resourceRepo.Object, null!, null!, opts.Object, dt.Object);

            mng.RevokeToken(token, ip);

            repo.Verify(x => x.GetByRefreshToken(token), Times.Once());
        }

        [Theory, AutoMoqData]
        public void RevokeToken_RevokesToken(string token, string ip, DateTime now, UserModel user, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            var refreshToken = new RefreshToken
            {
                Token = token,
                Created = now,
                CreatedFromIp = ip,
                Expires = now.AddDays(1)
            };

            user.RefreshTokens = new List<RefreshToken> { refreshToken };

            repo.Setup(x => x.GetByRefreshToken(token))
                .Returns(user);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);
            SystemTime.SetDateTimeProvider(dt.Object);

            UsersManager mng = new(repo.Object, resourceRepo.Object, null!, null!, opts.Object, dt.Object);

            mng.RevokeToken(token, ip);

            Assert.Equal(now, refreshToken.Revoked);
            Assert.Equal(ip, refreshToken.RevokedFromIp);
            Assert.NotNull(refreshToken.Reason);
        }

        [Theory, AutoMoqData]
        public void RevokeToken_Calls_UpdateRefreshTokens_Once(string token, string ip, UserModel user, DateTime now, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            user.RefreshTokens.Add(
                new RefreshToken { Token = token, Created = now, CreatedFromIp = ip, Expires = now.AddDays(1) }
            );

            repo.Setup(x => x.GetByRefreshToken(token))
                .Returns(user);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);
            SystemTime.SetDateTimeProvider(dt.Object);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, null!, opts.Object, dt.Object);

            mng.RevokeToken(token, ip);

            repo.Verify(x => x.UpdateRefreshTokens(user.Id, It.IsAny<IEnumerable<RefreshToken>>()));
        }

        [Theory, AutoMoqData]
        public void RevokeToken_Removes_OldTokens(string token, string ip, DateTime now, UserModel user, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            SetupForRemovesOldTokens(now, token, user, repo, opts, dt, out RefreshToken expiredToken, out RefreshToken notExpiredToken);
            user.RefreshTokens.Add(new RefreshToken { Token = token, Created = now, CreatedFromIp = ip, Expires = now, Revoked = null });

            repo.Setup(x => x.GetByRefreshToken(token))
                .Returns(user);

            UsersManager mng = new(repo.Object, resourceRepo.Object, null!, null!, opts.Object, dt.Object);

            mng.RevokeToken(token, ip);

            Assert.Contains(notExpiredToken, user.RefreshTokens);
            Assert.DoesNotContain(expiredToken, user.RefreshTokens);
        }


        [Theory, AutoMoqData]
        public void ChangeEmail_Calls_GetById_Once(string id, string email, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            UsersManager mng = new(repo.Object, resourceRepo.Object, null!, null!, opts.Object, dt.Object);

            mng.ChangeEmail(id, email);

            repo.Verify(x => x.GetById(id), Times.Once());
        }

        [Theory, AutoMoqData]
        public void ChangeEmail_Calls_UpdateEmail_Once(UserModel user, string newEmail, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetById(user.Id))
                .Returns(user);

            UsersManager mng = new(repo.Object, resourceRepo.Object, null!, null!, opts.Object, dt.Object);

            mng.ChangeEmail(user.Id, newEmail);

            repo.Verify(x => x.UpdateEmail(user.Id, It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Theory, AutoMoqData]
        public void ChangeEmail_Calls_UpdateEmail_WithCorrectParameters(UserModel user, string newEmail, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetById(user.Id))
                .Returns(user);
            string passedId = null!, passedEmail = null!, passedEmailNormalized = null!;
            repo.Setup(x => x.UpdateEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((id, email, emailNormalized) =>
                {
                    passedId = id;
                    passedEmail = email;
                    passedEmailNormalized = emailNormalized;
                });

            UsersManager mng = new(repo.Object, resourceRepo.Object, null!, null!, opts.Object, dt.Object);

            mng.ChangeEmail(user.Id, newEmail);

            Assert.Equal(user.Id, passedId);
            Assert.Equal(newEmail, passedEmail);
            Assert.Equal(newEmail.ToLowerInvariant(), passedEmailNormalized);
        }


        [Theory, AutoMoqData]
        public void ChangeName_Calls_GetById_Once(string id, string name, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            UsersManager mng = new(repo.Object, resourceRepo.Object, null!, null!, opts.Object, dt.Object);

            mng.ChangeName(id, name);

            repo.Verify(x => x.GetById(id), Times.Once());
        }

        [Theory, AutoMoqData]
        public void ChangeName_Calls_UpdateName_Once(UserModel user, string newName, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetById(user.Id))
                .Returns(user);

            UsersManager mng = new(repo.Object, resourceRepo.Object, null!, null!, opts.Object, dt.Object);

            mng.ChangeName(user.Id, newName);

            repo.Verify(x => x.UpdateName(user.Id, It.IsAny<string>()), Times.Once());
        }

        [Theory, AutoMoqData]
        public void ChangeName_Calls_UpdateEmail_WithCorrectParameters(UserModel user, string newName, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetById(user.Id))
                .Returns(user);
            string passedId = null!, passedName = null!;
            repo.Setup(x => x.UpdateName(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((id, name) =>
                {
                    passedId = id;
                    passedName = name;
                });

            UsersManager mng = new(repo.Object, resourceRepo.Object, null!, null!, opts.Object, dt.Object);

            mng.ChangeName(user.Id, newName);

            Assert.Equal(user.Id, passedId);
            Assert.Equal(newName, passedName);
        }


        [Theory, AutoMoqData]
        public void ChangePassword_Calls_GetById_Once(string id, string oldPassword, string newPassword, string ip, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            UsersManager mng = new(repo.Object, resourceRepo.Object, null!, null!, opts.Object, dt.Object);

            mng.ChangePassword(id, oldPassword, newPassword, ip, out _);

            repo.Verify(x => x.GetById(id), Times.Once());
        }

        [Theory, AutoMoqData]
        public void ChangePassword_Calls_VerifyPassword_Once(UserModel user, string oldPassword, string newPassword, string ip, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordHasher> passHasher, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetById(user.Id))
                .Returns(user);

            UsersManager mng = new(repo.Object, resourceRepo.Object, null!, passHasher.Object, opts.Object, dt.Object);

            mng.ChangePassword(user.Id, oldPassword, newPassword, ip, out _);

            passHasher.Verify(x => x.VerifyPassword(oldPassword, user.PasswordHash!), Times.Once());
        }

        [Theory, AutoMoqData]
        public void ChangePassword_Removes_OldTokens(UserModel user, string token, DateTime now, string oldPassword, string newPassword, string ip, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IPasswordHasher> passHasher, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            SetupForRemovesOldTokens(now, token, user, repo, opts, dt, out RefreshToken expiredToken, out RefreshToken notExpiredToken);
            passHasher.Setup(x => x.VerifyPassword(oldPassword, user.PasswordHash!))
                .Returns(true);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, dt.Object);

            mng.ChangePassword(user.Id, oldPassword, newPassword, ip, out _);

            Assert.Contains(notExpiredToken, user.RefreshTokens);
            Assert.DoesNotContain(expiredToken, user.RefreshTokens);
        }

        [Theory, AutoMoqData]
        public void ChangePassword_Revokes_ActiveTokens(UserModel user, string oldPassword, string newPassword, string ip, string token, DateTime now, DateTime revoked, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IPasswordHasher> passHasher, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            SetupForRevokeActiveTokens(token, ip, now, revoked, user, repo, dt, out RefreshToken activeToken1, out RefreshToken activeToken2, out RefreshToken inactiveToken);
            passHasher.Setup(x => x.VerifyPassword(oldPassword, user.PasswordHash!))
                .Returns(true);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, dt.Object);

            mng.ChangePassword(user.Id, oldPassword, newPassword, ip, out _);

            Assert.Equal(now, activeToken1.Revoked);
            Assert.Equal(now, activeToken2.Revoked);
            Assert.Equal(revoked, inactiveToken.Revoked);
        }

        [Theory, AutoMoqData]
        public void ChangePassword_Generates_NewRefreshToken(UserModel user, string token, string oldPassword, string newPassword, string ip, DateTime now, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordHasher> passHasher, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetById(user.Id))
                .Returns(user);
            passHasher.Setup(x => x.VerifyPassword(oldPassword, user.PasswordHash!))
                .Returns(true);
            passGenerator.Setup(x => x.GenerateRefreshToken(user.Id))
                .Returns(token);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, dt.Object);

            mng.ChangePassword(user.Id, oldPassword, newPassword, ip, out _);

            RefreshToken? refreshToken = user.RefreshTokens.Find(x => x.Token == token);

            Assert.NotNull(refreshToken);
        }

        [Theory, AutoMoqData]
        public void ChangePassword_Calls_HashPassword_Once(UserModel user, string oldPassword, string newPassword, string ip, DateTime now, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordHasher> passHasher, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetById(user.Id))
                .Returns(user);
            passHasher.Setup(x => x.VerifyPassword(oldPassword, user.PasswordHash!))
                .Returns(true);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, dt.Object);

            mng.ChangePassword(user.Id, oldPassword, newPassword, ip, out _);

            passHasher.Verify(x => x.HashPassword(newPassword), Times.Once());
        }

        [Theory, AutoMoqData]
        public void ChangePassword_Calls_UpdatePassword_Once(UserModel user, string oldPassword, string newPassword, string ip, string hash, DateTime now, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordHasher> passHasher, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetById(user.Id))
                .Returns(user);
            passHasher.Setup(x => x.VerifyPassword(oldPassword, user.PasswordHash!))
                .Returns(true);
            passHasher.Setup(x => x.HashPassword(newPassword))
                .Returns(hash);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, dt.Object);

            mng.ChangePassword(user.Id, oldPassword, newPassword, ip, out _);

            repo.Verify(x => x.UpdatePassword(user.Id, hash, It.IsAny<IEnumerable<RefreshToken>>()), Times.Once());
        }

        [Theory, AutoMoqData]
        public void ChangePassword_Returns_RefreshToken(UserModel user, string oldPassword, string newPassword, string ip, string hash, string refreshToken, DateTime now, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordHasher> passHasher, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetById(user.Id))
                .Returns(user);
            passHasher.Setup(x => x.VerifyPassword(oldPassword, user.PasswordHash!))
                .Returns(true);
            passHasher.Setup(x => x.HashPassword(newPassword))
                .Returns(hash);
            passGenerator.Setup(x => x.GenerateRefreshToken(user.Id))
                .Returns(refreshToken);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, dt.Object);

            mng.ChangePassword(user.Id, oldPassword, newPassword, ip, out RefreshToken? token);

            Assert.NotNull(token);
            Assert.Equal(refreshToken, token!.Token);
        }


        [Theory, AutoMoqData]
        public void RestorePassword_Calls_GetByEmail_Once(string email, string ip, DateTime now, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IPasswordHasher> passHasher, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, dt.Object);

            mng.RestorePassword(email, ip);

            repo.Verify(x => x.GetByEmail(email), Times.Once());
        }

        [Theory, AutoMoqData]
        public void RestorePassword_Removes_OldTokens(UserModel user, string token, DateTime now, string ip, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordGenerator> passGenerator, Mock<IPasswordHasher> passHasher, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            SetupForRemovesOldTokens(now, token, user, repo, opts, dt, out RefreshToken expiredToken, out RefreshToken notExpiredToken);
            repo.Setup(x => x.GetByEmail(user.Email!))
                .Returns(user);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, dt.Object);

            mng.RestorePassword(user.Email!, ip);

            Assert.Contains(notExpiredToken, user.RefreshTokens);
            Assert.DoesNotContain(expiredToken, user.RefreshTokens);
        }

        [Theory, AutoMoqData]
        public void RestorePassword_Revokes_ActiveTokens(UserModel user, string ip, string token, DateTime now, DateTime revoked, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordHasher> passHasher, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            SetupForRevokeActiveTokens(token, ip, now, revoked, user, repo, dt, out RefreshToken activeToken1, out RefreshToken activeToken2, out RefreshToken inactiveToken);
            repo.Setup(x => x.GetByEmail(user.Email!))
                .Returns(user);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, dt.Object);

            mng.RestorePassword(user.Email!, ip);

            Assert.Equal(now, activeToken1.Revoked);
            Assert.Equal(now, activeToken2.Revoked);
            Assert.Equal(revoked, inactiveToken.Revoked);
        }

        [Theory, AutoMoqData]
        public void RestorePassword_Generates_NewRefreshToken(UserModel user, string ip, string token, DateTime now, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordHasher> passHasher, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetByEmail(user.Email!))
                .Returns(user);
            passGenerator.Setup(x => x.GenerateRefreshToken(user.Id))
                .Returns(token);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, dt.Object);

            mng.RestorePassword(user.Email!, ip);

            RefreshToken? refreshToken = user.RefreshTokens.Find(x => x.Token == token);

            Assert.NotNull(refreshToken);
        }

        [Theory, AutoMoqData]
        public void RestorePassword_Calls_GeneratePassword_Once(UserModel user, string ip, DateTime now, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordHasher> passHasher, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, dt.Object);

            mng.RestorePassword(user.Email!, ip);

            passGenerator.Verify(x => x.GeneratePassword(), Times.Once());
        }

        [Theory, AutoMoqData]
        public void RestorePassword_Calls_HashPassword_Once(UserModel user, string ip, string newPassword, DateTime now, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordHasher> passHasher, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            passGenerator.Setup(x => x.GeneratePassword())
                .Returns(newPassword);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, dt.Object);

            mng.RestorePassword(user.Email!, ip);

            passHasher.Verify(x => x.HashPassword(newPassword), Times.Once());
        }

        [Theory, AutoMoqData]
        public void RestorePassword_Calls_UpdatePassword_Once(UserModel user, string ip, string newPassword, string hash, DateTime now, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IPasswordHasher> passHasher, Mock<IPasswordGenerator> passGenerator, Mock<IDateTimeProvider> dt, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetByEmail(user.Email!))
                .Returns(user);
            passGenerator.Setup(x => x.GeneratePassword())
                .Returns(newPassword);
            passHasher.Setup(x => x.HashPassword(newPassword))
                .Returns(hash);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);

            UsersManager mng = new(repo.Object, resourceRepo.Object, passGenerator.Object, passHasher.Object, opts.Object, dt.Object);

            mng.RestorePassword(user.Email!, ip);

            repo.Verify(x => x.UpdatePassword(user.Id, hash, It.IsAny<IEnumerable<RefreshToken>>()), Times.Once());
        }

        [Theory, AutoMoqData]
        public void AttachPhoto_GetById_Once(string resourceId, string userId, UserModel user, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetById(userId))
                .Returns(user);

            UsersManager mng = new(repo.Object, resourceRepo.Object, null!, null!, opts.Object, null!);

            mng.AttachPhoto(userId, resourceId);

            repo.Verify(x => x.GetById(userId), Times.Once);
            resourceRepo.Verify(x => x.GetById(resourceId), Times.Once);
        }

        [Theory, AutoMoqData]
        public void AttachPhoto_UpdatePhoto_Once(string resourceId, string userId, UserModel user, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            var resource = new ResourceModel { Id = resourceId };
            repo.Setup(x => x.GetById(userId))
                   .Returns(user);
            resourceRepo.Setup(x => x.GetById(resourceId))
                   .Returns(resource);

            UsersManager mng = new(repo.Object, resourceRepo.Object, null!, null!, opts.Object, null!);

            mng.AttachPhoto(userId, resourceId);

            repo.Verify(x => x.UpdatePhoto(user.Id, resourceId), Times.Once);
        }

        [Theory, AutoMoqData]
        public void DeletePhoto_GetById_Once(string userId, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            UsersManager mng = new(repo.Object, resourceRepo.Object, null!, null!, opts.Object, null!);

            mng.DeletePhoto(userId);

            repo.Verify(x => x.GetById(userId), Times.Once);
        }

        [Theory, AutoMoqData]
        public void DeletePhoto_UpdatePhoto_Once(string userId, UserModel user, Mock<IUsersRepository> repo, Mock<IResourceRepository> resourceRepo, Mock<IOptions<RefreshTokenOptions>> opts)
        {
            repo.Setup(x => x.GetById(userId))
                   .Returns(user);

            UsersManager mng = new(repo.Object, resourceRepo.Object, null!, null!, opts.Object, null!);

            mng.DeletePhoto(userId);

            repo.Verify(x => x.UpdatePhoto(user.Id, null), Times.Once);
        }


        private void SetupForRemovesOldTokens(DateTime now, string token, UserModel user, Mock<IUsersRepository> repo, Mock<IOptions<RefreshTokenOptions>> opts, Mock<IDateTimeProvider> dt, out RefreshToken expiredToken, out RefreshToken notExpiredToken)
        {
            DateTime notExpired = now.AddDays(1);
            DateTime expired = now.AddDays(-14);

            notExpiredToken = new()
            {
                Created = now,
                Expires = notExpired,
                Token = token
            };
            expiredToken = new()
            {
                Created = expired,
                Expires = expired,
                Revoked = expired
            };

            user.RefreshTokens = new List<RefreshToken> { expiredToken, notExpiredToken };

            repo.Setup(x => x.GetById(user.Id))
                .Returns(user);
            opts.SetupGet(x => x.Value)
                .Returns(new RefreshTokenOptions { DeleteTokensOlderThenDays = 14 });
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);
            dt.SetupGet(x => x.FixedUtcNow)
                .Returns(now);
            SystemTime.SetDateTimeProvider(dt.Object);
        }

        private void SetupForRevokeActiveTokens(string token, string ip, DateTime now, DateTime revoked, UserModel user, Mock<IUsersRepository> repo, Mock<IDateTimeProvider> dt, out RefreshToken activeToken1, out RefreshToken activeToken2, out RefreshToken inactiveToken)
        {
            activeToken1 = new RefreshToken
            {
                Token = token,
                Created = now,
                CreatedFromIp = ip,
                Expires = now.AddDays(1),
            };
            activeToken2 = new RefreshToken
            {
                Token = token,
                Created = now,
                CreatedFromIp = ip,
                Expires = now.AddDays(1),
            };
            inactiveToken = new RefreshToken
            {
                Token = token,
                Revoked = revoked
            };

            user.RefreshTokens = new List<RefreshToken> { activeToken1, activeToken2, inactiveToken };

            repo.Setup(x => x.GetById(user.Id))
                .Returns(user);
            dt.SetupGet(x => x.UtcNow)
                .Returns(now);
            SystemTime.SetDateTimeProvider(dt.Object);
        }
    }
}
