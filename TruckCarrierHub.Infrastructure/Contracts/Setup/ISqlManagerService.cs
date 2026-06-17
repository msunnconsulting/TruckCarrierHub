

namespace PartnerCarrier.Infrastructure.Contracts.Setup
{
    using System.Data;

    public interface ISqlManagerService
    {
        /// <summary>
        /// Check Which type of Query User Entered 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        bool checkTypeOfQuery(string query);

        /// <summary>
        /// Execute Select query method
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        DataSet SelectQueryResult(string query, string dbConnectionString);


        /// <summary>
        ///Execute DDL Command like Insert/Update/Delete
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int ExecuteQuery(string query, string dbConnectionString);

        bool SendEmailProcess(string email);

        ///// <summary>
        ///// Encrypt Existing Password.
        ///// </summary>
        //void EncryptPassword();


    }
}
