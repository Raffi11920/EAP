using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Utilities.LogUtilities
{
    public sealed class LogFactory
    {
        public static LogHelper GetLogHelper(string loggerName)
        {
            var logHelper = new LogHelper();
            logHelper.InitializeLogger(loggerName);

            return logHelper;
        }

        public static LogHelper GetLogHelperByPath(string appenderName, string loggerName, string logPath)
        {
            var logHelper = new LogHelper();
            logHelper.InitializeLogger(appenderName, loggerName, logPath);

            return logHelper;
        }
    }
}
