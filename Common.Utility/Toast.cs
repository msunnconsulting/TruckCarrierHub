namespace Common.Utility
{
    using EnumUtil;
    using System;
    using System.Web.Script.Serialization;

    /// <summary>
    /// Types of the Toast
    /// </summary>
    public enum ToastType
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
    /// Position of the Toas on screen
    /// </summary>
    public enum ToastPosition
    {
        /// <summary>
        /// 
        /// </summary>
        [EnumName("toast-top-right")]
        TopRight,

        /// <summary>
        /// 
        /// </summary>
        [EnumName("toast-bottom-right")]
        BottomRight,

        /// <summary>
        /// 
        /// </summary>
        [EnumName("toast-bottom-left")]
        BottomLeft,

        /// <summary>
        /// 
        /// </summary>
        [EnumName("toast-top-left")]
        TopLeft,

        /// <summary>
        /// /
        /// </summary>
        [EnumName("toast-top-full-width")]
        TopFullWidth,

        /// <summary>
        /// 
        /// </summary>
        [EnumName("toast-bottom-full-width")]
        BottomFullWidth,
    }

    /// <summary>
    /// Toast object that has all necesary information to generate Toast using toastr js library
    /// </summary>
    public class Toast
    {
        /// <summary>
        /// Title for the Toast
        /// </summary>
        public string Title;

        /// <summary>
        /// Message to be shown
        /// </summary>
        public string Message;

        private ToastType _Type;

        /// <summary>
        /// Type of the toast
        /// </summary>
        [ScriptIgnore]
        public ToastType Type { get { return _Type; } set { _Type = value; } }

        /// <summary>
        /// Type of the toast in the form of string
        /// </summary>
        public string TypeName { get { return this._Type.ToName(); } }

        private ToastPosition _Position;

        /// <summary>
        /// Position of the toast on screen
        /// </summary>
        [ScriptIgnore]
        public ToastPosition Position { get { return _Position; } set { _Position = value; } }

        /// <summary>
        /// Position of the toast on screen in the form of string
        /// </summary>
        public string PositionName { get { return this._Position.ToName(); } }

        /// <summary>
        /// Id of element for which this message is being sent 
        /// </summary>
        public string MessageFor;

        /// <summary>
        /// indicates if close button should be shown on toast or not
        /// </summary>
        public bool ShowClose;

        /// <summary>
        /// Indicates toastr for this Toast will be shown automatically from global ajax handler
        /// </summary>
        [ScriptIgnore]
        public bool Show;

        /// <summary>
        /// creates Toast object, which can be used to send messages from server to client side and can be shown using toastr library very easily.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public Toast(string message, ToastType type)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException("message");

            this.Message = message;
            this.Type = type;
            this.Position = ToastPosition.BottomRight;
            this.ShowClose = true;
            this.Show = true;
        }
    }

}