namespace PartnerCarrier.Infrastructure.Services.Setup
{
    using Common.Utility;
    using Common.Utility.ADO.DAL;
    using Contracts.Setup;
    using PartnerCarrier.Infrastructure.Database;
    using PartnerCarrier.ViewModels.Setup;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    public class SqlManagerService : ISqlManagerService
    {
        #region Private Variable
        private readonly ADODbContext adoDb;
        private readonly PartnerCarrier_DevEntities db;
        #endregion

        #region Constructor
        public SqlManagerService(PartnerCarrier_DevEntities dba)
        {
            db = dba;
            adoDb = new ADODbContext(db.Database.Connection.ConnectionString);
        }
        #endregion

        #region CheckTypeOf Query
        public bool checkTypeOfQuery(string query)
        {
            bool nonQueryCondition = query.ToLower().Contains("insert") || query.ToLower().Contains("update") || query.ToLower().Contains("delete") || query.ToLower().Contains("create") || query.ToLower().Contains("drop") || query.ToLower().Contains("alter");
            bool selectQueryCondition = query.ToLower().Contains("select") || query.ToLower().Contains("exec");
            if (query == "" || ((!nonQueryCondition) && (!selectQueryCondition)))
            {
                throw new Exception("UserEnterNullOrAnythingValueQueryErrorMessage");
            }
            else if (nonQueryCondition)
            {
                //Call Insert/Update/Delete method
                return true;
            }
            else if (selectQueryCondition)
            {
                //Call Select Query method
                return false;
            }
            else
            {
                throw new Exception();
            }
        }
        #endregion

        #region Select Query
        /// <summary>
        /// Select Query 
        /// This Will Perform For Select Any Data Or Execute Store Procedure
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public DataSet SelectQueryResult(string query, string dbConnectionString)
        {
            ADODbContext newAdoDb = new ADODbContext(dbConnectionString);
            SqlQueryVM result = new SqlQueryVM();
            //This Will Return DataSet
            result.ExecuteSelectQueryDataSet = newAdoDb.Select(query, null, CommandType.Text);
            return result.ExecuteSelectQueryDataSet;
        }
        #endregion

        #region Execute Store Query
        /// <summary>
        /// This Method Can Be Perform For Query Like Insert/Update/Delete/Create Store Procedure
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public int ExecuteQuery(string query, string dbConnectionString)
        {
            ADODbContext newAdoDb = new ADODbContext(dbConnectionString);
            SqlQueryVM result = new SqlQueryVM();
            //This Will Return Int Value
            result.ExecuteQueryResult = newAdoDb.ExecuteNonQuery(query);
            //result.ExecuteQueryResult = db.Database.ExecuteSqlCommand(query);
            return result.ExecuteQueryResult;
        }

        /// <summary>
        /// Test Multiple Email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool SendEmailProcess(string email)
        {
            //Separate Multiple Email
            List<string> saperateEmail = email.Split(',').ToList();
            try
            {
                //Send Email
                for (int i = 0; i < saperateEmail.Count; i++)
                {
                    Dictionary<string, string> replacevalues = new Dictionary<string, string>();
                    replacevalues.Add("{siteURL}", Config.SiteURL + "setup/test-email");
                    EmailUtility.Send(saperateEmail[i], "Test Email", AppSettings.FromEmail, EmailUtility.GetTemplate(TemplateType.TestEmail), replacevalues);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        ///// <summary>
        ///// Encrypt Existing Password.
        ///// </summary>
        //public void EncryptPassword()
        //{
        //    var userList = (from user in db.Users select user).ToList();
        //    foreach (var users in userList)
        //    {
        //        if (!string.IsNullOrEmpty(users.PasswordHash))
        //        {
        //            users.PasswordSalt = PasswordGenerator.GetSalt();
        //            users.PasswordHash = PasswordGenerator.GetHashedPassword(users.PasswordSalt, users.PasswordHash);
        //            db.SaveChanges();
        //        }
        //    }
        //}
    }
}
