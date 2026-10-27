using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Models.Users.Requests;

public record UpdateAccountRequest : IValidatableObject
{
    [StringLength(100, MinimumLength = 3)]
    public string? FirstName { get; init; }

    [StringLength(100, MinimumLength = 3)]
    public string? LastName { get; init; }

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; init; }

    [StringLength(20, MinimumLength = 7)]
    public string? Phone
    {
        get => _phone;
        init { _phone = value; PhoneProvided = true; }
    }
    private readonly string? _phone;

    [JsonIgnore]
    public bool PhoneProvided { get; private init; }

    [MaxLength(500)]
    public string? Address { get; init; }

    [MaxLength(100)]
    public string? Province { get; init; }

    [MaxLength(150)]
    public string? Locality { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var (name, value) in new[]
        {
            (nameof(FirstName), FirstName), (nameof(LastName), LastName), (nameof(Email), Email),
            (nameof(Phone), Phone), (nameof(Address), Address), (nameof(Province), Province), (nameof(Locality), Locality)
        })
            if (value is not null && string.IsNullOrWhiteSpace(value))
                yield return new ValidationResult($"{name} no puede estar vacío.", [name]);

        if ((Address is not null || Province is not null || Locality is not null)
            && (Address is null || Province is null || Locality is null))
            yield return new ValidationResult("Para cambiar la dirección enviá Address, Province y Locality juntos.",
                [nameof(Address), nameof(Province), nameof(Locality)]);
    }
}
