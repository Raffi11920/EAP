using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.NATSCommunicationDriver
{
    using Utilities.ExtensionPlug;
    using Utilities.LogUtilities;

    public sealed class Logger
    {
        private LogHelper mLogHelper = null;

        public LogHelper LogHelper
        {
            get { return mLogHelper; }
        }

        public void Initialize(string loggerName)
        {
            mLogHelper = LogFactory.GetLogHelper(loggerName);
        }
    }
}
