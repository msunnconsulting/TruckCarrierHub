namespace Common.Utility
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public class DataToastException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public Toast[] DataErrors;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errors"></param>
        public DataToastException(params Toast[] errors)
        {
            if (errors != null)
                this.DataErrors = errors;
            else
                this.DataErrors = new Toast[1];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public DataToastException(string message)
        {
            this.DataErrors = new Toast[1];
            this.DataErrors[0] = new Toast(message, ToastType.Error);
        }
    }
}
