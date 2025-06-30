using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SECSDriver.EventManager.EventArguments
{
    using Base.BridgeMessage;

    internal class SECSGEMActionEventArgs : EventArgs, IErrorInfo
    {
        private bool mReplyRequired;
        private int mErrorCode;
        private string mErrorText;
        private string mMessageID;
        private string mActionName;

        public bool ReplyRequired
        {
            get { return mReplyRequired; }
        }

        public string MessageID
        {
            get { return mMessageID; }
        }

        public string ActionName
        {
            get { return mActionName; }
        }

        public int ErrorCode
        {
            get
            {
                return mErrorCode;
            }
            set
            {
                mErrorCode = value;
            }
        }

        public string ErrorText
        {
            get
            {
                return mErrorText;
            }
            set
            {
                mErrorText = value;
            }
        }

        public SECSGEMActionEventArgs(string actionName, bool replyRequired, string messageId, int errorCode, string errorText)
        {
            mActionName = actionName;
            mReplyRequired = replyRequired;
            mMessageID = messageId;
            mErrorCode = errorCode;
            mErrorText = errorText;
        }
    }
}
