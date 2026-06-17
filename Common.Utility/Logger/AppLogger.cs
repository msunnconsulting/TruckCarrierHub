
namespace Common.Utility.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;


    public sealed class AppLogger
    {
        private static ILoggerService loggerService;
        private static object syncRoot = new Object();
        private static volatile AppLogger instance;
        public static AppLogger Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            if (string.IsNullOrEmpty(Config.LogType))
                                throw new Exception("missing configuration logType for logger");
                            if (Config.LogType.ToLower() == "db")
                                loggerService = new LoggerServiceDB();
                            else if (Config.LogType.ToLower() == "text")
                                loggerService = new LoggerServiceText();
                            else
                                throw new Exception("logger section has some invalid value for logger type. It should be 'db' or 'text'");
                            instance = new AppLogger();
                        }
                    }
                }
                return instance;
            }
        }

        private static LoggerSection _Config = null;
        private static LoggerSection Config
        {
            get
            {
                if (_Config == null)
                {
                    _Config = ((LoggerSection)ConfigurationManager.GetSection("logger"));
                    if (_Config == null)
                        throw new Exception("missing configuration section for logger");

                }
                return _Config;
            }
        }

        public void Log(string message, LogType logType = LogType.Info, string additionalMessage = null, bool IsLogEnabled = true)
        {
            if (!Config.Enabled)
            {
                return;
            }
            else if (IsLogEnabled == false)
            {
                return;
            }
            else
            {
                loggerService.Log(message, logType, additionalMessage);
            }
        }

        public void Log(Exception ex, LogType logType = LogType.Error, string additionalMessage = null, bool IsLogEnabled = true)
        {
            if (!Config.Enabled)
            {
                return;
            }
            else if (IsLogEnabled == false)
            {
                return;
            }
            else
            {
                loggerService.Log(ex, logType, additionalMessage);
            }
            //if (!Config.Enabled && IsLogEnabled==false) return;
            //loggerService.Log(ex, logType, additionalMessage);
        }

        public void Log(LogInfo logInfo)
        {
            if (logInfo == null) return;
            if (!Config.Enabled) return;
            loggerService.Log(logInfo);
        }

        public string GetTextLog() { return loggerService.GetTextLog(); }

        public List<LogInfo> GetDbLog() { return loggerService.GetDbLog(); }

        public void ClearLog() { loggerService.ClearLog(); }

        public static string LogFilePath
        {
            get
            {
                return Config.TextLogger.LogFilePath;
            }
            set
            {
                if (instance != null) throw new Exception("Please configure LogFilePath before logger is used");
                if (Config.LogType.ToLower() != "text")
                    throw new Exception("LogFilePath can be set dynamically only for LogType = 'text'");
                else
                    Config.TextLogger.LogFilePath = value;
            }
        }

        public static string TableName
        {
            get { return Config.DbLogger.TableName; }
            set
            {
                if (instance != null) throw new Exception("Please configure TableName before logger is used");
                if (Config.LogType.ToLower() != "db")
                    throw new Exception("TableName can be set dynamically only for LogType = 'db'");
                else
                    Config.TextLogger.LogFilePath = value;
            }
        }

        public static string ConnectionString
        {
            get { return Config.DbLogger.ConnectionString; }
            set
            {
                if (instance != null) throw new Exception("Please configure ConnectionString before logger is used");
                if (Config.LogType.ToLower() != "db")
                    throw new Exception("ConnectionString can be set dynamically only for LogType = 'db'");
                else
                    Config.DbLogger.ConnectionString = value;
            }
        }

        public static bool Enabled
        {
            get
            {
                return Config.Enabled;
            }
            set
            {
                Config.Enabled = value;
            }
        }

    }
}
