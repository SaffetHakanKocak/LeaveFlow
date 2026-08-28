using System.Net.Mail;

namespace LeaveFlow.Application.People;

internal static class PeopleValidation
{
    internal static ValidationResult ValidateConsultant(ConsultantInput input)
    {
        var result = ValidatePerson(input.FirstName, input.LastName, input.Email, input.Department);

        if (input.EmployeeNumber?.Length > 64)
        {
            result.Add(nameof(input.EmployeeNumber), "Personel numarası en fazla 64 karakter olabilir.");
        }

        return result;
    }

    internal static ValidationResult ValidateManager(ManagerInput input)
    {
        return ValidatePerson(input.FirstName, input.LastName, input.Email, input.Department);
    }

    private static ValidationResult ValidatePerson(string firstName, string lastName, string email, string? department)
    {
        var result = new ValidationResult();

        ValidateRequired(result, "FirstName", firstName, 80);
        ValidateRequired(result, "LastName", lastName, 80);
        ValidateRequired(result, "Email", email, 256);

        if (!string.IsNullOrWhiteSpace(email))
        {
            try
            {
                _ = new MailAddress(email);
            }
            catch (FormatException)
            {
                result.Add("Email", "E-posta formatı geçersiz.");
            }
        }

        if (department?.Length > 120)
        {
            result.Add("Department", "Departman en fazla 120 karakter olabilir.");
        }

        return result;
    }

    private static void ValidateRequired(ValidationResult result, string field, string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result.Add(field, "Bu alan zorunludur.");
            return;
        }

        if (value.Length > maxLength)
        {
            result.Add(field, $"Bu alan en fazla {maxLength} karakter olabilir.");
        }
    }
}
