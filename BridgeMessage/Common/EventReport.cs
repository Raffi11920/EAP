using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class EventReport
    {
        public int EventID { get; set; }

        public ReportItem[] Reports { get; set; }

        public class ReportItem
        {
            public int ReportID { get; set; }

            public object[] SV { get; set; }
        }
    }
}
