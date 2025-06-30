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
    [XmlRoot("RMSConfiguration", Namespace = "", IsNullable = false)]
    public sealed class RMSConfig : XMLBase<RMSConfig>
    {
        public RMSConfig()
        {

        }

        public RMSConfig(string xmlPath) : base(xmlPath)
        {

        }

        [XmlElement("MapPath")]
        public MapPathSection MapPath { get; set; }

        [XmlElement("RecipeUploadSetting")]
        public RecipeUploadSettingSection RecipeUploadSetting { get; set; }

        public class MapPathSection
        {
            [XmlElement("Path")]
            public PathSection[] Path { get; set; }

            public class PathSection
            {
                [XmlAttribute("Username")]
                public string Username { get; set; }

                [XmlAttribute("Password")]
                public string Password { get; set; }

                [XmlAttribute("PathName")]
                public string PathName { get; set; }

                [XmlAttribute("PathValue")]
                public string PathValue { get; set; }

                [XmlAttribute("MapPath")]
                public string MapPath { get; set; }

                [XmlAttribute("Filter")]
                public string Filter { get; set; }
            }
        }

        public class RecipeUploadSettingSection
        {
            [XmlElement("UploadPath")]
            public string UploadPath { get; set; }
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
