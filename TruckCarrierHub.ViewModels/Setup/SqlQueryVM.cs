namespace PartnerCarrier.ViewModels.Setup
{
    using System.Data;

    public class SqlQueryVM
    {
        public DataSet ExecuteSelectQueryDataSet { get; set; }
        public int ExecuteQueryResult { get; set; }
        public int StoreProcedureResult { get; set; }
    }
}
