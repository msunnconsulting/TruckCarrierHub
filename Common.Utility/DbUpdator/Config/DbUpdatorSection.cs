namespace Common.Utility.DbUpdator.Config
{
    using Logger;
    using System;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.IO;
    using System.Web;

    internal class DbUpdatorSection : ConfigurationSection
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
                this["connectionString"] = value;
                _connectionString = null;
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
                        catch (Exception ex)
                        {
                            var errorMsg = "DbUpdator : ConnectiongString setting of dbUpdator seems to have some invalid value. ConnectionString : " + _connectionString;
                            AppLogger.Instance.Log(new Exception(errorMsg, ex), LogType.Error);
                            throw new Exception(errorMsg);
                        }
                    }
                }
                return _connectionString;
            }
        }

        private string _dbFilesDirectory;
        [ConfigurationProperty("dbFilesDirectory", IsRequired = false, DefaultValue = "")]
        public String DbFilesDirectory
        {
            set
            {
                this["dbFilesDirectory"] = value;
                _dbFilesDirectory = null;
            }
            get
            {
                if (_dbFilesDirectory == null)
                {
                    _dbFilesDirectory = (String)this["dbFilesDirectory"];
                    if (!string.IsNullOrEmpty(_dbFilesDirectory))
                    {
                        if (_dbFilesDirectory.Contains("|root|"))
                            _dbFilesDirectory = _dbFilesDirectory.Replace("|root|", Config.Root);

                        if (!Directory.Exists(_dbFilesDirectory))
                        {
                            try
                            {
                                Directory.CreateDirectory(_dbFilesDirectory);
                            }
                            catch (Exception ex)
                            {
                                var errorMsg = "DbUpdator : Value of dbFilesDirectory is not a valid directory path : " + _dbFilesDirectory;
                                AppLogger.Instance.Log(ex, LogType.Error, errorMsg);
                                throw new Exception(errorMsg);
                            }
                        }
                    }
                }
                return _dbFilesDirectory;
            }
        }

        private string _dbScriptsDirectory;
        [ConfigurationProperty("dbScriptsDirectory", IsRequired = false, DefaultValue = "")]
        public String DbScriptsDirectory
        {
            set
            {
                this["dbScriptsDirectory"] = value;
                _dbScriptsDirectory = null;
            }
            get
            {
                if (_dbScriptsDirectory == null)
                {
                    _dbScriptsDirectory = (String)this["dbScriptsDirectory"];
                    if (!string.IsNullOrEmpty(_dbScriptsDirectory))
                    {
                        if (_dbScriptsDirectory.Contains("|root|"))
                            _dbScriptsDirectory = _dbScriptsDirectory.Replace("|root|", Config.Root);

                        if (!Directory.Exists(_dbScriptsDirectory))
                        {
                            try
                            {
                                Directory.CreateDirectory(_dbScriptsDirectory);
                            }
                            catch (DirectoryNotFoundException ex)
                            {
                                var errorMsg = "DbUpdator : Value of dbScriptsDirectory is not a valid directory path : " + _dbScriptsDirectory;
                                AppLogger.Instance.Log(ex, LogType.Error, errorMsg);
                                throw new Exception(errorMsg);
                            }
                        }
                    }
                }
                return _dbScriptsDirectory;
            }
        }

        [ConfigurationProperty("backup", IsRequired = false)]
        public Backup Backup
        {
            get
            {
                return (Backup)this["backup"];
            }
        }

        [ConfigurationProperty("createDb", IsRequired = false, DefaultValue = true)]
        public bool CreateDb
        {
            set { this["createDb"] = value; }

            get
            {
                return (bool)this["createDb"];
            }
        }

        [ConfigurationProperty("enableLog", IsRequired = false, DefaultValue = false)]
        public bool EnableLog
        {
            set { this["enableLog"] = value; }

            get
            {
                return (bool)this["enableLog"];
            }
        }
    }

    // Define the "textLogger" element
    // with "filePath" attribute.
    internal class Backup : ConfigurationElement
    {
        public override bool IsReadOnly()
        {
            return false;
        }

        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
        public bool Enabled
        {
            set { this["enabled"] = value; }

            get
            {
                return (bool)this["enabled"];
            }
        }

        private string _backupDirectory;
        [ConfigurationProperty("backupDirectory", IsRequired = false, DefaultValue = "")]
        public String BackupDirectory
        {
            set
            {
                this["backupDirectory"] = value;
                _backupDirectory = null;
            }
            get
            {
                if (_backupDirectory == null)
                {
                    _backupDirectory = (String)this["backupDirectory"];
                    if (!string.IsNullOrEmpty(_backupDirectory))
                    {
                        if (_backupDirectory.Contains("|root|"))
                            _backupDirectory = _backupDirectory.Replace("|root|", Config.Root);

                        if (!Directory.Exists(_backupDirectory))
                        {
                            try
                            {
                                Directory.CreateDirectory(_backupDirectory);
                            }
                            catch (DirectoryNotFoundException ex)
                            {
                                var errorMsg = "DbUpdator : Value of backupDirectory is not a valid directory path : " + _backupDirectory;
                                AppLogger.Instance.Log(ex, LogType.Error, errorMsg);
                                throw new Exception(errorMsg);
                            }
                        }
                    }
                }
                return _backupDirectory;
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
