namespace Common.Utility.Logger
{
    using ADO.DAL;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;

    internal sealed class LoggerServiceDB : ILoggerService
    {
        private readonly DbLogger Config;
        private readonly ADODbContext DB;

        public LoggerServiceDB()
        {
            var config = ((LoggerSection)ConfigurationManager.GetSection("logger"));
            if (config == null) { throw new Exception("Missing logger configuration section."); }

            this.Config = config.DbLogger;
            if (string.IsNullOrEmpty(this.Config.ConnectionString)) { throw new Exception("logger section is missing ConnectionString setting."); }
            if (string.IsNullOrEmpty(this.Config.TableName)) { throw new Exception("logger section is missing TableName setting."); }

            this.DB = new ADODbContext(this.Config.ConnectionString);
            this.Init();
        }

        private void Init()
        {
            try
            {
                var isTableExist = this.DB.IsTableExist(this.Config.TableName);
                if (!isTableExist)
                {
                    var qryCreateTable = "CREATE TABLE [dbo].[" + this.Config.TableName + @"] ( 
    [Id]             INT  IDENTITY (1, 1) NOT NULL,
    [Message]        NVARCHAR (255)  NOT NULL,
    [Description]    NVARCHAR (MAX)  NULL,    
    [LogType]        TINYINT NOT NULL,
    [LogDate]    DATE NOT NULL,
    CONSTRAINT [PK_dbo." + this.Config.TableName + "] PRIMARY KEY CLUSTERED ([Id] ASC))";
                    this.DB.ExecuteNonQuery(qryCreateTable);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Logger Error : Couldn't connect to server or database does not exist or table cannot be created. Please check connection string for logger and database permissions", ex);
            }
        }

        public override void Log(LogInfo logInfo)
        {
            var qry = "INSERT INTO [dbo].[" + this.Config.TableName + @"](
            [Message],[Description],[LogType],[LogDate]
            ) 
            Values(
            @Message,@Description,@LogType,@LogDate
            )";

            List<IDbDataParameter> parameters = new List<IDbDataParameter>();
            parameters.Add(new SqlParameter("@Message", logInfo.Message));
            parameters.Add(new SqlParameter("@Description", this.DB.GetNullableStringPara(logInfo.Description)));
            parameters.Add(new SqlParameter("@LogType", logInfo.LogType));
            parameters.Add(new SqlParameter("@LogDate", logInfo.LogDate));

            this.DB.ExecuteNonQuery(qry, parameters);
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
            throw new Exception("This GetLog method can only be used if logtype is set to 'text'");
        }

        public override List<LogInfo> GetDbLog()
        {
            return this.DB.Select<LogInfo>("select * from [dbo].[" + this.Config.TableName + "]");
        }

        public override void ClearLog()
        {
            this.DB.ExecuteNonQuery("delete from [dbo].[" + this.Config.TableName + "]");
        }
    }
}
