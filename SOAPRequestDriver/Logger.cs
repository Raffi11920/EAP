using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SOAPRequestDriver
{
    using Utilities.LogUtilities;

    public sealed class Logger
    {
        private const string LOGGERNAME = "SOAPRequestLog";
        private LogHelper mLogHelper = null;

        public LogHelper LogHelper
        {
            get
            {
                return mLogHelper;
            }
        }

        public void Initialize()
        {
            mLogHelper = LogFactory.GetLogHelper(LOGGERNAME);
        }

        public void Initialize(string loggerName)
        {
            mLogHelper = LogFactory.GetLogHelper(loggerName);
        }
    }
}
