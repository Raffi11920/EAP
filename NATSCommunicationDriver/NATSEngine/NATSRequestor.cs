using Generic;
using NATS.Client;
using Qynix.EAP.Drivers.NATSCommunicationDriver.EventManager.EventArgument;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.NATSCommunicationDriver.NATSEngine
{
    class NATSRequestor : NATSBase
    {
        private IConnection mConnection;

        public delegate void MessageEventHandler(object sender, NATSMessageEventArgs e);
        public event MessageEventHandler OnMessageRequested;

        public NATSRequestor(string url, string subject, Helper helper, Logger logger) : base(url, subject, helper, logger)
        {
        }

        public void Request(BaseMessage message)
        {
            if (!this.Helper.Configuration.Enable)
            {
                this.Logger.LogHelper.LogInfo("NATS connection is disabled. Please check NATS config file.", "Request", "C:\\EAP\\NATSCommunicationDriver\\NATSEngine\\NATSRequestor.cs");
            }
            else
            {
                message.TransactionDate = DateTime.Now;
                Msg rawMessage = Connection.Request(Subject, Common.ObjectToByteArray(message));
                // ISSUE: reference to a compiler-generated field
                OnMessageRequested(this, new NATSMessageEventArgs(message, rawMessage, Subject));
            }
        }
    }
}
