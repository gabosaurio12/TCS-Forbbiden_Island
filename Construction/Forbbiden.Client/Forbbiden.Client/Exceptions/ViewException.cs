using Forbbiden.Client.ErrorCodes;
using System;

namespace Forbbiden.Client.Exceptions
{
    public class ViewException : Exception
    {
        public ServerErrorCodes ErrorCode { get; set; }

        public ViewException(ServerErrorCodes errorCode)
        {
            ErrorCode = errorCode;
        }
    }
}
