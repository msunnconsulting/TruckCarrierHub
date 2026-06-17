namespace Common.Utility.DbUpdator.Services
{
    using ADO.DAL;
    using Logger;
    using Models;
    using System;
    using System.Data.SqlClient;
    using System.IO;


    internal sealed class DbUpdatorService
    {
        private readonly string DbName;
        private readonly ADODbContext DbContext;
        private readonly DbVersionService dbVersionService;
        private readonly DbVersionHistoryService dbVersionHistoryService;
        private readonly DbScriptExecutionService dbScriptExecutionService;
        //private readonly DbUpdatorSection Config;        
        public DbUpdatorService()
        {

            //this.Config = (DbUpdatorSection)ConfigurationManager.GetSection("dbUpdator");
            //if (this.Config == null) { throw new Exception("Missing dbUpdator configuration section."); }            

            if (string.IsNullOrEmpty(DbUpdator.Config.ConnectionString)) { throw new Exception("dbUpdator section is missing ConnectionString setting."); }
            if (string.IsNullOrEmpty(DbUpdator.Config.DbFilesDirectory)) { throw new Exception("dbUpdator section is missing DbFilesDirectory setting."); }
            if (string.IsNullOrEmpty(DbUpdator.Config.DbScriptsDirectory)) { throw new Exception("dbUpdator section is missing DbScriptsDirectory setting."); }

            try
            {
                SqlConnectionStringBuilder conStr = new SqlConnectionStringBuilder(DbUpdator.Config.ConnectionString);
                this.DbName = conStr.InitialCatalog;
            }
            catch (Exception ex)
            {
                var errorEx = new Exception("DbUpdator : ConnectiongString setting of dbUpdator seems to have some invalid value. ConnectionString : " + DbUpdator.Config.ConnectionString, ex);
                AppLogger.Instance.Log(errorEx, LogType.Error);
                throw errorEx;
            }

            this.DbContext = new ADODbContext(DbUpdator.Config.ConnectionString);
            this.dbVersionHistoryService = new DbVersionHistoryService(this.DbContext);
            this.dbVersionService = new DbVersionService(this.DbContext);
            this.dbScriptExecutionService = new DbScriptExecutionService(this.DbContext);

            if (DbUpdator.EnableLog)
            {
                AppLogger.Instance.Log("DbUpdator : DbUpdatorService Initialized", LogType.Info);
                AppLogger.Instance.Log("DbUpdator : DbName : " + this.DbName, LogType.Info);
                AppLogger.Instance.Log("DbUpdator : Connection String : " + DbUpdator.ConnectionString, LogType.Info);
                AppLogger.Instance.Log("DbUpdator : DbFilesDirectory : " + DbUpdator.DbFilesDirectory, LogType.Info);
                AppLogger.Instance.Log("DbUpdator : DbScriptsDirectory : " + DbUpdator.DbScriptsDirectory, LogType.Info);
                AppLogger.Instance.Log("DbUpdator : BackupDirectory : " + DbUpdator.BackupDirectory, LogType.Info);
                AppLogger.Instance.Log("DbUpdator : BackupEnabled : " + DbUpdator.BackupEnabled, LogType.Info);
                AppLogger.Instance.Log("DbUpdator : CreateDb : " + DbUpdator.CreateDb, LogType.Info);
                AppLogger.Instance.Log("DbUpdator : EnableLog : " + DbUpdator.EnableLog, LogType.Info);
            }

        }

        private void CreateDatabaseIfDoesNotExist()
        {

            bool isDbExist = this.DbContext.IsDatabaseExist(this.DbName);
            // create database            
            if (!isDbExist)
            {
                // if db doesn't exist, then check if createdb is allowed or not.
                // if not allowed throw exception

                if (!DbUpdator.Config.CreateDb)
                {
                    var logEx = new Exception("DbUpdator : Database doesn't exist and Create database is not enabled. Please create a blank database or enable 'createDb' option in dbupdator");
                    AppLogger.Instance.Log(logEx, LogType.Error);
                    throw logEx;
                }

                try
                {
                    if (DbUpdator.EnableLog) AppLogger.Instance.Log("DbUpdator : Trying To Create Database", LogType.Info);
                    this.DbContext.CreateDatabase(this.DbName, DbUpdator.Config.DbFilesDirectory);
                    if (DbUpdator.EnableLog) AppLogger.Instance.Log("DbUpdator : Database created successfully", LogType.Info);
                }
                catch (Exception ex)
                {
                    var logEx = new Exception("DbUpdator : Create database failed. DbName : " + this.DbName + " DbFilesDirectory : " + DbUpdator.Config.DbFilesDirectory, ex);
                    AppLogger.Instance.Log(logEx, LogType.Error);
                    throw logEx;
                }

            }
            else
                if (DbUpdator.EnableLog) AppLogger.Instance.Log("DbUpdator : Database already exist : " + this.DbName, LogType.Info);

            // at this point database will always exist.
            try
            {
                bool isVersioningTablesExist = this.DbContext.IsTableExist("DbVersion");
                if (!isVersioningTablesExist)
                {
                    if (DbUpdator.EnableLog) AppLogger.Instance.Log("DbUpdator : Trying To Create dbVersion table", LogType.Info);
                    this.dbVersionService.CreateTable();
                    if (DbUpdator.EnableLog) AppLogger.Instance.Log("DbUpdator : DbVersion table created successfully", LogType.Info);
                }

                isVersioningTablesExist = this.DbContext.IsTableExist("DbVersionHistory");
                if (!isVersioningTablesExist)
                {
                    if (DbUpdator.EnableLog) AppLogger.Instance.Log("DbUpdator : Trying To Create dbVersionHistory table", LogType.Info);
                    this.dbVersionHistoryService.CreateTable();
                    if (DbUpdator.EnableLog) AppLogger.Instance.Log("DbUpdator : DbVersionHistory table created successfully", LogType.Info);
                }

                // this point versioning tables will always exist.
                if (DbUpdator.EnableLog) AppLogger.Instance.Log("DbUpdator : versioning tables exist now", LogType.Info);

            }
            catch (Exception ex)
            {
                var logEx = new Exception("DbUpdator : Create versioning tables for dbUpdator failed", ex);
                AppLogger.Instance.Log(logEx, LogType.Error);
                throw logEx;
            }

        }



        public void UpgradeDatabase()
        {
            if (DbUpdator.EnableLog) AppLogger.Instance.Log("DbUpdator : UpgradeDatabase Started", LogType.Info);
            // check if the values in config file has proper connection.
            try
            {
                this.DbContext.TryConnect();
            }
            catch (Exception ex)
            {
                var errorEx = new Exception("DbUpdator : Unable to upgrade database. Connection to the server failed. Unable reach to server or unable to connect to 'master'. Please verify the connection string. ConnectionString : " + DbUpdator.Config.ConnectionString, ex);
                AppLogger.Instance.Log(errorEx, LogType.Error);
                throw errorEx;
            }

            // check for the Version information in databse is available.
            this.CreateDatabaseIfDoesNotExist();

            // check if version tables exist
            bool isVersioningTablesExist = this.DbContext.IsTableExist("DbVersion");
            if (isVersioningTablesExist)
                isVersioningTablesExist = this.DbContext.IsTableExist("DbVersionHistory");
            if (!isVersioningTablesExist)
            {
                var errorEx = new Exception("DbUpdator : Database does not have the necessary database table DbVersion and/or DbVersionHistory. You need these tables to proceed further.");
                AppLogger.Instance.Log(errorEx, LogType.Error);
                throw errorEx;
            }

            DbVersion dbVersion = this.dbVersionService.FirstOrDefault();
            DbVersionHistory dbVersionHistory;
            // now we have successful connection to SQL Server using ADO 
            // there are two steps to be performed now.
            // Updgrade the databsae by executing scripts through 
            // Insert / Update Version information in the database.

            if (dbVersion == null)
                dbVersion = new DbVersion()
                {
                    Build = 0,
                    Major = 0,
                    Minor = 0,
                    Id = 0
                };
            var currentDatabaseVersion = new System.Version(dbVersion.Version);
            System.Version dirVersion;
            if (DbUpdator.EnableLog) AppLogger.Instance.Log("DbUpdator : Current Db Version : " + currentDatabaseVersion.ToString(), LogType.Info);
            var UpgradeScripts = dbScriptExecutionService.GetVirsionDirectoriesToUpgrade(DbUpdator.Config.DbScriptsDirectory, currentDatabaseVersion);
            if (DbUpdator.Config.Backup.Enabled && UpgradeScripts.Count > 0)
            {
                // if we are going to do some upgrade (new version is available) then take backup if backup is enabled                                            
                var dbFileName = this.DbName + "_DBV" + currentDatabaseVersion.ToString().Replace(".", string.Empty) + "_" + DateTime.UtcNow.ToString("yyyMMddHHmm") + ".bak";
                try
                {
                    this.DbContext.BackupDatabase(this.DbName, DbUpdator.Config.Backup.BackupDirectory, dbFileName);
                    if (DbUpdator.EnableLog) AppLogger.Instance.Log("DbUpdator : Backup Taken Successfully : " + this.DbName + " : " + DbUpdator.Config.Backup.BackupDirectory + " : " + dbFileName, LogType.Info);
                }
                catch (Exception ex)
                {
                    var errorEx = new Exception("DbUpdator : Unable to backup database : DbName : " + this.DbName + " : BackupPath : " + DbUpdator.Config.Backup.BackupDirectory, ex);
                    AppLogger.Instance.Log(errorEx, LogType.Error);
                    throw errorEx;
                }
            }
            else
            {
                if (DbUpdator.EnableLog)
                {
                    AppLogger.Instance.Log("DbUpdator : UpgradeScripts Count : " + UpgradeScripts.Count, LogType.Info);
                    AppLogger.Instance.Log("DbUpdator : BackupEnabled : " + DbUpdator.Config.Backup.Enabled, LogType.Info);
                    AppLogger.Instance.Log("DbUpdator : Backup not taken", LogType.Info);
                }
            }

            foreach (DirectoryInfo UpgradeScript in UpgradeScripts)
            {
                try
                {
                    dirVersion = Version.Parse(UpgradeScript.Name);
                    if (DbUpdator.EnableLog) AppLogger.Instance.Log("DbUpdator : Trying to upgrade to : " + dirVersion.ToString(), LogType.Info);
                    dbScriptExecutionService.ExecuteScriptsInDirectory(UpgradeScript.FullName, DbUpdator.Config.ConnectionString);
                    if (DbUpdator.EnableLog) AppLogger.Instance.Log("DbUpdator : Script executed successfully for version : " + dirVersion.ToString(), LogType.Info);

                    // inserting version history.
                    dbVersionHistory = new DbVersionHistory()
                    {
                        DateCreated = DateTime.UtcNow,
                        OldMajor = dbVersion.Major,
                        OldMinor = dbVersion.Minor,
                        OldBuild = dbVersion.Build,
                        NewMajor = dirVersion.Major,
                        NewMinor = dirVersion.Minor,
                        NewBuild = dirVersion.Build
                    };
                    dbVersionHistoryService.Add(dbVersionHistory);
                    if (DbUpdator.EnableLog) AppLogger.Instance.Log("DbUpdator : DbVersionHistory Added", LogType.Info);

                    // updating version information.
                    bool addDbVersion = false;
                    if (dbVersion.Major == 0 && dbVersion.Minor == 0 && dbVersion.Build == 0)
                        addDbVersion = true;

                    dbVersion.DateUpdated = DateTime.UtcNow;
                    dbVersion.Major = dirVersion.Major;
                    dbVersion.Minor = dirVersion.Minor;
                    dbVersion.Build = dirVersion.Build;

                    if (addDbVersion)
                    {
                        dbVersionService.Add(dbVersion);
                        if (DbUpdator.EnableLog) AppLogger.Instance.Log("DbUpdator : Dbversion Added", LogType.Info);
                    }
                    else
                    {
                        dbVersionService.UpdateVersion(dbVersion);
                        AppLogger.Instance.Log("DbUpdator : Dbversion Updated", LogType.Info);
                    }
                    currentDatabaseVersion = dirVersion;
                }
                catch (Exception ex)
                {
                    var errorEx = new Exception("DbUpdator : Unable to Upgrade to version " + UpgradeScript.Name, ex);
                    AppLogger.Instance.Log(errorEx, LogType.Error);
                    throw errorEx;
                }
            }
            if (DbUpdator.EnableLog) AppLogger.Instance.Log("DbUpdator : Final Current Db Version : " + currentDatabaseVersion.ToString(), LogType.Info);

        }
    }
}
