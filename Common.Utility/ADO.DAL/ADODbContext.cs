namespace Common.Utility.ADO.DAL
{
    using Extensions;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;


    /// <summary>
    /// Enum to identify SQL Server Auth mode to be used for connection string
    /// </summary>
    public enum SQLServerAuthenticationMode
    {
        /// <summary>
        /// Windows Auth
        /// </summary>
        Windows = 0,

        /// <summary>
        /// SQL Server Auth
        /// </summary>
        SQLServer = 1
    }

    /// <summary>
    /// ADODbContext class that is used to do all the actions using ADODb
    /// </summary>
    public class ADODbContext
    {
        # region Private Property

        /// <summary>
        /// Returns the connection string used by this context
        /// </summary>
        private readonly string ConnectionString;

        /// <summary>
        /// Returns the connection string to master database for this context
        /// Master connection string is created by replacing orginal db name by name master
        /// </summary>
        private readonly string MasterConnectionString;

        private const string replaceGoPattern = "^[\\s]*?GO[\\s]*?$";

        # endregion

        # region Constructors

        /// <summary>
        /// Constructor with connectionstring
        /// </summary>
        /// <param name="connectionString"></param>
        public ADODbContext(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException("connectionString");
            var conStr = Convert.ToString(ConfigurationManager.ConnectionStrings[connectionString]);
            if (!string.IsNullOrEmpty(conStr))
                connectionString = conStr;
            this.ConnectionString = connectionString;

            SqlConnectionStringBuilder sbSQL = new SqlConnectionStringBuilder(this.ConnectionString);
            sbSQL.InitialCatalog = "master";
            this.MasterConnectionString = sbSQL.ConnectionString;
        }

        /// <summary>
        /// Constructor with separate information about connection string form which constr is built
        /// </summary>
        /// <param name="host"></param>
        /// <param name="dbName"></param>
        /// <param name="loginId"></param>
        /// <param name="password"></param>
        /// <param name="authenticationMode"></param>
        public ADODbContext(string host, string dbName, string loginId, string password, SQLServerAuthenticationMode authenticationMode)
        {
            if (string.IsNullOrEmpty(host)) throw new ArgumentNullException("host");
            if (string.IsNullOrEmpty(dbName)) throw new ArgumentNullException("dbName");
            SqlConnectionStringBuilder sqlSB = new SqlConnectionStringBuilder();
            sqlSB.DataSource = host;
            sqlSB.InitialCatalog = dbName;
            if (authenticationMode == SQLServerAuthenticationMode.SQLServer)
            {
                sqlSB.IntegratedSecurity = false;
                sqlSB.UserID = loginId;
                sqlSB.Password = password;
            }
            else
                sqlSB.IntegratedSecurity = true;
            this.ConnectionString = sqlSB.ConnectionString;

            SqlConnectionStringBuilder sbSQL = new SqlConnectionStringBuilder(this.ConnectionString);
            sbSQL.InitialCatalog = "master";
            this.MasterConnectionString = sbSQL.ConnectionString;
        }

        #endregion

        # region Database & Table Related Methods

        /// <summary>
        /// Returns true if database exist or send false
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public bool IsDatabaseExist(string dbName)
        {
            if (dbName == null) throw new ArgumentNullException("dbName");

            var qry = string.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'", dbName);
            object dbId = this.ExecuteScalar(qry, useMasterConnection: true);
            if (Convert.IsDBNull(dbId))
            {
                qry = string.Format("select db_id('{0}')", dbName);
                dbId = this.ExecuteScalar(qry, useMasterConnection: true);
            }

            return !Convert.IsDBNull(dbId);
        }

        /// <summary>
        /// Takes the backup of the database in the backup directory specified.
        /// </summary>
        /// <param name="dbName">database to be backedup</param>
        /// <param name="backupDirectory">directory in which backup will be taken</param>
        /// <param name="backupFileName">name of the backup file</param>
        public void BackupDatabase(string dbName, string backupDirectory, string backupFileName = null)
        {
            if (dbName == null) throw new ArgumentNullException("dbName");
            if (backupDirectory == null) throw new ArgumentNullException("backupDirectory");

            if (!this.IsDatabaseExist(dbName))
                throw new Exception("Cannot backup database : " + dbName + ". Database does not exist. ConnectionString : " + this.ConnectionString);
            if (backupFileName == null)
                backupFileName = dbName + ".bak";
            string qry = "BACKUP DATABASE " + dbName + " TO DISK = '" + Path.Combine(backupDirectory, backupFileName) + "'";
            this.ExecuteNonQuery(qry, useMasterConnection: true);
        }

        /// <summary>
        /// Creates a database with physical files stored at database files directory
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="dbFilesDirectory"></param>
        public void CreateDatabase(string dbName, string dbFilesDirectory)
        {
            string qCreateDatabase = "CREATE DATABASE " + dbName + " ON PRIMARY " +
                "(NAME = " + dbName + "_Data, " +
        "FILENAME = " + "'" + Path.Combine(dbFilesDirectory, dbName + ".mdf") + "') " +
        "LOG ON (NAME = MyDatabase_Log, " +
        "FILENAME = '" + Path.Combine(dbFilesDirectory, dbName + ".ldf") + "')";
            this.ExecuteNonQuery(qCreateDatabase, useMasterConnection: true);
        }

        /// <summary>
        /// Returns if table exist or not
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public bool IsTableExist(string tableName, string schema = "dbo")
        {
            // check if table doesn't exist, create tables
            var qryIfTableExist = @"SELECT count(1) 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = '" + schema + @"' 
                 AND  TABLE_NAME = '" + tableName + "'";
            var count = this.ExecuteCount(qryIfTableExist);
            return (count != 0);
        }

        /// <summary>
        /// Try a connection, if it fails, calling method gets an exception
        /// </summary>
        public void TryConnect()
        {
            IDbConnection conn = null;
            try
            {
                // get an open connection
                conn = this.GetConnection(true);
            }
            finally
            {
                this.CloseConnection(conn);
            }
        }

        # endregion

        # region Executing Script From File & Directory

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

        /// <summary>
        /// Executes all the scripts in a directory. Files are taken in asending order of file name.
        /// Either all the scripts in all the files are executed or none.
        /// </summary>
        /// <param name="dirPath">Path of the directory inside which files resides</param>
        /// <param name="useMasterConnection"></param>
        public void ExecuteScriptsInDirectory(string dirPath, bool useMasterConnection = false)
        {
            FileInfo[] scriptFiles = GetFilesOrderByName(dirPath, "*.sql");

            using (SqlConnection cn = new SqlConnection(useMasterConnection ? this.MasterConnectionString : this.ConnectionString))
            {
                SqlTransaction trans = null;
                string currentFile = null;
                int goCount = -1;
                try
                {

                    cn.Open();
                    trans = cn.BeginTransaction();

                    foreach (FileInfo script in scriptFiles)
                    {
                        currentFile = script.FullName;
                        goCount = ExecuteScriptInFile(script.FullName, cn, trans);
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    if (trans != null)
                        trans.Rollback();

                    var errorEx = new Exception("Error while executing script in file : " + currentFile + Environment.NewLine
                        + "Go Count In Script File : " + goCount.ToString() + Environment.NewLine
                        + "Regex Used to split by GO : " + replaceGoPattern, ex);
                    throw errorEx;
                }
            }
        }

        /// <summary>
        /// Execute script file provided in files one by one. Either all scripts executed successfully or none
        /// </summary>
        /// <param name="scriptFiles"></param>
        /// <param name="useMasterConnection"></param>
        public void ExecuteScriptInFiles(FileInfo[] scriptFiles, bool useMasterConnection = false)
        {
            using (SqlConnection cn = new SqlConnection(useMasterConnection ? this.MasterConnectionString : this.ConnectionString))
            {
                SqlTransaction trans = null;
                string currentFile = null;
                int goCount = -1;
                try
                {

                    cn.Open();
                    trans = cn.BeginTransaction();

                    foreach (FileInfo script in scriptFiles)
                    {
                        currentFile = script.FullName;
                        goCount = ExecuteScriptInFile(script.FullName, cn, trans);
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    if (trans != null)
                        trans.Rollback();

                    var errorEx = new Exception("Error while executing script in file : " + currentFile + Environment.NewLine
                        + "Go Count In Script File : " + goCount.ToString() + Environment.NewLine
                        + "Regex Used to split by GO : " + replaceGoPattern, ex);
                    throw errorEx;
                }
            }
        }

        /// <summary>
        /// Execute script in a specific file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="useMasterConnection"></param>
        public void ExecuteScriptInFile(string filePath, bool useMasterConnection = false)
        {
            int goCount = -1;
            using (SqlConnection cn = new SqlConnection(useMasterConnection ? this.MasterConnectionString : this.ConnectionString))
            {
                SqlTransaction trans = null;
                try
                {

                    cn.Open();
                    trans = cn.BeginTransaction();

                    goCount = ExecuteScriptInFile(filePath, cn, trans);

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    if (trans != null)
                        trans.Rollback();

                    var errorEx = new Exception("Error while executing script in file : " + filePath + Environment.NewLine
                        + "Go Count In Script File : " + goCount.ToString() + Environment.NewLine
                        + "Regex Used to split by GO : " + replaceGoPattern, ex);
                    throw errorEx;
                }
            }
        }

        /// <summary>
        /// Execute script in a specific file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="cn"></param>
        /// <param name="trans"></param>
        private int ExecuteScriptInFile(string filePath, SqlConnection cn, SqlTransaction trans)
        {
            string commandText = string.Empty;
            string[] lines = null;
            int goCount = 0;
            using (Stream stream = File.OpenRead(filePath))
            {

                // read the entire file and split to separate commands split by GO
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    commandText = streamReader.ReadToEnd().Trim();
                }

                if (!string.IsNullOrEmpty(commandText))
                {
                    Regex regex = new Regex(replaceGoPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    goCount = regex.Matches(commandText).Count;
                    lines = regex.Split(commandText);

                    using (SqlCommand cmd = new SqlCommand(commandText, cn))
                    {
                        if (trans != null)
                            cmd.Transaction = trans;

                        // execute the file line by line
                        foreach (string line in lines)
                        {
                            var cmdText = line.Trim();
                            if (cmdText.Length > 0)
                            {
                                cmd.CommandTimeout = 300;
                                cmd.CommandText = cmdText;
                                cmd.CommandType = CommandType.Text;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            return goCount;
        }



        #endregion

        # region Query Execution Methods

        /// <summary>
        /// Executes a scaler query and return value as object
        /// </summary>
        /// <param name="commandText">scaler query to be executed</param>
        /// <param name="parameters">sql parameters for the query</param>
        /// <param name="cmdType">command type for the query</param>
        /// <param name="useMasterConnection">indicates if we should use master connection string or db connection string</param>
        /// <returns></returns>
        public object ExecuteScalar(string commandText, List<IDbDataParameter> parameters = null, CommandType cmdType = CommandType.Text, bool useMasterConnection = false)
        {
            IDbCommand cmd = null;
            try
            {
                cmd = this.GetCommand(commandText, cmdType, useMasterConnection);
                if (parameters != null)
                    parameters.ToList().ForEach(m => cmd.Parameters.Add(m));
                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseCommand(cmd);
            }
        }

        /// <summary>
        /// Returns the scaler value converted to integer as a result. Mostly used for count
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public int ExecuteCount(string commandText, List<IDbDataParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            return (int)this.ExecuteScalar(commandText, parameters, cmdType);
        }

        /// <summary>
        /// Executes non query and returns number of rows affected
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="cmdType"></param>
        /// <param name="useMasterConnection"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string commandText, List<IDbDataParameter> parameters = null, CommandType cmdType = CommandType.Text, bool useMasterConnection = false)
        {
            IDbCommand cmd = null;
            try
            {
                cmd = this.GetCommand(commandText, cmdType, useMasterConnection);
                if (parameters != null)
                    parameters.ToList().ForEach(m => cmd.Parameters.Add(m));
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseCommand(cmd);
            }
        }

        /// <summary>
        /// Selects more than one record and returns the list of object. Values are filled in object by mapping columnname to class property name
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public List<R> Select<R>(string commandText, List<IDbDataParameter> parameters = null, CommandType cmdType = CommandType.Text) where R : class, new()
        {
            IDataReader reader = null;
            IDbCommand cmd = null;
            try
            {
                cmd = this.GetCommand(commandText, cmdType);
                if (parameters != null)
                    parameters.ToList().ForEach(m => cmd.Parameters.Add(m));
                reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
                List<R> lstResult = new List<R>();
                while (reader.Read())
                    lstResult.Add(this.ReadObject<R>(reader));
                return lstResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseReader(reader);
                CloseCommand(cmd);
            }
        }


        public IQueryable<R> SelectForPagination<R>(string commandText, List<IDbDataParameter> parameters = null, CommandType cmdType = CommandType.Text) where R : class, new()
        {
            IDataReader reader = null;
            IDbCommand cmd = null;
            try
            {
                cmd = this.GetCommand(commandText, cmdType);
                if (parameters != null)
                    parameters.ToList().ForEach(m => cmd.Parameters.Add(m));
                reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
                List<R> lstResult = new List<R>();


                while (reader.Read())
                {
                    lstResult.Add(this.ReadObject<R>(reader));
                }
                // IQueryable listResult = Enumerable.Empty<R>().AsQueryable();
                IQueryable<R> listResult = null;
                listResult.Concat(lstResult);
                return listResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseReader(reader);
                CloseCommand(cmd);
            }
        }

        /// <summary>
        /// Selects the first record from database and return the object fileed from that record.
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public R FirstOrDefault<R>(string commandText, List<IDbDataParameter> parameters = null, CommandType cmdType = CommandType.Text) where R : class, new()
        {
            IDataReader reader = null;
            IDbCommand cmd = null;
            try
            {
                cmd = this.GetCommand(commandText, cmdType);
                if (parameters != null)
                    parameters.ToList().ForEach(m => cmd.Parameters.Add(m));
                reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read())
                    return this.ReadObject<R>(reader);
                else
                    return null;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseReader(reader);
                CloseCommand(cmd);
            }
        }

        /// <summary>
        /// can fire one or more select queries and Returns one dataset with number of tables same as number of result set.
        /// </summary>        
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public DataSet Select(string commandText, List<IDbDataParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            IDataReader reader = null;
            IDbDataAdapter adpt = null;
            DataSet ds = new DataSet();
            try
            {
                adpt = GetAdapter(commandText, cmdType);
                if (parameters != null)
                    parameters.ToList().ForEach(m => adpt.SelectCommand.Parameters.Add(m));
                adpt.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseReader(reader);
                CloseAdapter(adpt);
            }
        }

        #endregion

        # region Core ADO Methods

        /// <summary>
        /// Converts reader record to object
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        private R ReadObject<R>(IDataReader reader) where R : class, new()
        {
            R obj = new R();
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);
            foreach (var prop in properties)
            {
                if (reader.HasColumn(prop.Name))
                    obj.SetPropertyValue(prop.Name, reader[prop.Name]);
            }
            return obj;
        }

        /// <summary>
        /// Return an open connection
        /// </summary>
        /// <param name="useMasterConnnection">indicates master or normal db connection string to be used</param>
        /// <returns>Return an open connection</returns>
        private IDbConnection GetConnection(bool useMasterConnnection = false)
        {
            IDbConnection conn = new SqlConnection(useMasterConnnection ? this.MasterConnectionString : this.ConnectionString);
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Gets a command Object for the given query
        /// </summary>
        /// <param name="commandText">command text for the command object</param>
        /// <param name="cmdType">command type for the command object</param>
        /// <param name="useMasterConnection">indicates if we should use master or normal connection string</param>
        /// <returns></returns>
        private IDbCommand GetCommand(string commandText, CommandType cmdType, bool useMasterConnection = false)
        {
            IDbCommand command = new SqlCommand(commandText, GetConnection(useMasterConnection) as SqlConnection);
            command.CommandType = cmdType;
            return command;
        }

        private IDbDataAdapter GetAdapter(string commandText, CommandType cmdType, bool useMasterConnection = false)
        {
            IDbDataAdapter adpt = new SqlDataAdapter();
            adpt.SelectCommand = GetCommand(commandText, cmdType, useMasterConnection);
            return adpt;
        }

        private void CloseAdapter(IDbDataAdapter adpt, bool closeCommand = true)
        {
            if (adpt == null) return;
            if (closeCommand)
                CloseCommand(adpt.SelectCommand, closeCommand);
            adpt = null;
        }

        /// <summary>
        /// CLose and Dispose a Connection
        /// </summary>
        /// <param name="conn"></param>
        private void CloseConnection(IDbConnection conn)
        {
            if (conn == null) return;
            if (conn.State == ConnectionState.Open)
                conn.Close();
            conn.Dispose();
        }

        /// <summary>
        /// Dispose a command and its connection
        /// </summary>
        /// <param name="command"></param>
        /// <param name="closeConnection"></param>
        private void CloseCommand(IDbCommand command, bool closeConnection = true)
        {
            if (command == null) return;
            var conn = command.Connection;
            if (closeConnection && conn != null) CloseConnection(conn);
            command.Dispose();
        }

        /// <summary>
        /// CLose this Reader
        /// </summary>
        /// <param name="reader"></param>
        private void CloseReader(IDataReader reader)
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
        }

        #endregion

        # region Read Values From Reader

        /// <summary>
        /// read value from reader and return string or null value.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private string ReadString(IDataReader reader, string column)
        {
            var value = reader[column];
            if (Convert.IsDBNull(value))
            {
                return null;
            }
            return (string)value;
        }

        /// <summary>
        /// Read a specified column from the reader as return as int
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private int ReadInt(IDataReader reader, string column)
        {
            return Convert.ToInt32(reader[column].ToString());
        }

        /// <summary>
        /// Read a specified column from the reader as return as long
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private long ReadLong(IDataReader reader, string column)
        {
            return Convert.ToInt64(reader[column].ToString());
        }

        /// <summary>
        /// read a specified column from the reader and return int or null value
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private int? ReadNullableInt(IDataReader reader, string column)
        {
            var value = reader[column];
            if (Convert.IsDBNull(value))
            {
                return null;
            }
            return (int?)value;
        }

        /// <summary>
        /// read a specified column from the reader and return long or null value
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private long? ReadNullableLong(IDataReader reader, string column)
        {
            var value = reader[column];
            if (Convert.IsDBNull(value))
            {
                return null;
            }
            return (long?)value;
        }

        /// <summary>
        /// read a specified column from the reader and return datetime value or minimum datetime
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private DateTime ReadDateTime(IDataReader reader, string column)
        {
            var value = reader[column];
            if (Convert.IsDBNull(value))
            {
                return DateTime.MinValue;
            }
            return (DateTime)value;
        }

        /// <summary>
        /// read a specified column from a reader and return datetime or null
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private DateTime? ReadNullableDateTime(IDataReader reader, string column)
        {
            var value = reader[column];
            if (Convert.IsDBNull(value))
            {
                return null;
            }
            return (DateTime)value;
        }

        /// <summary>
        /// // read a specified column from a reader and return boolean  or if it is  null then return false
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private bool ReadBool(IDataReader reader, string column)
        {
            var value = reader[column];
            if (Convert.IsDBNull(value))
            {
                return false;
            }
            return (bool)value;
        }

        /// <summary>
        ///  read a specified column from a reader and return boolean or null value
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private bool? ReadNullableBool(IDataReader reader, string column)
        {
            var value = reader[column];
            if (Convert.IsDBNull(value))
            {
                return null;
            }
            return (bool)value;
        }

        # endregion

        # region Get Parameters Values

        /// <summary>
        /// pass parameter and return bool or null value
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public object GetNullableBoolPara(bool? para)
        {
            if (para.HasValue)
                return para.Value;
            else
                return DBNull.Value;
        }

        /// <summary>
        /// pass parameter and return int or null value
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public object GetNullableIntPara(int? para)
        {
            if (para.HasValue)
                return para.Value;
            else
                return DBNull.Value;
        }

        /// <summary>
        /// pass parameter and return parameter or null value
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public object GetNullableStringPara(string para)
        {
            if (para != null)
                return para;
            else
                return DBNull.Value;
        }

        /// <summary>
        /// pass character parameter and return parameter value or null value.
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public object GetNullableCharPara(char? para)
        {
            if (para.HasValue && para != char.MinValue)
                return para;
            else
                return DBNull.Value;
        }

        /// <summary>
        /// pass datetime parameter and return parameter or null value
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public object GetNullableDateTimePara(DateTime? para)
        {
            if (para.HasValue)
                return para.Value;
            else
                return DBNull.Value;
        }

        #endregion
    }
}
