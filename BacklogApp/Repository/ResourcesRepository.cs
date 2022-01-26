using BacklogApp.Models;
using BacklogApp.Models.Db;
using MongoDB.Driver;

namespace BacklogApp.Repository
{
    public interface IResourceRepository : IBaseRepository<ResourceModel> { }

    public class ResourcesRepository : BaseRepository<ResourceModel>, IResourceRepository
    {
        public ResourcesRepository(IMongoDatabase db) : base(DbCollections.Resources, db) { }
    }
}
