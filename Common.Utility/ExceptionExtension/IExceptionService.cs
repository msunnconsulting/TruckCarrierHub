using System;

namespace Common.Utility.ExceptionExtension
{
    public interface IExceptionService
    {
        void ExceptionSendToMail(Exception ex);
    }
}
