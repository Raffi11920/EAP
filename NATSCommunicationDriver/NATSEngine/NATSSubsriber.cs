using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.NATSCommunicationDriver.NATSEngine
{
    using NATS.Client;
    using Generic;
    using EventManager.EventArgument;

    internal class NATSSubscriber : NATSBase
    {
        #region Private Field

        private IConnection mConnection;

        #endregion

        #region Public Field

        public delegate void MessageEventHandler(object sender, NATSMessageEventArgs e);
        public event MessageEventHandler OnMessageReceived;

        #endregion

        #region Constructor

        public NATSSubscriber(string url, string subject, Helper helper, Logger logger) : base(url, subject, helper, logger)
        {
            mConnection = CreateConnection();

            EventHandler<MsgHandlerEventArgs> msgHandler = (sender, args) =>
            {
                var message = (IMessage)Common.ByteArrayToObject(args.Message.Data);
                OnMessageReceived(this, new NATSMessageEventArgs(message, args.Message, args.Message.Reply));
            };

            mConnection.SubscribeAsync(Subject, msgHandler);

        }

        #endregion

    }
}
