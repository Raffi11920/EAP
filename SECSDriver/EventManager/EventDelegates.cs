using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SECSDriver.EventManager
{
    using EventArguments;

    internal class EventDelegates
    {
        public delegate void SECSGEMActionEventHandler(object sender, SECSGEMActionEventArgs e);
    }
}
