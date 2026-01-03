using Forbbiden.Contracts;
using log4net;
using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Net.Mail;
using System.ServiceModel;

namespace Forbbiden.Server.exceptionHandlers
{
    public class ExceptionHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ExceptionHandler));

        public static void HandleEntityException(EntityException ex, string classMethod)
        {
            Log.Error(classMethod, ex);
            string error = "Database Error";
            ThrowFault(error, ex.Message);
        }

        public static void HandleDbUpdateException(DbUpdateException ex, string classMethod)
        {
            Log.Error(classMethod, ex);
            string error = "Database Error";
            ThrowFault(error, ex.Message);
        }

        public static void HandleEntityValidationException(DbEntityValidationException ex, string classMethod)
        {
            Log.Error(classMethod, ex);
            string error = "Database Validation Error";
            ThrowFault(error, ex.Message);
        }

        public static void HandleCommunicationException(CommunicationException ex, string classMethod)
        {
            Log.Error(classMethod, ex);
            string error = "Communication Error";
            ThrowFault(error, ex.Message);
        }

        public static void HandleTimeoutException(TimeoutException ex, string classMethod)
        {
            Log.Error(classMethod, ex);
            string error = "Timeout Error";
            ThrowFault(error, ex.Message);
        }

        public static void HandleException(Exception ex, string classMethod)
        {
            Log.Error(classMethod, ex);
            string error = "Error";
            ThrowFault(error, ex.Message);
        }

        public static void HandleSmtpException(SmtpException ex, string classMethod)
        {
            Log.Error(classMethod, ex);
            string error = "SMTP Error";
            ThrowFault(error, ex.Message);
        }

        private static void ThrowFault(string error, string details)
        {
            var fault = new Fault
            {
                Error = error,
                Details = details
            };

            string entityError = "Exception";

            throw new FaultException<Fault>(fault,
                new FaultReason(entityError));
        }
    }
}
