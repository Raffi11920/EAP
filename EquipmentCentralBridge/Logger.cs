using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.EquipmentCentralDriver
{
    using Utilities.LogUtilities;

    internal sealed class Logger
    {
        private const string LOGGERNAME = "EQCentralLog";
        private static LogHelper mLogHelper = null;

        public static LogHelper LogHelper
        {
            get
            {
                return mLogHelper;
            }
        }

        public static void Initialize()
        {
            mLogHelper = LogFactory.GetLogHelper(LOGGERNAME);
        }

        public static void Initialize(string loggerName)
        {
            mLogHelper = LogFactory.GetLogHelper(loggerName);
        }
    }
}
