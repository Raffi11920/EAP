using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SOAPRequestDriver.EventManager
{
    using EventArguments;

    public class EventDelegates
    {
        public delegate void ServiceEventHandler(object sender, ServiceArgs e);
    }
}
