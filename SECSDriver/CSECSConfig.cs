using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Qynix.EAP.Drivers.SECSDriver
{
    using Base.XMLPlayer;

    [Serializable()]
    [XmlRoot("SECSConfiguration", Namespace = "", IsNullable = false)]
    public sealed class CSECSConfig : XMLBase<CSECSConfig>
    {
        public CSECSConfig()
        {

        }

        public CSECSConfig(string xmlPath) : base(xmlPath)
        {

        }

        [XmlElement(ElementName = "HostSettings")]
        public HostSettingsSection HostSettings { get; set; }

        [XmlElement(ElementName = "EquipmentSettings")]
        public EquipmentSettingsSection EquipmentSettings { get; set; }

        [XmlElement(ElementName = "SECSProConfiguration")]
        public SECSProConfigurationSection SECSProConfiguration { get; set; }

        [XmlElement(ElementName = "SECSGEMActions")]
        public SECSGEMActionsSection SECSGEMActions { get; set; }

        [XmlElement(ElementName = "SECSAutoReply")]
        public SECSAutoReplySection SECSAutoReply { get; set; }

        [XmlElement(ElementName = "SECSDefinition")]
        public SECSDefinitionSection SECSDefinition { get; set; }

        [XmlElement(ElementName = "SECSItemFormats")]
        public SECSItemFormatsSection SECSItemFormats { get; set; }

        [XmlElement(ElementName = "SVIDMapping")]
        public SVIDMappingSection SVIDMapping { get; set; }

        [XmlElement(ElementName = "HCACKMapping")]
        public HCACKMappingSection HCACKMapping { get; set; }

        public class HostSettingsSection
        {
            [XmlElement(ElementName = "EstablishCommTimer")]
            public int EstablishCommTimer { get; set; }

            [XmlElement(ElementName = "PPDir")]
            public string PPDir { get; set; }

            [XmlElement(ElementName = "PPExt")]
            public string PPExt { get; set; }
        }

        public class EquipmentSettingsSection
        {
            [XmlElement(ElementName = "PPDirs")]
            public PPDirsSection PPDirs { get; set; }

            [XmlElement(ElementName ="PPExt")]
            public string PPExt { get; set; }

            public class PPDirsSection
            {
                [XmlElement("PPDir")]
                public string[] PPDir { get; set; }
            }
        }

        public class SECSProConfigurationSection
        {
            [XmlElement(ElementName = "DefaultSECSLibrary")]
            public string DefaultSECSLibrary { get; set; }

            [XmlElement(ElementName = "LogFileName")]
            public string LogFileName { get; set; }

            [XmlElement(ElementName = "LogFileDir")]
            public string LogFileDir { get; set; }

            [XmlElement(ElementName = "LogFileTraceLevel")]
            public string LogFileTraceLevel { get; set; }

            [XmlElement(ElementName = "LogFileMaxDays")]
            public int LogFileMaxDays { get; set; }
        }

        public class SECSGEMActionsSection
        {
            [XmlElement("Action")]
            public ActionItem[] Action { get; set; }

            public class ActionItem
            {
                [XmlAttribute("Name")]
                public string Name { get; set; }

                [XmlAttribute("Type")]
                public string Type { get; set; }

                [XmlElement("SECSFunction")]
                public SECSFunctionItem[] SECSFunction { get; set; }

                public class SECSFunctionItem
                {
                    [XmlAttribute("Name")]
                    public string Name { get; set; }
                    
                    [XmlAttribute("Order")]
                    public int Order { get; set; }

                    [XmlAttribute("WaitForReply")]
                    public bool WaitForReply { get; set; }

                    [XmlAttribute("ExpectedAcknowledge")]
                    public string ExpectedAcknowledge { get; set; }

                    [XmlAttribute("UseDefaultTransaction")]
                    public bool UseDefaultTransaction { get; set; }
                }
            }
        }

        public class SECSAutoReplySection
        {
            [XmlElement("SECSReplyFunction")]
            public SECSReplyFunctionSection[] SECSFunction { get; set; }

            public class SECSReplyFunctionSection
            {
                [XmlAttribute("Name")]
                public string Name { get; set; }

                [XmlAttribute("SxFx")]
                public string SxFx { get; set; }

                [XmlAttribute("UseDefaultReply")]
                public bool UserDefaultReply { get; set; }
            }
        }
        
        public class SECSDefinitionSection
        {
            [XmlElement(ElementName = "SECSTransactionDir")]
            public string SECSTransactionDir { get; set; }

            [XmlElement(ElementName = "SECSFunction")]
            public SECSFunctionSection[] SECSFunction { get; set; }

            public class SECSFunctionSection
            {
                [XmlElement(ElementName = "Name")]
                public string Name { get; set; }

                [XmlElement(ElementName = "SxFx")]
                public string SxFx { get; set; }
            }
        }

        public class SECSItemFormatsSection
        {
            [XmlElement("SECSItemFormat")]
            public SECSItemFormatSection[] SECSItemFormat { get; set; }

            public class SECSItemFormatSection
            {
                [XmlAttribute("Name")]
                public string Name { get; set; }

                [XmlAttribute("Value")]
                public string Value { get; set; }
            }
        }

        public class SVIDMappingSection
        {
            [XmlElement("SV")]
            public SVSection[] SV { get; set; }

            public class SVSection
            {
                [XmlAttribute("Name")]
                public string Name { get; set; }

                [XmlAttribute("VID")]
                public int VID { get; set; }

                [XmlElement("ValueMap")]
                public ValueMapSection[] ValueMap { get; set; }

                public class ValueMapSection
                {
                    [XmlAttribute("Value")]
                    public string Value { get; set; }
                    
                    [XmlAttribute("Text")]
                    public string Text { get; set; }
                }
            }
        }
        
        public class HCACKMappingSection
        {
            [XmlElement("HCACK")]
            public HCACKSection[] HCACK { get; set; }
            
            public class HCACKSection
            {
                [XmlAttribute("Value")]
                public int Value { get; set; }

                [XmlAttribute("Text")]
                public string Text { get; set; }
            }
        }        
    }
}
