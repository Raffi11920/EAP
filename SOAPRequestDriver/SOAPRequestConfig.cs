using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Qynix.EAP.Drivers.SOAPRequestDriver
{
    using Base.XMLPlayer;

    [Serializable()]
    [XmlRoot("SOAPRequestConfiguration", Namespace = "", IsNullable = false)]
    public sealed class SOAPRequestConfig : XMLBase<SOAPRequestConfig>
    {
        public SOAPRequestConfig()
        {

        }

        public SOAPRequestConfig(string path) : base(path)
        {

        }

        [XmlElement(ElementName = "Logger")]
        public string Logger { get; set; }

        [XmlElement(ElementName = "EMAPURL")]
        public string EMAPURL { get; set; }

        [XmlElement(ElementName = "WebServices")]
        public CWebServices WebServices { get; set; }

        [XmlElement(ElementName = "Setting")]
        public CSetting Setting { get; set; }

        [Serializable()]
        public class CSetting
        {
            [XmlElement(ElementName = "RequestTimeout")]
            public int RequestTimeout { get; set; }

            [XmlElement(ElementName = "RetryCount")]
            public int RetryCount { get; set; }

            [XmlElement(ElementName = "DefectCodePrefix")]
            public string DefectCodePrefix { get; set; }

            [XmlElement(ElementName = "DefectCodePrefixType")]
            public string DefectCodePrefixType { get; set; }

            [XmlElement(ElementName = "DefectCodeMapping")]
            public CDefectCodeMapping[] DefectCodeMapping { get; set; }

            [Serializable()]
            public class CDefectCodeMapping
            {
                [XmlElement(ElementName = "DefectCodeMachine")]
                public string DefectCodeMachine { get; set; }

                [XmlElement(ElementName = "DefectCodeMapped")]
                public string DefectCodeMapped { get; set; }
            }
            
            [XmlElement("SharedDrivePath")]
            public PathSection[] SharedDrivePath { get; set; }

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

                [XmlAttribute("UploadPath")]
                public string UploadPath { get; set; }

                [XmlAttribute("Filter")]
                public string Filter { get; set; }

            }

            [XmlElement(ElementName = "Path")]
            public string Path { get; set; }

            [XmlElement(ElementName = "EMapsUpdateTimeout")]
            public int EMapsUpdateTimeout { get; set; }

            [XmlElement(ElementName = "StripUpdateCheckJobState")]
            public bool StripUpdateCheckJobState { get; set; }
        }

        [Serializable()]
        public class CWebServices
        {
            [XmlElement(ElementName = "WebServicesDir")]
            public string WebServicesDir { get; set; }

            [XmlElement(ElementName = "WebService")]
            public WebServiceItem[] WebService { get; set; }

            [Serializable()]
            public class WebServiceItem
            {
                [XmlElement(ElementName = "ServiceName")]
                public string ServiceName { get; set; }

                [XmlElement(ElementName = "XMLFILE")]
                public string XMLFILE { get; set; }
            }
        }
    }
}
