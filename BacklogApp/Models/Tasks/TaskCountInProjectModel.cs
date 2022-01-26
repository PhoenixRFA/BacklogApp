using MongoDB.Driver;

namespace BacklogApp.Models.Tasks
{
    public class TaskCountInProjectModel
    {
        public MongoDBRef Id { get; set; } = default!;
        public int Count { get; set; }
    }
}
