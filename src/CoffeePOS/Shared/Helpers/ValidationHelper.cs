using System.ComponentModel.DataAnnotations;

namespace CoffeePOS.Shared.Helpers;

public static class ValidationHelper
{
    public static bool TryValidate<T>(T obj, out string errorMessage) where T : class
    {
        var context = new ValidationContext(obj);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(obj, context, results, true))
        {
            errorMessage = results.First().ErrorMessage ?? "Dữ liệu không hợp lệ!";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
