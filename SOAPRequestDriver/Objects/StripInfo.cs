using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SOAPRequestDriver.Objects
{
    public sealed class StripInfo
    {
        public string FWEquipmentID { get; set; }
        public string EquipmentID { get; set; }
        public string LotID { get; set; }
        public string StripID { get; set; }
        public string Package { get; set; }
        public string Operator { get; set; }
        public int UnitRow { get; set; }
        public int UnitColumn { get; set; }
        public int ClusterRow { get; set; }
        public int ClusterColumn { get; set; }
        public string MapFile { get; set; }
        public string MarkRow { get; set; }
        public string MarkColumn { get; set; }
        public string SortRow { get; set; }
        public string SortColumn { get; set; }
        public int OriginLocation { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public string[] DefectCode { get; set; }
        public string Location { get; set; }
    }
}
