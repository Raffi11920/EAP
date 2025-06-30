using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SECSDriver
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

        //public static void Initialize(LogHelper logHelper)
        //{
        //    mLogHelper = logHelper;
        //}

        public void Initialize(string equipmentName, string logPath)
        {
            mLogHelper = LogFactory.GetLogHelperByPath("CSECS_{0}_Appender".FillArguments(equipmentName), "CSECS_{0}".FillArguments(equipmentName), logPath);
        }

    }
}
