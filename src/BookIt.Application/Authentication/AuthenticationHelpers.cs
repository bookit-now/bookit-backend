using System.Net.Mail;

namespace BookIt.Application.Authentication;

internal static class AuthenticationHelpers
{
    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    public static void ValidateRegistration(string name, string email, string password)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors["name"] = ["Name is required."];
        }
        else if (name.Trim().Length > 200)
        {
            errors["name"] = ["Name must be 200 characters or fewer."];
        }

        if (!IsValidEmail(email))
        {
            errors["email"] = ["A valid email address is required."];
        }

        if (string.IsNullOrEmpty(password) || password.Length < 8)
        {
            errors["password"] = ["Password must contain at least 8 characters."];
        }

        if (errors.Count > 0)
        {
            throw new AuthenticationValidationException(errors);
        }
    }

    public static void ValidateLogin(string email, string password)
    {
        var errors = new Dictionary<string, string[]>();

        if (!IsValidEmail(email))
        {
            errors["email"] = ["A valid email address is required."];
        }

        if (string.IsNullOrEmpty(password))
        {
            errors["password"] = ["Password is required."];
        }

        if (errors.Count > 0)
        {
            throw new AuthenticationValidationException(errors);
        }
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 320)
        {
            return false;
        }

        try
        {
            return new MailAddress(email.Trim()).Address.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
