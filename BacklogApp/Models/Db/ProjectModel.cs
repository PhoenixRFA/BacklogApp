using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace BacklogApp.Models.Db
{
    public class ProjectModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public MongoDBRef Owner { get; set; } = default!;
        public List<MongoDBRef> Users { get; set; } = new List<MongoDBRef>();
    }
}
