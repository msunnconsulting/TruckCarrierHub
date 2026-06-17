
namespace Common.Utility.Logger
{
    using System;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.IO;
    using System.Web;


    internal class LoggerSection : ConfigurationSection
    {
        public override bool IsReadOnly()
        {
            return false;
        }

        // Create a "logType" attribute.
        [ConfigurationProperty("logType", IsRequired = true)]
        public string LogType
        {
            set
            {
                this["logType"] = value;
            }
            get
            {
                return (string)this["logType"];
            }
        }

        // Create a "debug" attribute.
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
        public bool Enabled
        {
            set { this["enabled"] = value; }

            get
            {
                return (bool)this["enabled"];
            }
        }

        // Create a "font" element.
        [ConfigurationProperty("textLogger")]
        public TextLogger TextLogger
        {
            get
            {
                return (TextLogger)this["textLogger"];
            }
        }

        // Create a "dbLogger element."
        [ConfigurationProperty("dbLogger")]
        public DbLogger DbLogger
        {
            get
            {
                return (DbLogger)this["dbLogger"];
            }
        }
    }

    // Define the "textLogger" element
    // with "filePath" attribute.
    internal class TextLogger : ConfigurationElement
    {
        public override bool IsReadOnly()
        {
            return false;
        }

        private string _LogFilePath;
        [ConfigurationProperty("logFilePath", IsRequired = false, DefaultValue = "")]
        public String LogFilePath
        {
            set
            {
                this["logFilePath"] = value;
                _LogFilePath = null;
            }
            get
            {
                if (_LogFilePath == null)
                {
                    _LogFilePath = (String)this["logFilePath"];

                    if (!string.IsNullOrEmpty(_LogFilePath))
                    {
                        if (_LogFilePath.Contains("|root|"))
                            _LogFilePath = _LogFilePath.Replace("|root|", Config.Root);

                        if (!File.Exists(_LogFilePath))
                        {
                            try
                            {
                                var dirPath = Path.GetDirectoryName(_LogFilePath);
                                if (!Directory.Exists(dirPath))
                                    Directory.CreateDirectory(dirPath);
                                using (StreamWriter sw = File.AppendText(_LogFilePath)) { sw.Close(); }
                            }
                            catch (Exception ex)
                            {
                                var errorMsg = "Value of logFilePath is not a valid file path : " + _LogFilePath;
                                throw new Exception(errorMsg, ex);
                            }
                        }
                    }
                }
                return _LogFilePath;
            }
        }
    }

    // Define the "dbLogger" element 
    // with "connectionString" attribute.
    internal class DbLogger : ConfigurationElement
    {
        public override bool IsReadOnly()
        {
            return false;
        }

        private string _connectionString;
        [ConfigurationProperty("connectionString", IsRequired = false, DefaultValue = "")]
        public String ConnectionString
        {
            set
            {
                _connectionString = null;
                this["connectionString"] = value;
            }
            get
            {
                if (_connectionString == null)
                {
                    _connectionString = (String)this["connectionString"];
                    var conStr = Convert.ToString(ConfigurationManager.ConnectionStrings[_connectionString]);
                    if (!string.IsNullOrEmpty(conStr))
                        _connectionString = conStr;
                    if (!string.IsNullOrEmpty(_connectionString))
                    {
                        try
                        {
                            SqlConnectionStringBuilder sbSQL = new SqlConnectionStringBuilder(_connectionString);
                        }
                        catch (Exception)
                        {
                            throw new Exception("ConnectiongString setting of logger seems to have some invalid value. ConnectionString : " + _connectionString);
                        }
                    }
                }
                return _connectionString;
            }
        }

        [ConfigurationProperty("tableName", IsRequired = false, DefaultValue = "")]
        public String TableName
        {
            set
            {
                this["tableName"] = value;
            }
            get
            {
                return (String)this["tableName"];
            }
        }
    }

    internal static class Config
    {
        private static string _Root;
        public static string Root
        {
            get
            {
                if (_Root != null) return _Root;

                if (HttpContext.Current != null && HttpContext.Current.Server != null)
                    _Root = HttpContext.Current.Server.MapPath("~/");
                else
                    _Root = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                _Root = _Root.TrimEnd('\\');

                return _Root;
            }
        }
    }
}
