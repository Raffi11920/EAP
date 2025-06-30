using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SECSDriver
{
    internal sealed class ConstantBank
    {
        public struct FilePath
        {
            public const string CONFIGDIR = "EQConnection";
            public const string CONFIGFILE = "SECSConfig.xml";
        }
    }
}
