namespace Application.Interfaces;

public interface IUserService
{
    Task DeleteAccountAsync(Guid userId);
}
