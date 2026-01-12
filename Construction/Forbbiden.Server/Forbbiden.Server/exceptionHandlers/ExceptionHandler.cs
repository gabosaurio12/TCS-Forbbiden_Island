using Forbbiden.Contracts;
using log4net;
using System;
using System.Data.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Net.Mail;
using System.ServiceModel;

namespace Forbbiden.Server.exceptionHandlers
{
    public static class ExceptionHandler
    {
        public static readonly string SqlMessage = "Db Source Error";
        public static readonly string PullingError = "Pulling info from datatabase Error";
        public static readonly string PushingError = "Pushing info to datatabase Error";
        public static readonly string EmailError = "Sending email error";

        private const string DBError = "DatabaseError";
        private const string EntityError = "EntityError";
        private const string CommunicationError = "CommunicationError";
        private const string TimeoutError = "TimeoutError";
        private const string UnexpectedError = "UnexpectedError";
        private const string SmtpError = "SMTPError";

        private static readonly ILog Log = LogManager.GetLogger(typeof(ExceptionHandler));

        public static void HandleEntityException(EntityException ex, string classMethod, string message)
        {
            Log.Error(classMethod, ex);
            ThrowFault(EntityError, message);
        }

        public static void HandleDBException(DbException ex, string classMethod, string message)
        {
            Log.Error(classMethod, ex);
            ThrowFault(DBError, message);
        }

        public static void HandleDbUpdateException(DbUpdateException ex, string classMethod, string message)
        {
            Log.Error(classMethod, ex);
            ThrowFault(EntityError, message);
        }

        public static void HandleEntityValidationException(DbEntityValidationException ex,
            string classMethod, string message)
        {
            Log.Error(classMethod, ex);
            ThrowFault(EntityError, message);
        }

        public static void HandleCommunicationException(CommunicationException ex,
            string classMethod, string message)
        {
            Log.Error(classMethod, ex);
            ThrowFault(CommunicationError, message);
        }

        public static void HandleTimeoutException(TimeoutException ex, string classMethod, string message)
        {
            Log.Error(classMethod, ex);
            ThrowFault(TimeoutError, message);
        }

        public static void HandleException(Exception ex, string classMethod, string message)
        {
            Log.Error(classMethod, ex);
            ThrowFault(UnexpectedError, message);
        }

        public static void HandleSmtpException(SmtpException ex, string classMethod, string message)
        {
            Log.Error(classMethod, ex);
            ThrowFault(SmtpError, message);
        }

        private static void ThrowFault(string error, string details)
        {
            var fault = new Fault
            {
                Error = error,
                Details = details
            };

            throw new FaultException<Fault>(fault, 
                new FaultReason(fault.Details));
        }
    }
}
