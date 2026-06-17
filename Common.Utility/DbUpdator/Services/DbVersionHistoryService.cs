namespace Common.Utility.DbUpdator.Services
{
    using ADO.DAL;
    using Models;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    internal class DbVersionHistoryService
    {
        private readonly ADODbContext DbContext;
        public DbVersionHistoryService(ADODbContext dbContext)
        {
            this.DbContext = dbContext;
        }

        public void CreateTable()
        {
            if (this.DbContext.IsTableExist("DbVersionHistory"))
                return;

            string qCreateTable = "CREATE TABLE [dbo].[DbVersionHistory]([Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, [OldMajor] BIGINT NULL, [OldMinor] BIGINT NULL, [OldBuild] BIGINT NULL, [NewMajor] BIGINT NOT NULL, [NewMinor] BIGINT NOT NULL, [NewBuild] BIGINT NOT NULL, [DateCreated] DATETIME NOT NULL)";
            this.DbContext.ExecuteNonQuery(qCreateTable);
        }

        public long Add(DbVersionHistory dbVersionHistory)
        {
            string qAdd = @"INSERT INTO DbVersionHistory (
                                            OldMajor, 
                                            OldMinor, 
                                            OldBuild, 
                                            NewMajor, 
                                            NewMinor, 
                                            NewBuild, 
                                            DateCreated
                                            ) Values( 
                                            @OldMajor,
                                            @OldMinor, 
                                            @OldBuild, 
                                            @NewMajor, 
                                            @NewMinor, 
                                            @NewBuild, 
                                            @DateCreated 
                                            )  SELECT SCOPE_IDENTITY() as DbVersionHistoryId";


            List<IDbDataParameter> parameters = new List<IDbDataParameter>();
            parameters.Add(new SqlParameter("@OldMajor", dbVersionHistory.OldMajor));
            parameters.Add(new SqlParameter("@OldMinor", dbVersionHistory.OldMinor));
            parameters.Add(new SqlParameter("@OldBuild", dbVersionHistory.OldBuild));
            parameters.Add(new SqlParameter("@NewMajor", dbVersionHistory.NewMajor));
            parameters.Add(new SqlParameter("@NewMinor", dbVersionHistory.NewMinor));
            parameters.Add(new SqlParameter("@NewBuild", dbVersionHistory.NewBuild));
            parameters.Add(new SqlParameter("@DateCreated", dbVersionHistory.DateCreated));

            dbVersionHistory = this.DbContext.FirstOrDefault<DbVersionHistory>(qAdd, parameters);

            return dbVersionHistory.Id;
        }

        public DbVersionHistory Last()
        {
            string qFirstOrDefault = "Select TOP 1 * FROM DbVersionHistory Order By Id Desc";
            return this.DbContext.FirstOrDefault<DbVersionHistory>(qFirstOrDefault);
        }
    }
}
