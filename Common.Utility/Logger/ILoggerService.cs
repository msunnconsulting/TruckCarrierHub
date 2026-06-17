
namespace Common.Utility.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal abstract class ILoggerService
    {
        protected LogInfo GetLogInfo(string message, LogType logType, string moreInfo = null)
        {
            if (message == null) { throw new ArgumentNullException("message"); }
            LogInfo logInfo = new LogInfo();
            logInfo.LogDate = DateTime.UtcNow;
            logInfo.Message = message;
            logInfo.Description = moreInfo;
            logInfo.LogType = logType;
            return logInfo;
        }
        protected LogInfo GetLogInfo(Exception ex, LogType logType, string moreInfo = null)
        {
            LogInfo logInfo = new LogInfo();
            logInfo.LogDate = DateTime.UtcNow;
            logInfo.Message = ex.Message;
            if (logInfo.Message == null)
                logInfo.Message = "Unspecified error message";

            StringBuilder sbLog = new StringBuilder();
            if (moreInfo != null)
                sbLog.AppendLine(moreInfo);
            int errorMessageCount = 1;
            do
            {
                if (errorMessageCount > 1 && ex.Message != null)
                {
                    sbLog.Append(Environment.NewLine);
                    sbLog.AppendLine("Message(" + errorMessageCount + ") : " + ex.Message);
                }
                if (ex.StackTrace != null)
                {
                    if (errorMessageCount > 1)
                        sbLog.AppendLine("StackTrace(" + errorMessageCount + ")");
                    sbLog.AppendLine(ex.StackTrace);
                }
                errorMessageCount++;
                ex = ex.InnerException;
            }
            while (ex != null);
            logInfo.Description = sbLog.ToString();
            logInfo.LogType = logType;
            return logInfo;
        }

        //void Init();
        public abstract void Log(LogInfo logInfo);
        public abstract void Log(string message, LogType logType, string moreInfo = null);
        public abstract void Log(Exception ex, LogType logType, string moreInfo = null);
        public abstract string GetTextLog();
        public abstract List<LogInfo> GetDbLog();
        public abstract void ClearLog();
    }
}
