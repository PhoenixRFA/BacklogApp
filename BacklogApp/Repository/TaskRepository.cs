using BacklogApp.Models;
using BacklogApp.Models.Db;
using BacklogApp.Models.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BacklogApp.Repository
{
    public interface ITaskRepository : IBaseRepository<TaskModel>
    {
        List<TaskModel> GetByProject(string projectId);
        TaskModel Create(string projectId, string userId, TaskModel model);
        void Update(TaskModel task);
        void UpdateStatus(string id, string status);
        int CountTasksInProject(string projectId);
        List<TaskCountInProjectModel> CountTasksInProjects(IEnumerable<string> projectIds);
    }

    public class TaskRepository : BaseRepository<TaskModel>, ITaskRepository
    {
        public TaskRepository(IMongoDatabase db) : base(DbCollections.Tasks, db) { }

        private static BsonDocument CountAggregate => new BsonDocument {
            { "_id", "$project" },
            //{ "count", new BsonDocument("$count", new BsonDocument()) }
            { "count", new BsonDocument("$sum", 1) }
        };

        public List<TaskModel> GetByProject(string projectId)
        {
            var dbRef = BuildProjectRef(projectId);
            var filter = Builders<TaskModel>.Filter.Eq(nameof(TaskModel.Project), dbRef);

            return Collection.FindSync(filter).ToList();
        }

        public int CountTasksInProject(string projectId)
        {
            var dbRef = BuildProjectRef(projectId);
            var filter = Builders<TaskModel>.Filter.Eq(nameof(TaskModel.Project), dbRef);

            TaskCountInProjectModel? res = Collection.Aggregate()
                .Match(filter)
                .Group<TaskCountInProjectModel>(CountAggregate)
                .FirstOrDefault();

            return res?.Count ?? 0;
        }
        
        public List<TaskCountInProjectModel> CountTasksInProjects(IEnumerable<string> projectIds)
        {
            MongoDBRef[] refs = projectIds.Select(projectId => BuildProjectRef(projectId)).ToArray();
            var filter = Builders<TaskModel>.Filter.In(nameof(TaskModel.Project), refs);

            var res = Collection.Aggregate()
                .Match(filter)
                .Group<TaskCountInProjectModel>(CountAggregate)
                .ToList();

            return res;
        }

        public TaskModel Create(string projectId, string userId, TaskModel model)
        {
            var projectRef = BuildProjectRef(projectId);
            var userRef = BuildUserRef(userId);

            model.Project = projectRef;
            model.CreatedBy = userRef;

            Collection.InsertOne(model);

            return model;
        }

        public void Update(TaskModel task)
        {
            var filter = Builders<TaskModel>.Filter.Eq("_id", ObjectId.Parse(task.Id));

            Collection.ReplaceOne(filter, task);
        }

        public void UpdateStatus(string id, string status)
        {
            var filter = Builders<TaskModel>.Filter.Eq("_id", ObjectId.Parse(id));
            var update = Builders<TaskModel>.Update.Set(nameof(TaskModel.Status), status);

            Collection.UpdateOne(filter, update);
        }
    }
}
