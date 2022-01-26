using BacklogApp.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BacklogApp.Repository
{
    public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        protected IMongoDatabase Db { get; }
        protected IMongoCollection<TEntity> Collection { get; }

        public BaseRepository(string collectionName, IMongoDatabase db)
        {
            Db = db;
            Collection = db.GetCollection<TEntity>(collectionName);
        }

        public void Create(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            Collection.InsertOne(entity);
        }

        public void Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            var filter = GetIdFilter(id);
            Collection.DeleteOne(filter);
        }

        public TEntity? GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            var filter = GetIdFilter(id);

            return TakeOne(Collection.FindSync(filter));
        }

        protected TEntity? TakeOne(IAsyncCursor<TEntity> cursor)
        {
            cursor.MoveNext();
            return cursor.Current.FirstOrDefault();
        }

        protected FilterDefinition<TEntity> GetIdFilter(string id) => Builders<TEntity>.Filter.Eq("_id", ObjectId.Parse(id));

        protected MongoDBRef BuildProjectRef(string projectId) => BuildRef(DbCollections.Projects, projectId);
        protected MongoDBRef BuildTaskRef(string projectId) => BuildRef(DbCollections.Tasks, projectId);
        protected MongoDBRef BuildUserRef(string projectId) => BuildRef(DbCollections.Users, projectId);

        private MongoDBRef BuildRef(string collection, string id) => new(collection, ObjectId.Parse(id));
    }
}
