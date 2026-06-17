namespace Common.Utility
{
    using System;

    public class BusinessException : Exception
    {
        public ServerMessage[] ErrorMessages;

        public BusinessException(params ServerMessage[] errors)
        {
            this.ErrorMessages = errors;
        }

        public BusinessException(string code, string message, bool autoShow = true)
        {
            this.ErrorMessages = new ServerMessage[] { new ServerMessage(code, message, MessageType.Error, autoShow) };
        }


    }
}
