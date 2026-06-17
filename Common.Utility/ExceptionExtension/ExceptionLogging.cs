using Common.Utility.Logger;
using System;


namespace Common.Utility.ExceptionExtension
{
    public static class ExceptionLogging
    {
        private static readonly IExceptionService _exceptionService;

        static ExceptionLogging()
        {
            ExceptionService exceptionService = new ExceptionService();
            _exceptionService = exceptionService;

        }
        public static void ExceptionSendToMail(Exception ex)
        {
            try
            {
                _exceptionService.ExceptionSendToMail(ex);
            }
            catch (Exception exception)
            {
                AppLogger.Instance.Log(exception);
            }
        }
    }
}