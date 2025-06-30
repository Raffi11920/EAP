using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SOAPRequestDriver.Objects
{
    public sealed class WaferInfo
    {
        public string FWEquipmentID { get; set; }
        public string EquipmentID { get; set; }
        public string LotID { get; set; }
        public string StripID { get; set; }
        public string WaferID { get; set; }
        public int Strip_X { get; set; }
        public int Strip_Y { get; set; }
        public int Wafer_X { get; set; }
        public int Wafer_Y { get; set; }
        public int NotchLoc { get; set; }


    }
}
