using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.RMSDriver
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

        public void Initialize(string equipmentName, string logPath)
        {
            mLogHelper = LogFactory.GetLogHelperByPath("RMS_{0}_Appender".FillArguments(equipmentName), "RMS_{0}".FillArguments(equipmentName), logPath);
        }
    }
}
