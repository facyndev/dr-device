using Domain.Exceptions;

namespace Domain.Entities;

public class Specialization
{
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }

    protected Specialization() { }

    public Specialization(Guid userId, Guid categoryId)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("La especialización debe vincularse a un técnico.");
        if (categoryId == Guid.Empty)
            throw new ValidationException("La especialización debe vincularse a una categoría.");

        UserId = userId;
        CategoryId = categoryId;
    }
}
