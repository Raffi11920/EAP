using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Qynix.EAP.Drivers.EquipmentCentralDriver
{
    using Base.XMLPlayer;

    [Serializable()]
    [XmlRoot("EquipmentCentralConfiguration", Namespace = "", IsNullable = false)]
    public sealed class EquipmentCentralConfig : XMLBase<EquipmentCentralConfig>
    {
        CEquipments mEquipments;

        public EquipmentCentralConfig()
        {

        }

        public EquipmentCentralConfig(string xmlPath) : base(xmlPath)
        {

        }

        [XmlElement(ElementName = "Logger")]
        public string Logger { get; set; }

        [XmlElement(ElementName = "FWEquipmentName")]
        public string FWEquipmentName { get; set; }

        [XmlElement(ElementName = "Equipments")]
        public CEquipments Equipments
        {
            get { return mEquipments; }
            set { mEquipments = value; }
        }

        [Serializable()]
        public class CEquipments
        {
            [XmlElement(ElementName = "EquipmentDriver")]
            public CEquipmentDriver EquipmentDriver { get; set; }

            [XmlElement(ElementName = "EquipmentCount")]
            public string EquipmentCount { get; set; }

            [XmlElement(ElementName = "EquipmentList")]
            public CEquipmentList EquipmentList { get; set; }

            [Serializable()]
            public class CEquipmentDriver
            {
                [XmlElement(ElementName = "Name")]
                public string Name { get; set; }

                [XmlElement(ElementName = "DLL")]
                public string DLL { get; set; }

                [XmlElement(ElementName = "Description")]
                public string Description { get; set; }
            }

            [Serializable()]
            public class CEquipmentList
            {
                [XmlElement(ElementName = "Equipment")]
                public string[] Equipment { get; set; }
            }
        }

    }
}
