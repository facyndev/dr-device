namespace Application.Interfaces;

public interface ITransactionManager
{
    Task ExecuteAsync(Func<Task> operation);
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation);
}
