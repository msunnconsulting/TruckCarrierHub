namespace Common.Utility.DbUpdator.Services
{
    using ADO.DAL;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal class DbScriptExecutionService
    {
        private readonly ADODbContext DbContext;
        public DbScriptExecutionService(ADODbContext dbContext)
        {
            this.DbContext = dbContext;
        }

        private FileInfo[] GetFilesOrderByName(string dirPath, string searchPattern)
        {
            List<FileInfo> lstFile = new List<FileInfo>();

            string[] files;
            if (string.IsNullOrEmpty(searchPattern))
                files = Directory.GetFiles(dirPath);
            else
                files = Directory.GetFiles(dirPath, searchPattern);
            files.ToList().ForEach(f => lstFile.Add(new FileInfo(f)));

            return lstFile.OrderBy(f => f.Name).ToArray<FileInfo>();
        }

        public List<DirectoryInfo> GetVirsionDirectoriesToUpgrade(string dirPath, Version currentDatabaseVersion)
        {
            List<DirectoryInfo> lstDir = new List<DirectoryInfo>();
            string[] dirs = Directory.GetDirectories(dirPath);
            dirs.ToList().ForEach(d => lstDir.Add(new DirectoryInfo(d)));
            System.Version dirVersion;
            return lstDir.Where(d => Version.TryParse(d.Name, out dirVersion) == true && dirVersion.CompareTo(currentDatabaseVersion) > 0)
                .OrderBy(d => new Version(d.Name)).ToList();
        }

        public void ExecuteScriptsInDirectory(string dirPath, string connectionString)
        {
            string conStr = connectionString;
            FileInfo[] scripts = GetFilesOrderByName(dirPath, "*.sql");
            this.DbContext.ExecuteScriptInFiles(scripts);
        }
    }
}
