namespace BacklogApp.Repository
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        TEntity? GetById(string id);
        void Create(TEntity entity);
        void Delete(string id);
    }
}
