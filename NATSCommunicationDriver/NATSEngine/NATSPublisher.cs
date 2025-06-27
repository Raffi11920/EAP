using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.NATSCommunicationDriver.NATSEngine
{
    using NATS.Client;
    using Generic;

    public class NATSPublisher : NATSBase
    {
        public NATSPublisher(string url, string subject, Helper helper, Logger logger) : base(url, subject, helper, logger)
        {
        }

        public void Publish(BaseMessage message)
        {
            // ***TBD - Log message
            // *** If Enable false - Return
            if (!Helper.Configuration.Enable)
            {
                Logger.LogHelper.LogInfo("NATS connection is disabled. Please check NATS config file.");
                return;
            }

            message.TransactionDate = DateTime.Now;
            var payload = Common.ObjectToByteArray(message);

            int retryCount = 5;

            while (retryCount > 0)
            {
                try
                {
                    Connection.Publish(Subject, payload);
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
    }
}
