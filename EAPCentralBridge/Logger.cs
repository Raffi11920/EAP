using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.EAPCentral
{
    using Utilities.LogUtilities;

    public class Logger
    {
        private const string LOGGERNAME = "EAPLog";
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
