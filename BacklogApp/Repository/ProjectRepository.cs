using BacklogApp.Models;
using BacklogApp.Models.Db;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BacklogApp.Repository
{
    public interface IProjectRepository : IBaseRepository<ProjectModel>
    {
        List<ProjectModel> GetUserOwnedProjects(string userId);
        List<ProjectModel> GetUserProjects(string userId);
        ProjectModel Create(string name, string userId);
        void UpdateName(string projectId, string newName);
        void UpdateUsers(string projectId, ICollection<string> userIds);
        bool IsUserInProject(string projectId, string userId);
        bool IsUserProjectOwner(string projectId, string userId);
    }

    public class ProjectRepository : BaseRepository<ProjectModel>, IProjectRepository
    {
        public ProjectRepository(IMongoDatabase db) : base(DbCollections.Projects, db) { }

        public List<ProjectModel> GetUserOwnedProjects(string userId)
        {
            var dbRef = BuildUserRef(userId);
            var filter = Builders<ProjectModel>.Filter.Eq(nameof(ProjectModel.Owner), dbRef);

            return Collection.FindSync(filter).ToList();
        }

        public List<ProjectModel> GetUserProjects(string userId)
        {
            var dbRef = BuildUserRef(userId);
            var filter = Builders<ProjectModel>.Filter.Eq(nameof(ProjectModel.Users), dbRef);

            return Collection.FindSync(filter).ToList();
        }

        public ProjectModel Create(string name, string userId)
        {
            var dbRef = BuildUserRef(userId);
            var item = new ProjectModel
            {
                Name = name,
                Owner = dbRef,
                Users = { dbRef }
            };

            Create(item);

            return item;
        }

        public void UpdateName(string projectId, string newName)
        {
            var filter = Builders<ProjectModel>.Filter.Eq("_id", ObjectId.Parse(projectId));
            var update = Builders<ProjectModel>.Update.Set(nameof(ProjectModel.Name), newName);

            Collection.UpdateOne(filter, update);
        }

        public void UpdateUsers(string projectId, ICollection<string> userIds)
        {
            var filter = Builders<ProjectModel>.Filter.Eq("_id", ObjectId.Parse(projectId));
            ProjectModel? item = Collection.FindSync(filter).FirstOrDefault();
            if (item == null) return;

            List<MongoDBRef> users = userIds.Select(x => BuildUserRef(x)).ToList();

            var update = Builders<ProjectModel>.Update.Set(nameof(ProjectModel.Users), users);

            Collection.UpdateOne(filter, update);
        }

        public bool IsUserInProject(string projectId, string userId)
        {
            MongoDBRef userRef = BuildUserRef(userId);
            var filter = Builders<ProjectModel>.Filter.And
            (
                Builders<ProjectModel>.Filter.Eq("_id", ObjectId.Parse(projectId)),
                Builders<ProjectModel>.Filter.Eq(nameof(ProjectModel.Users), userRef)
            );

            return Collection.CountDocuments(filter) > 0;
        }

        public bool IsUserProjectOwner(string projectId, string userId)
        {
            MongoDBRef userRef = BuildUserRef(userId);
            var filter = Builders<ProjectModel>.Filter.And
            (
                Builders<ProjectModel>.Filter.Eq("_id", ObjectId.Parse(projectId)),
                Builders<ProjectModel>.Filter.Eq(nameof(ProjectModel.Owner), userRef)
            );

            return Collection.CountDocuments(filter) > 0;
        }
    }
}
