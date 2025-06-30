using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.RMSDriver
{
    using Base.XMLPlayer;
    using System.Xml.Serialization;

    [Serializable()]
    [XmlRoot("RMSParameters", Namespace = "", IsNullable = false)]
    public sealed class RMSParameters : XMLBase<RMSParameters>
    {
        public RMSParameters()
        {

        }

        public RMSParameters(string xmlPath) : base(xmlPath)
        {

        }

        [XmlElement("Parameters")]
        public ParametersSection Parameters { get; set; }

        public class ParametersSection
        {
            [XmlElement("Param")]
            public ParamSection[] Param { get; set; }

            public class ParamSection
            {
                [XmlAttribute("Name")]
                public string Name { get; set; }

                [XmlAttribute("DataType")]
                public string DataType { get; set; }

                [XmlAttribute("Suffix")]
                public string Suffix { get; set; }

                [XmlAttribute("Operator")]
                public string Operator { get; set; }
            }
        }

    }
}
