
namespace Common.Utility.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Text;


    internal sealed class LoggerServiceText : ILoggerService
    {
        private readonly TextLogger Config;
        private Object FileAccessLock = new Object();

        public LoggerServiceText()
        {
            var config = ((LoggerSection)ConfigurationManager.GetSection("logger"));
            if (config == null) { throw new Exception("Missing logger configuration section."); }

            this.Config = config.TextLogger;
            if (string.IsNullOrEmpty(this.Config.LogFilePath)) { throw new Exception("logger section is missing LogFilePath setting."); }
        }

        public override void Log(LogInfo logInfo)
        {
            StringBuilder sbLog = new StringBuilder();

            sbLog.Append("[" + logInfo.LogDate.ToString("MM-dd-yyyy HH:mm:ss.fff") + "] ");
            sbLog.Append(": " + logInfo.LogType.ToString());
            sbLog.Append(" : " + logInfo.Message);
            sbLog.Append(Environment.NewLine);

            if (logInfo.Description != null)
            {
                sbLog.Append("Description".PadRight(19));
                //sbLog.Append(Environment.NewLine);
                sbLog.Append(": " + logInfo.Description);
                sbLog.Append(Environment.NewLine);
            }
            //sbLog.Append("*".PadRight(100, '*') + Environment.NewLine);

            lock (FileAccessLock)
            {
                using (StreamWriter sw = File.AppendText(this.Config.LogFilePath))
                {
                    sw.Write(sbLog.ToString());
                    sw.Close();
                }
            }
        }

        public override void Log(string message, LogType logType, string moreInfo)
        {
            this.Log(this.GetLogInfo(message, logType, moreInfo));
        }

        public override void Log(Exception ex, LogType logType, string moreInfo)
        {
            this.Log(this.GetLogInfo(ex, logType, moreInfo));
        }

        public override string GetTextLog()
        {
            lock (FileAccessLock)
            {
                if (File.Exists(this.Config.LogFilePath))
                    return File.ReadAllText(this.Config.LogFilePath);
                else
                    return "";
            }
        }

        public override List<LogInfo> GetDbLog()
        {
            throw new Exception("This GetLog method can only be used if logtype is set to 'db'");
        }

        public override void ClearLog()
        {
            lock (FileAccessLock)
            {
                File.Delete(this.Config.LogFilePath);
            }
        }
    }
}
