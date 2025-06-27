using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace Qynix.EAP.Drivers.NATSCommunicationDriver
{
    using EAP.Base.XMLPlayer;
    [Serializable()]
    [XmlRoot("NATSConfiguration")]
    public class NATSClientConfig : XMLBase<NATSClientConfig>
    {
        [XmlElement("Logger")]
        public string Logger { get; set; }

        [XmlElement("Enable")]
        public bool Enable { get; set; }

        [XmlElement("NATSConnectionSettings")]
        public NATSConnectionSettingsSection NATSConnectionSettings { get; set; }

        [XmlElement("Subjects")]
        public SubjectsSection Subjects { get; set; }

        public NATSClientConfig()
        {
            
        }

        public NATSClientConfig(string xmlPath) : base(xmlPath)
        {
            
        }

        public class NATSConnectionSettingsSection
        {
            [XmlElement("URL")]
            public string URL { get; set; }
        }

        public class SubjectsSection
        {
            [XmlElement("Subject")]
            public SubjectElement[] Subject { get; set; }

            public class SubjectElement
            {
                [XmlAttribute("Name")]
                public string Name { get; set; }

                [XmlAttribute("Type")]
                public string Type { get; set; }
            }
        }
    }
}
