using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BacklogApp.Models.Db
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = default!;
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? EmailNormalized { get; set; }
        public string? PasswordHash { get; set; }
        public ObjectId? PhotoId { get; set; }

        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
