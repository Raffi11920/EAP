using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SOAPRequestDriver
{
    public sealed class ConstantBank
    {
        public struct FilePath
        {
            public const string CONFIGDIR = "EQConnection";
            public const string CONFIGFILE = "SOAPRequestConfig.xml";
        }

        public enum DefectCodePrefixType
        {
            Add,
            Concat
        }

        public enum JobState
        {
            New,
            Queued,
            Selected,
            WaitingForStart,
            InProcess,
            Processed,
            Stopped,
            Aborted,
            Destroyed,
            Resume
        }
    }
}
