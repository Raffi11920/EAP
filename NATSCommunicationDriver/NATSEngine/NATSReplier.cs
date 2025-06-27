using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.NATSCommunicationDriver.NATSEngine
{
    using NATS.Client;
    using Generic;
    using EventManager.EventArgument;

    internal class NATSReplier : NATSBase
    {
        #region Public Field

        public delegate void MessageEventHandler(object sender, NATSMessageEventArgs e);
        public event MessageEventHandler OnRequestReceived;

        #endregion

        #region Properties

        #endregion

        #region Constructor

        public NATSReplier(string url, string subject, Helper helper, Logger logger) : base(url, subject, helper, logger)
        {
            EventHandler<MsgHandlerEventArgs> msgHandler = (sender, args) =>
            {
                var message = (IMessage)Common.ByteArrayToObject(args.Message.Data);
                OnRequestReceived(this, new NATSMessageEventArgs(message, args.Message, args.Message.Reply));
            };

            Connection.SubscribeAsync(Subject, msgHandler);
        }

        #endregion

        #region Public Method

        public void Reply(string replySubject, BaseMessage replyMessage, Msg requestMessage)
        {
            replyMessage.TransactionDate = DateTime.Now;
            var payload = Common.ObjectToByteArray(replyMessage);
            requestMessage.Subject = replySubject;
            requestMessage.Data = payload;

            int retryCount = 5;

            while (retryCount > 0)
            {
                try
                {
                    Connection.Publish(requestMessage);
                    break;
                }
                catch
                {
                    Logger.LogHelper.LogInfo("NATS connection issue, retrying...");
                    System.Threading.Thread.Sleep(1000);
                    retryCount--;
                    continue;
                }
            }
        }

        #endregion

    }
}
