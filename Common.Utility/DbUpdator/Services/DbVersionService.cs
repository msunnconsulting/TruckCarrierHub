namespace Common.Utility.DbUpdator.Services
{
    using ADO.DAL;
    using Models;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    internal class DbVersionService
    {
        private readonly ADODbContext DbContext;
        public DbVersionService(ADODbContext dbContext)
        {
            this.DbContext = dbContext;
        }

        public DbVersion FirstOrDefault()
        {
            string qFirstOrDefault = "Select TOP 1 * FROM DbVersion Order By Id Desc";
            return this.DbContext.FirstOrDefault<DbVersion>(qFirstOrDefault);
        }

        public void CreateTable()
        {
            if (this.DbContext.IsTableExist("DbVersion"))
                return;

            string qCreateTable = "CREATE TABLE [dbo].[DbVersion] ([Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, [Major] BIGINT NOT NULL, [Minor] BIGINT NOT NULL, [Build] BIGINT NOT NULL, [DateUpdated] DATETIME NOT NULL)";
            this.DbContext.ExecuteNonQuery(qCreateTable);
        }

        public long Add(DbVersion dbVersion)
        {
            string qAdd = "INSERT INTO DbVersion (" +
                                              "Major," +
                                              "Minor," +
                                              "Build," +
                                              "DateUpdated" +
                                              ") Values(" +
                                              "@Major," +
                                              "@Minor," +
                                              "@Build," +
                                              "@DateUpdated" +
                                              ")  SELECT SCOPE_IDENTITY() as DbVersionId";

            List<IDbDataParameter> parameters = new List<IDbDataParameter>();
            parameters.Add(new SqlParameter("@Major", dbVersion.Major));
            parameters.Add(new SqlParameter("@Minor", dbVersion.Minor));
            parameters.Add(new SqlParameter("@Build", dbVersion.Build));
            parameters.Add(new SqlParameter("@DateUpdated", dbVersion.DateUpdated));

            dbVersion = this.DbContext.FirstOrDefault<DbVersion>(qAdd, parameters);
            return dbVersion.Id;
        }

        public void UpdateVersion(DbVersion dbVersion)
        {

            string qUpdateVersion = "Update DbVersion " +
                                            "Set Major = @Major," +
                                            "Minor = @Minor," +
                                            "Build = @Build," +
                                            "DateUpdated = @DateUpdated";

            List<IDbDataParameter> parameters = new List<IDbDataParameter>();
            parameters.Add(new SqlParameter("@Major", dbVersion.Major));
            parameters.Add(new SqlParameter("@Minor", dbVersion.Minor));
            parameters.Add(new SqlParameter("@Build", dbVersion.Build));
            parameters.Add(new SqlParameter("@DateUpdated", dbVersion.DateUpdated));

            this.DbContext.ExecuteNonQuery(qUpdateVersion, parameters);

        }
    }
}
