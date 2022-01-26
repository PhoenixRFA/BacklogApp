using BacklogApp.Models;
using BacklogApp.Models.Db;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BacklogApp.Repository
{
    public interface IUsersRepository : IBaseRepository<UserModel>
    {
        UserModel? GetByEmail(string email);
        UserModel? GetByRefreshToken(string token);
        List<UserModel> GetByIds(IEnumerable<string> userIds);
        void UpdateRefreshTokens(string id, IEnumerable<RefreshToken> tokens);
        void UpdateEmail(string id, string email, string emailNormalized);
        void UpdateName(string id, string name);
        void UpdatePassword(string id, string passwordHash, IEnumerable<RefreshToken> tokens);
        void UpdatePhoto(string id, string? resourceId);
    }

    public class UsersRepository : BaseRepository<UserModel>, IUsersRepository
    {
        public UsersRepository(IMongoDatabase db) : base(DbCollections.Users, db) { }

        public UserModel? GetByEmail(string email)
        {
            var filter = Builders<UserModel>.Filter.Eq(nameof(UserModel.EmailNormalized), email);

            return TakeOne(Collection.FindSync(filter));
        }

        public UserModel? GetByRefreshToken(string token)
        {
            var filter = Builders<UserModel>.Filter.ElemMatch(nameof(UserModel.RefreshTokens),
                    Builders<RefreshToken>.Filter.Eq(nameof(RefreshToken.Token), token)
                );

            return TakeOne(Collection.FindSync(filter));
        }

        public List<UserModel> GetByIds(IEnumerable<string> userIds)
        {
            var filter = Builders<UserModel>.Filter.In(nameof(UserModel.Id), userIds);

            return Collection.Find(filter).ToList();
        }

        public void UpdateRefreshTokens(string id, IEnumerable<RefreshToken> tokens)
        {
            var filter = GetIdFilter(id);
            var update = Builders<UserModel>.Update.Set(nameof(UserModel.RefreshTokens), tokens);

            Collection.UpdateOne(filter, update);
        }

        public void UpdateEmail(string id, string email, string emailNormalized)
        {
            var filter = GetIdFilter(id);
            var update = Builders<UserModel>.Update
                .Set(nameof(UserModel.Email), email)
                .Set(nameof(UserModel.EmailNormalized), emailNormalized);

            Collection.UpdateOne(filter, update);
        }

        public void UpdateName(string id, string name)
        {
            var filter = GetIdFilter(id);
            var update = Builders<UserModel>.Update
                .Set(nameof(UserModel.Name), name);

            Collection.UpdateOne(filter, update);
        }

        public void UpdatePassword(string id, string passwordHash, IEnumerable<RefreshToken> tokens)
        {
            var filter = GetIdFilter(id);
            var update = Builders<UserModel>.Update
                .Set(nameof(UserModel.PasswordHash), passwordHash)
                .Set(nameof(UserModel.RefreshTokens), tokens);

            Collection.UpdateOne(filter, update);
        }

        public void UpdatePhoto(string id, string? resourceId)
        {
            ObjectId? objId = string.IsNullOrEmpty(resourceId) ? null : ObjectId.Parse(resourceId);
            
            var filter = GetIdFilter(id);
            var update = Builders<UserModel>.Update
                .Set(nameof(UserModel.PhotoId), objId);

            Collection.UpdateOne(filter, update);
        }
    }
}
