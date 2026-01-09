using Forbbiden.Client.ErrorCodes;
using Forbbiden.Client.Exceptions;
using Forbbiden.Client.logic;
using Forbbiden.Client.Model;
using System.Collections.Generic;
using System.Windows;

namespace Forbbiden.Client.Logic
{
    internal class ErrorsNotificationManager
    {

        public static void ShowUsernameValidationErrors(List<ValidationErrorCodes> errors, Window window)
        {
            foreach (var error in errors)
            {
                string title = Properties.Resources.invalid_input;
                string message;
                switch (error)
                {
                    case ValidationErrorCodes.UsernameEmpty:
                        message = Properties.Resources.invalid_empty_username;
                        ViewUtils.OpenNotificationWindow(title, message, window);
                        break;
                    case ValidationErrorCodes.UsernameContainsWhiteSpaces:
                        message = Properties.Resources.invalid_username_contains_whitespace;
                        ViewUtils.OpenNotificationWindow(title, message, window);
                        break;
                }
            }
        }

        public static void ShowEmailValidationErrors(List<ValidationErrorCodes> errors, Window window)
        {
            foreach (var error in errors)
            {
                string title = Properties.Resources.invalid_input;
                string message;
                switch (error)
                {
                    case ValidationErrorCodes.EmailEmpty:
                        message = Properties.Resources.invalid_empty_email;
                        ViewUtils.OpenNotificationWindow(title, message, window);
                        break;
                    case ValidationErrorCodes.EmailNotContaintsAt:
                        message = Properties.Resources.invalid_email_not_contains_at;
                        ViewUtils.OpenNotificationWindow(title, message, window);
                        break;
                    case ValidationErrorCodes.EmailContainsMoreThanOneAt:
                        message = Properties.Resources.invalid_email_contains_multiple_at;
                        ViewUtils.OpenNotificationWindow(title, message, window);
                        break;
                    case ValidationErrorCodes.EmailNotContainsExtension:
                        message = Properties.Resources.invalid_email_not_contains_extension;
                        ViewUtils.OpenNotificationWindow(title, message, window);
                        break;
                }
            }
        }

        public static void ShowPasswordValidationErrors(List<ValidationErrorCodes> errors, Window window)
        {
            foreach (var error in errors)
            {
                string title = Properties.Resources.invalid_input;
                string message;
                switch (error)
                {
                    case ValidationErrorCodes.PasswordEmpty:
                        message = Properties.Resources.invalid_password_empty;
                        ViewUtils.OpenNotificationWindow(title, message, window);
                        break;
                    case ValidationErrorCodes.PasswordTooShort:
                        message = Properties.Resources.invalid_password_too_short;
                        ViewUtils.OpenNotificationWindow(title, message, window);
                        break;
                    case ValidationErrorCodes.PassowrdMissingUpperCase:
                        message = Properties.Resources.invalid_password_missing_uppercase;
                        ViewUtils.OpenNotificationWindow(title, message, window);
                        break;
                    case ValidationErrorCodes.PasswordMissingLowerCase:
                        message = Properties.Resources.invalid_password_missing_lowercase;
                        ViewUtils.OpenNotificationWindow(title, message, window);
                        break;
                    case ValidationErrorCodes.PasswordMissingNumbers:
                        message = Properties.Resources.invalid_password_missing_number;
                        ViewUtils.OpenNotificationWindow(title, message, window);
                        break;
                    case ValidationErrorCodes.PasswordMissingSpecialCharacters:
                        message = Properties.Resources.invalid_password_missing_special_character;
                        ViewUtils.OpenNotificationWindow(title, message, window);
                        break;
                }
            }
        }

        public static void ShowPullError(Window window)
        {
            string title = Properties.Resources.error;
            string message = Properties.Resources.pull_database_error;
            ViewUtils.OpenNotificationWindow(title, message, window);
        }

        public static void ShowPushError(Window window)
        {
            string title = Properties.Resources.error;
            string message = Properties.Resources.push_database_error;
            ViewUtils.OpenNotificationWindow(title, message, window);
        }

        public static void HandlePageLoadError(Window window)
        {
            string title = Properties.Resources.error;
            string message = Properties.Resources.load_page_error;
            ViewUtils.OpenNotificationWindow(title, message, window);
        }

        public static void ShowViewExceptionNotification(ViewException ex, Window window)
        {
            string title = Properties.Resources.error;
            string message;
            switch (ex.ErrorCode)
            {
                case ServerErrorCodes.pullingDataError:
                    ShowPullError(window);
                    break;
                case ServerErrorCodes.pushingDataError:
                    ShowPushError(window);
                    break;
                case ServerErrorCodes.updatingDataError:
                    message = Properties.Resources.error_update_database;
                    ViewUtils.OpenNotificationWindow(title, message, window);
                    break;
                case ServerErrorCodes.timeoutError:
                    message = Properties.Resources.error_timeout;
                    ViewUtils.OpenNotificationWindow(title, message, window);
                    break;
                case ServerErrorCodes.sendEmailError:
                    message = Properties.Resources.send_email_error;
                    ViewUtils.OpenNotificationWindow(title, message, window);
                    break;
                case ServerErrorCodes.avatarDownloadError:
                    message = Properties.Resources.error_avatar_download;
                    ViewUtils.OpenNotificationWindow(title, message, window);
                    break;

            }
        }
    }
}
