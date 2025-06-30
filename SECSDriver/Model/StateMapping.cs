using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.SECS.SECSFramework.Model
{
    public class StateMapping
    {
        public ControlStateMapping ControlStateMap { get; set; }
        public ProcessStateMapping ProcessStateMap { get; set; }

        public class ControlStateMapping
        {
            public int ControlStateVID { get; set; }

            public Dictionary<int, string> ValueMap { get; set; }
        }

        public class ProcessStateMapping
        {
            public int ProcessStateVID { get; set; }

            public Dictionary<int, string> ValueMap { get; set; }
        }
    }
}
