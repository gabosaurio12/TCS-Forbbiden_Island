namespace Forbbiden.Client.Model
{
    public enum ValidationErrorCodes
    {
        UsernameEmpty,
        UsernameContainsWhiteSpaces,
        UsernameIsNotAvailable,
        PasswordEmpty,
        PasswordTooShort,
        PassowrdMissingUpperCase,
        PasswordMissingLowerCase,
        PasswordMissingNumbers,
        PasswordMissingSpecialCharacters,
        EmailEmpty,
        EmailNotContaintsAt,
        EmailContainsMoreThanOneAt,
        EmailNotContainsExtension
    }
}
