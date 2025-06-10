using System.ComponentModel.DataAnnotations;

namespace UserAuth.Repository.CustomValidation;

public class MinimumAgeAttribute : ValidationAttribute
{
    private readonly int _minimumAge;

    public MinimumAgeAttribute(int minimumAge)
    {
        _minimumAge = minimumAge;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime dateOfBirth)
        {
            var age = DateTime.Now.Year - dateOfBirth.Year;
            if (dateOfBirth > DateTime.Now.AddYears(-age)) age--;

            if (age < _minimumAge)
            {
                return new ValidationResult($"You must be at least {_minimumAge} years old.");
            }
        }

        return ValidationResult.Success;
    }
}
