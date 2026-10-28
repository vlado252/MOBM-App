using System.Text.RegularExpressions;

namespace MOBM_App.Helpers
{
    public static class Validators
    {
        private static readonly Regex EmailRegex =
            new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

        private static readonly Regex PhoneRegex =
            new Regex(@"^(\+375|80)?(29|33|44|25)\d{7}$");

        private static readonly Regex LoginRegex =
            new Regex(@"^[a-zA-Z0-9_]{3,20}$");

        // ✅ ИСПРАВЛЕНО: 3 символа минимум (вместо 6)
        private static readonly Regex PasswordRegex =
            new Regex(@"^.{3,}$");

        private static readonly Regex PriceRegex =
            new Regex(@"^\d+(\.\d{1,2})?$");

        private static readonly Regex QuantityRegex =
            new Regex(@"^\d+$");

        public static bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && EmailRegex.IsMatch(email);
        }

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return true;
            return PhoneRegex.IsMatch(phone.Replace(" ", "").Replace("-", ""));
        }

        public static bool IsValidLogin(string login)
        {
            return !string.IsNullOrEmpty(login) && LoginRegex.IsMatch(login);
        }

        public static bool IsValidPassword(string password)
        {
            return !string.IsNullOrEmpty(password) && PasswordRegex.IsMatch(password);
        }

        public static bool IsValidPrice(string price)
        {
            return !string.IsNullOrEmpty(price) && PriceRegex.IsMatch(price);
        }

        public static bool IsValidQuantity(string quantity)
        {
            return !string.IsNullOrEmpty(quantity) && QuantityRegex.IsMatch(quantity);
        }

        public static string GetValidationError(string fieldName, string value)
        {
            switch (fieldName.ToLower())
            {
                case "email":
                    return IsValidEmail(value) ? "" : "Некорректный email (пример: user@mail.com)";
                case "phone":
                    return IsValidPhone(value) ? "" : "Некорректный телефон (+375XXXXXXXXX)";
                case "login":
                    return IsValidLogin(value) ? "" : "Логин: 3-20 символов, латиница и цифры";
                case "password":
                    return IsValidPassword(value) ? "" : "Пароль минимум 3 символа";
                case "price":
                    return IsValidPrice(value) ? "" : "Цена должна быть числом";
                case "quantity":
                    return IsValidQuantity(value) ? "" : "Количество должно быть целым числом";
                default:
                    return "";
            }
        }
    }
}