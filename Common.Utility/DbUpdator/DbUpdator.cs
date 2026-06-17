namespace Common.Utility.DbUpdator
{
    using Config;
    using Services;
    using System;
    using System.Configuration;

    public sealed class DbUpdator
    {
        private static DbUpdatorService dbUpdatorService;
        private static object syncRoot = new Object();
        private static volatile DbUpdator instance;
        public static DbUpdator Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            dbUpdatorService = new DbUpdatorService();
                            instance = new DbUpdator();
                        }
                    }
                }
                return instance;
            }
        }

        private static DbUpdatorSection _Config = null;
        internal static DbUpdatorSection Config
        {
            get
            {
                if (_Config == null)
                {
                    _Config = ((DbUpdatorSection)ConfigurationManager.GetSection("dbUpdator"));
                    if (_Config == null)
                        throw new Exception("missing configuration section for dbUpdator");

                }
                return _Config;
            }
        }

        public void UpgradeDatabase()
        {
            dbUpdatorService.UpgradeDatabase();
        }

        public static string ConnectionString
        {
            get { return Config.ConnectionString; }
            set
            {
                if (instance != null) throw new Exception("Please configure dbUpdator ConnectionString before it is used");
                Config.ConnectionString = value;
            }
        }

        public static string DbFilesDirectory
        {
            get { return Config.DbFilesDirectory; }
            set
            {
                if (instance != null) throw new Exception("Please configure dbUpdator DbFilesDirectory before it is used");
                Config.DbFilesDirectory = value;
            }
        }

        public static bool CreateDb
        {
            get
            {
                return Config.CreateDb;
            }
            set
            {
                if (instance != null) throw new Exception("Please configure dbUpdator CreateDb before it is used");
                Config.CreateDb = value;
            }
        }

        public static string DbScriptsDirectory
        {
            get { return Config.DbScriptsDirectory; }
            set
            {
                if (instance != null) throw new Exception("Please configure dbUpdator DbScriptsDirectory before it is used");
                Config.DbScriptsDirectory = value;
            }
        }

        public static string BackupDirectory
        {
            get { return Config.Backup.BackupDirectory; }
            set
            {
                if (instance != null) throw new Exception("Please configure dbUpdator BackupDirectory before it is used");
                Config.Backup.BackupDirectory = value;
            }
        }

        public static bool BackupEnabled
        {
            get
            {
                return Config.Backup.Enabled;
            }
            set
            {
                if (instance != null) throw new Exception("Please configure dbUpdator BackupEnabled before it is used");
                Config.Backup.Enabled = value;
            }
        }

        public static bool EnableLog
        {
            get
            {
                return Config.EnableLog;
            }
            set
            {
                if (instance != null) throw new Exception("Please configure dbUpdator EnableLog before it is used");
                Config.EnableLog = value;
            }
        }

    }
}
