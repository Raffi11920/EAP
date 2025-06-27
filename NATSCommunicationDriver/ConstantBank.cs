using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.NATSCommunicationDriver
{
    internal sealed class ConstantBank
    {
        public struct FilePath
        {
            public const string CONFIGDIR = "NATS";
            public const string CONFIGFILE = "NATS.Client.Config.xml";
        }
    }
}
