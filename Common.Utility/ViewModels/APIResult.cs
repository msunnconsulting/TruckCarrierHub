namespace Common.Utility.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An object which will be normally sent from API when we want to send back result as JSON
    /// </summary>
    public class APIResult<T>
    {
        /// <summary>
        /// Used to inform client side that everything is completed successfully.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Messages to be shown automatically by our javascript ajax handlers
        /// </summary>
        public List<Toast> AutoShowMessages { get; set; }

        /// <summary>
        /// Messages not to be shown automatically by our javascript ajax handlers, and should be handled manually by programmer
        /// </summary>
        public List<Toast> ManualShowMessages { get; set; }

        /// <summary>
        /// Response object, it can be any result, renderedview html, another object, string, int etc.
        /// </summary>
        public T Response { get; set; }

        /// <summary>
        /// Creates new instance of APIResult
        /// </summary>        
        /// <param name="data"></param>
        /// <param name="success">indicates if operation is successful or not</param>
        /// <param name="continueOnFailure"></param>
        /// <param name="messages">messages to be sent along with the result</param>
        public APIResult(T data, bool success, params Toast[] messages)
        {
            this.Response = data;
            this.IsSuccess = success;
            if (messages != null)
            {
                this.AutoShowMessages = messages.ToList().Where(m => m.Show == true).ToList();
                this.ManualShowMessages = messages.ToList().Where(m => m.Show == false).ToList();
            }
            else
            {
                this.AutoShowMessages = new List<Toast>();
                this.ManualShowMessages = new List<Toast>();
            }
        }

        /// <summary>
        /// Creates new instance of APIResult
        /// </summary>        
        /// <param name="data"></param>        
        public APIResult(T data)
        {
            this.Response = data;
            this.IsSuccess = true;
            this.AutoShowMessages = new List<Toast>();
            this.ManualShowMessages = new List<Toast>();
        }

        /// <summary>
        /// Creates new instance of APIResult. Developer has to set all the fields as per need. Default values : Response - default value of T, IsSuccess = false, ContinueOnFailure = false, AutoShowMessages=blank list of toast, ManualShowMessages = blank list of toast
        /// </summary>        
        public APIResult()
        {
            this.IsSuccess = false;
            this.AutoShowMessages = new List<Toast>();
            this.ManualShowMessages = new List<Toast>();
        }
    }

    ///// <summary>
    ///// An object which will be normally sent from API when we want to send back result as JSON
    ///// </summary>
    //public class APIResult
    //{
    //    /// <summary>
    //    /// Used to inform client side that everything is completed successfully.
    //    /// </summary>
    //    public bool IsSuccess { get; set; }

    //    /// <summary>
    //    /// Messages to be shown automatically by our javascript ajax handlers
    //    /// </summary>
    //    public List<Toast> AutoShowMessages { get; set; }

    //    /// <summary>
    //    /// Messages not to be shown automatically by our javascript ajax handlers, and should be handled manually by programmer
    //    /// </summary>
    //    public List<Toast> ManualShowMessages { get; set; }

    //    /// <summary>
    //    /// Response object, it can be any result, renderedview html, another object, string, int etc.
    //    /// </summary>
    //    public object Response { get; set; }

    //    /// <summary>
    //    /// NEED TO ADD COMMENT FOR THIS
    //    /// </summary>
    //    public bool ContinueOnFailure { get; set; }

    //    /// <summary>
    //    /// Default value for ContinueOnFailure is false and IsSuccess is true
    //    /// </summary>
    //    public APIResult()
    //    {
    //        ContinueOnFailure = false;
    //        IsSuccess = true;
    //    }

    //    /// <summary>
    //    /// Creates new instance of JsonAPIResult
    //    /// </summary>
    //    /// <param name="data">data to be sent as Json</param>
    //    /// <param name="success">indicates if operation is successful or not</param>
    //    /// <param name="continueOnFailure"></param>
    //    /// <param name="messages">messages to be sent along with the result</param>
    //    public APIResult(object data = null, bool success = true, bool continueOnFailure = false, params Toast[] messages)
    //    {
    //        this.Response = data;
    //        this.IsSuccess = success;
    //        this.ContinueOnFailure = continueOnFailure;
    //        this.AutoShowMessages = messages.ToList().Where(m => m.Show == true).ToList();
    //        this.ManualShowMessages = messages.ToList().Where(m => m.Show == false).ToList();
    //    }
    //}
}