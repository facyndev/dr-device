namespace Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAll();
    Task<T?> GetById(Guid id);
    Task<T> Create(T entity);
    Task Update(T entity);
    Task Delete(Guid id);
}
