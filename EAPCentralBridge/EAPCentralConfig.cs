using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Qynix.EAP.Drivers.EAPCentral
{
    using Base.XMLPlayer;

    [Serializable()]
    [XmlRoot("EAPCentralConfiguration", Namespace = "", IsNullable = false)]
    public sealed class EAPCentralConfig : XMLBase<EAPCentralConfig>
    {
        public EAPCentralConfig()
        {

        }

        public EAPCentralConfig(string xmlPath) : base(xmlPath)
        {
            
        }

        [XmlElement("Logger")]
        public string Logger { get; set; }

        [XmlElement("EAPDriverList")]
        public CEAPDriverList EAPDriverList { get; set; }

        public class CEAPDriverList
        {
            [XmlElement("EAPDriver")]
            public CEAPDriver[] EAPDriver { get; set; }
        }

        public class CEAPDriver
        {
            [XmlElement("Name")]
            public string Name { get; set; }

            [XmlElement("DLL")]
            public string DLL { get; set; }

            [XmlElement("Description")]
            public string Description { get; set; }
        }
    }
}
