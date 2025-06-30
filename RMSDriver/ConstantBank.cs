using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.RMSDriver
{
    public sealed class ConstantBank
    {
        public struct FilePath
        {
            public const string CONFIGDIR = "EQConnection";
            public const string CONFIGFILE = "RMS.Config.xml";
            public const string PARAMSFILE = "RMS.Params.xml";
            public const string EXTERNALEXE = @"\ExternalExecutable";
        }

    }
}
