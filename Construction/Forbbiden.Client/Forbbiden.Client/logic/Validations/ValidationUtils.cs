using Forbbiden.Client.Model;
using System;
using System.Text.RegularExpressions;

namespace Forbbiden.Client.Logic.Validations
{
    internal class ValidationUtils
    {
        private static void ValidateRegexPassword(string password, ValidationResults validationResult)
        {
            var passwordUpperCase = Regex.IsMatch(password, @"[A-Z]",
                    RegexOptions.None, TimeSpan.FromMilliseconds(100));
            if (!passwordUpperCase)
                validationResult.Errors.Add(ValidationErrorCodes.PassowrdMissingUpperCase);

            var passwordLowerCase = Regex.IsMatch(password, @"[a-z]",
                RegexOptions.None, TimeSpan.FromMilliseconds(100));
            if (!passwordLowerCase)
                validationResult.Errors.Add(ValidationErrorCodes.PasswordMissingLowerCase);

            var passwordNumbers = Regex.IsMatch(password, @"[0-9]",
                RegexOptions.None, TimeSpan.FromMilliseconds(100));
            if (!passwordNumbers)
                validationResult.Errors.Add(ValidationErrorCodes.PasswordMissingNumbers);

            var passwordSpecialChar = Regex.IsMatch(password, @"[\W_]",
                RegexOptions.None, TimeSpan.FromMilliseconds(100));
            if (!passwordSpecialChar)
                validationResult.Errors.Add(ValidationErrorCodes.PasswordMissingSpecialCharacters);
        }

        public static ValidationResults ValidatePassword(string password)
        {
            const int PasswordMinLength = 8;
            var validationResult = new ValidationResults();
            if (string.IsNullOrWhiteSpace(password))
            {
                validationResult.Errors.Add(ValidationErrorCodes.PasswordEmpty);
            }
            if (password.Length < PasswordMinLength)
            {
                validationResult.Errors.Add(ValidationErrorCodes.PasswordTooShort);
            }
            ValidateRegexPassword(password, validationResult);
            
            return validationResult;
        }

        public static ValidationResults ValidateUsername(string username)
        {
            var validationResult = new ValidationResults();

            if (string.IsNullOrWhiteSpace(username))
            {
                validationResult.Errors.Add(ValidationErrorCodes.UsernameEmpty);
            }
            if (username.Contains(" "))
            {
                validationResult.Errors.Add(ValidationErrorCodes.UsernameContainsWhiteSpaces);
            }

            return validationResult;
        }

        public static ValidationResults ValidateEmail(string email)
        {
            var validationResult = new ValidationResults();
            if (string.IsNullOrWhiteSpace(email))
            {
                validationResult.Errors.Add(ValidationErrorCodes.EmailEmpty);
            }
            string[] emailParts = email.Split('@');
            if (emailParts.Length == 0)
            {
                validationResult.Errors.Add(ValidationErrorCodes.EmailNotContaintsAt);
            }
            if (emailParts.Length > 2)
            {
                validationResult.Errors.Add(ValidationErrorCodes.EmailContainsMoreThanOneAt);
            }
            if (emailParts[1].Contains("."))
            {
                validationResult.Errors.Add(ValidationErrorCodes.EmailNotContainsExtension);
            }
            return validationResult;
        }
    }
}
