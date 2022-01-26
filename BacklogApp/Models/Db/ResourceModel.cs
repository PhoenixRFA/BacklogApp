using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BacklogApp.Models.Db
{
    public class ResourceModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = default!;
        public string Code { get; set; } = default!;
        public ObjectId FileId { get; set; }
        public ObjectId Owner { get; set; }
        public string MimeType { get; set; } = default!;
        public string OriginalName { get; set; } = default!;
    }
}
