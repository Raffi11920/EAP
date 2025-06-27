using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.NATSCommunicationDriver.EventManager.EventArgument
{
    using Generic;
    using NATS.Client;

    public class NATSMessageEventArgs : EventArgs
    {
        private IMessage mMessage;
        private Msg mRawMessage;
        private string mReplySubject;

        public IMessage Message
        {
            get { return mMessage; }
        }

        public Msg RawMessage
        {
            get { return mRawMessage; }
        }

        public string ReplySubject
        {
            get { return mReplySubject; }
        }

        public NATSMessageEventArgs(IMessage message, Msg rawMessage, string replySubject)
        {
            mMessage = message;
            mRawMessage = rawMessage;
            mReplySubject = replySubject;
        }
    }
}
