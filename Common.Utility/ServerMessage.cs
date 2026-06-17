namespace Common.Utility
{
    using EnumUtil;
    using System;
    using System.Web.Script.Serialization;

    /// <summary>
    /// Types of the Toast
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 
        /// </summary>
        [EnumName("success")]
        Scucess = 1,

        /// <summary>
        /// 
        /// </summary>
        [EnumName("info")]
        Info = 2,

        /// <summary>
        /// 
        /// </summary>
        [EnumName("warning")]
        Warning = 3,

        /// <summary>
        /// 
        /// </summary>
        [EnumName("error")]
        Error = 4
    }

    /// <summary>
    /// Toast object that has all necesary information to generate Toast using toastr js library
    /// </summary>
    public class ServerMessage
    {
        public string code;

        /// <summary>
        /// Message to be shown
        /// </summary>
        public string message;

        private MessageType _MessageType;

        /// <summary>
        /// Type of the toast
        /// </summary>
        [ScriptIgnore]
        public MessageType MessageType { get { return _MessageType; } set { _MessageType = value; } }

        /// <summary>
        /// Type of the toast in the form of string
        /// </summary>
        public string type { get { return this._MessageType.ToName(); } }

        /// <summary>
        /// Indicates toastr for this Toast will be shown automatically from global ajax handler
        /// </summary>        
        public bool autoShow;

        /// <summary>
        /// creates Toast object, which can be used to send messages from server to client side and can be shown using toastr library very easily.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
        public ServerMessage(string code, string message, MessageType messageType, bool autoShow = true)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentNullException("code");

            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException("message");
            this.code = code;
            this.message = message;
            MessageType = messageType;
            this.autoShow = autoShow;
        }
    }

}