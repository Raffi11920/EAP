using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage
{
    public class BridgeMessageEnum
    {
        public enum MessageType
        {
            Request,
            Reply,
            Notify,
            Initialization
        }

        public enum MessageDirection
        {
            Incoming,
            Outgoing
        }

        public enum DataType
        {
            CustomClass,
            Dictionary,
            List,
        }
    }
}
