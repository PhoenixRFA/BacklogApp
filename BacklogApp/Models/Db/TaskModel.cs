using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace BacklogApp.Models.Db
{
    public class TaskModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = default!;
        public string? Assessment { get; set; }
        public int Priority { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Description { get; set; }
        public string Name { get; set; } = default!;
        public string? Status { get; set; }
        public MongoDBRef CreatedBy { get; set; } = default!;
        public MongoDBRef Project { get; set; } = default!;
    }
}
