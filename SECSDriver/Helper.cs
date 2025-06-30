using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SECSDriver
{
    using Base.BridgeMessage;
    using SECS.SECSFramework.Model;
    using Utilities.ExtensionPlug;
    
    public sealed class Helper
    {
        #region Private Field

        private CSECSConfig mCSECSConfig;
        private IEnumerable<Tuple<string, string, string>> mSECSFunctionFileList;

        #endregion

        #region Properties

        public CSECSConfig Configuration
        {
            get { return mCSECSConfig.SerializedObject; }        
        }

        public IEnumerable<Tuple<string, string, string>> SECSFunctionFileList
        {
            get { return mSECSFunctionFileList; }
        }

        #endregion

        #region Constructor


        #endregion

        #region Public Method

        public bool LoadConfiguration(string configFilePath)
        {
            mCSECSConfig = new CSECSConfig(configFilePath);

            if (mCSECSConfig.Deserialize())
            {
                return true;
            }
            else
            {
                mCSECSConfig = null;
                return false;
            }
        }

        public static ConnectionSettings ExtractConnectionSettings(Dictionary<string, string> settings)
        {
            var connSettings = new ConnectionSettings();
            connSettings.DeviceID = settings["DeviceID"];
            connSettings.PortType = settings["ConnectionPortType"];

            if (connSettings.PortType == "HSMS" || connSettings.PortType == "HSMS_SS")
            {
                connSettings.HSMS = new ConnectionSettings.HSMSSettings();
                var hsms = connSettings.HSMS;

                hsms.ConnectionMode = settings["HSMSConnectionType"];
                hsms.LocalIP = settings["LocalIP"];
                hsms.RemoteIP = settings["RemoteIP"];
                hsms.LocalIPPort = settings["LocalIPPort"];
                hsms.RemoteIPPort = settings["RemoteIPPort"];
                hsms.LinkTestInterval = settings["LinkTestInterval"];
                hsms.ConnectionHost = settings["ConnectionHost"];

                hsms.T3 = settings["T3"];
                hsms.T5 = settings["T5"];
                hsms.T6 = settings["T6"];
                hsms.T7 = settings["T7"];
                hsms.T8 = settings["T8"];
            }
            else if (connSettings.PortType == "SECSI_TCPIP")
            {
                connSettings.TCPIP = new ConnectionSettings.TCPIPSettings();
                var TCPIP = connSettings.TCPIP;

                TCPIP.ConnectionMode = settings["HSMSConnectionType"];
                TCPIP.RemoteIP = settings["RemoteIP"];
                TCPIP.RemoteIPPort = settings["RemoteIPPort"];
                TCPIP.ConnectionRetryLimit  = settings["ConnectionRetryLimit"];
                TCPIP.ConnectionHost = settings["ConnectionHost"];

                TCPIP.T1 = settings["T1"];
                TCPIP.T2 = settings["T2"];
                TCPIP.T3 = settings["T3"];
                TCPIP.T4 = settings["T4"];
            }
            else if (connSettings.PortType == "SECSI_RS232")
            {
                connSettings.RS232 = new ConnectionSettings.RS232Settings();
                var rs232 = connSettings.RS232;

                rs232.SerialPort = settings["SerialPort"];
                rs232.BaudRate = settings["BaudRate"];
                rs232.ConnectionHost = settings["ConnectionHost"];
                rs232.ConnectionRetryLimit = settings["ConnectionRetryLimit"];

                rs232.T1 = settings["T1"];
                rs232.T2 = settings["T2"];
                rs232.T3 = settings["T3"];
                rs232.T4 = settings["T4"];
            }

            return connSettings;
        }

        public ConnectionSettings ExtractConnectionSettings(IBridgeMessage datas)
        {
            var connSettings = new ConnectionSettings();
            connSettings.DeviceID = datas.GetBasicData("DeviceID").Value.ToString();
            connSettings.PortType = datas.GetBasicData("ConnectionPortType").Value.ToString();

            if (connSettings.PortType == "HSMS" || connSettings.PortType == "HSMS_SS")
            {
                connSettings.HSMS = new ConnectionSettings.HSMSSettings();
                var hsms = connSettings.HSMS;

                hsms.ConnectionMode = datas.GetBasicData("HSMSConnectionType").Value.ToString();
                hsms.LocalIP = datas.GetBasicData("LocalIP").Value.ToString();
                hsms.RemoteIP = datas.GetBasicData("RemoteIP").Value.ToString();
                hsms.LocalIPPort = datas.GetBasicData("LocalIPPort").Value.ToString();
                hsms.RemoteIPPort = datas.GetBasicData("RemoteIPPort").Value.ToString();
                hsms.ConnectionHost = datas.GetBasicData("ConnectionHost").Value.ToString();
                hsms.LinkTestInterval = datas.GetBasicData("LinkTestInterval").Value.ToString();

                hsms.T3 = datas.GetBasicData("T3").Value.ToString();
                hsms.T5 = datas.GetBasicData("T5").Value.ToString();
                hsms.T6 = datas.GetBasicData("T6").Value.ToString();
                hsms.T7 = datas.GetBasicData("T7").Value.ToString();
                hsms.T8 = datas.GetBasicData("T8").Value.ToString();
            }
            else if (connSettings.PortType == "SECSI_TCPIP")
            {
                connSettings.TCPIP = new ConnectionSettings.TCPIPSettings();
                var TCPIP = connSettings.TCPIP;

                TCPIP.ConnectionMode = datas.GetBasicData("HSMSConnectionType").Value.ToString();
                TCPIP.RemoteIP = datas.GetBasicData("RemoteIP").Value.ToString();
                TCPIP.RemoteIPPort = datas.GetBasicData("RemoteIPPort").Value.ToString();
                TCPIP.ConnectionHost = datas.GetBasicData("ConnectionHost").Value.ToString();
                TCPIP.ConnectionRetryLimit = datas.GetBasicData("ConnectionRetryLimit").Value.ToString();

                TCPIP.T1 = datas.GetBasicData("T1").Value.ToString();
                TCPIP.T2 = datas.GetBasicData("T2").Value.ToString();
                TCPIP.T3 = datas.GetBasicData("T3").Value.ToString();
                TCPIP.T4 = datas.GetBasicData("T4").Value.ToString();
            }
            else if (connSettings.PortType == "SECSI_RS232")
            {
                connSettings.RS232 = new ConnectionSettings.RS232Settings();
                var rs232 = connSettings.RS232;

                rs232.SerialPort = datas.GetBasicData("SerialPort").Value.ToString();
                rs232.BaudRate = datas.GetBasicData("BaudRate").Value.ToString();
                rs232.ConnectionHost = datas.GetBasicData("ConnectionHost").Value.ToString();
                rs232.ConnectionRetryLimit = datas.GetBasicData("ConnectionRetryLimit").Value.ToString();

                rs232.T1 = datas.GetBasicData("T1").Value.ToString();
                rs232.T2 = datas.GetBasicData("T2").Value.ToString();
                rs232.T3 = datas.GetBasicData("T3").Value.ToString();
                rs232.T4 = datas.GetBasicData("T4").Value.ToString();
            }

            return connSettings;
        }

        public SECSFormatSetting CompileSECSFormatSettingsFromConfig()
        {
            var secsFormatSetting = new SECSFormatSetting();

            secsFormatSetting.DataIDFormat = GetSECSItem("DataIDFormat");
            secsFormatSetting.CEIDFormat = GetSECSItem("CEIDFormat");
            secsFormatSetting.RPTIDFormat = GetSECSItem("RPTIDFormat");
            secsFormatSetting.VIDFormat = GetSECSItem("VIDFormat");
            secsFormatSetting.ECIDFormat = GetSECSItem("ECIDFormat");
            secsFormatSetting.ALIDFormat = GetSECSItem("ALIDFormat");
            secsFormatSetting.DataID = Convert.ToInt32(GetSECSItem("DataID"));

            return secsFormatSetting;
        }

        public HCACKMappingCollection CompileHCACKMappingCollectionFromConfig()
        {
            var hcackMappingCollection = new HCACKMappingCollection();

            foreach (var a in Configuration.HCACKMapping.HCACK)
            {
                hcackMappingCollection.AddHCACKMap(a.Value, a.Text);
            }

            return hcackMappingCollection;
        }

        public StateMapping CompileStateMapFromConfig()
        {
            var stateMap = new StateMapping();

            var controlStateMap = CompileControlState();
            var processStateMap = CompileProcessState();

            stateMap.ControlStateMap = controlStateMap;
            stateMap.ProcessStateMap = processStateMap;

            return stateMap;
        }

        public string GetConfigFilePath(string eapConfigFolder, string equipmentName)
        {
            return Path.Combine(eapConfigFolder, ConstantBank.FilePath.CONFIGDIR, "EQC_{0}".FillArguments(equipmentName), ConstantBank.FilePath.CONFIGFILE);
        }

        public string GetTransactionDirPath(string eapConfigFolder)
        {
            return Path.Combine(eapConfigFolder, Configuration.SECSDefinition.SECSTransactionDir);
        }

        public void CollectSECSFunctionFiles(string secsTransactionDirPath)
        {
            mSECSFunctionFileList = new List<Tuple<string, string, string>>();

            var secsFunctionCollection = from a in Configuration.SECSDefinition.SECSFunction
                          select Tuple.Create(a.Name, a.SxFx, 
                            Path.Combine(
                                secsTransactionDirPath, 
                                "{0}_{1}.xml".FillArguments(a.Name, a.SxFx)));

            mSECSFunctionFileList = secsFunctionCollection;
        }

        public string GetSECSFunctionFilePath(string secsFunctionName)
        {
            var result = mSECSFunctionFileList.First(x => x.Item1.Equals(secsFunctionName));
            return result.Item3;
        }

        public Dictionary<int, string> ConvertToInternalSVList(Dictionary<string, string> svList)
        {
            var internalSVList = new Dictionary<int, string>();

            foreach (var kvp in svList)
            {
                var svText = kvp.Key;

                var svid = (from a in Configuration.SVIDMapping.SV
                                where a.Name == svText
                                select a.VID).First();

                internalSVList.Add(svid, null);
            }

            return internalSVList;
        }

        public Dictionary<string, string> ConvertToExternalSVList(Dictionary<int, object> svList)
        {
            var externalSVList = new Dictionary<string, string>();

            foreach (var kvp in svList)
            {
                var svid = kvp.Key;

                var svText = (from a in Configuration.SVIDMapping.SV
                                where a.VID == svid
                                select a.Name).First();

                externalSVList.Add(svText, kvp.Value.ToString());
            }

            return externalSVList;
        }

        public int GetControlStateVid()
        {
            return GetVidBySVName("ControlState");
        }

        public int GetProcessStateVid()
        {
            return GetVidBySVName("ProcessState");
        }

        public string GetVariableTextByID(int vid)
        {
            return GetSVTextBySVID(vid);
        }

        public string GetVariableValInText(int vid, int val)
        {
            var sv = (from a in Configuration.SVIDMapping.SV
                      where a.VID == vid
                      select a).First();

            foreach (var valueMap in sv.ValueMap)
            {
                if (valueMap.Value.Contains("|"))
                {
                    var vals = valueMap.Value.Split('|');

                    foreach (var value in vals)
                    {
                        if (value == val.ToString())
                        {
                            return valueMap.Text;
                        }
                    }
                }
                else
                {
                    if (valueMap.Value == val.ToString())
                    {
                        return valueMap.Text;
                    }
                }
            }

            return string.Empty;
        }

        public string FilterRecipe(string recipe)
        {
            var ret = recipe;
            var ppDirs = Configuration.EquipmentSettings.PPDirs.PPDir;
            var ppExt = Configuration.EquipmentSettings.PPExt;

            if (ppDirs != null)
            {
                var ppDirList = ppDirs.ToList();
                ppDirList.RemoveAll(x => string.IsNullOrEmpty(x) || string.IsNullOrWhiteSpace(x));
                ppDirs = ppDirList.ToArray();
            }

            if (ppDirs != null &&
                ppDirs.Length > 0)
            {
                var recipeDir = Path.GetDirectoryName(recipe);
                if (!string.IsNullOrEmpty(recipeDir))
                {
                    var exist = ppDirs.Any(x => x == recipeDir);

                    if (exist)
                    {
                        ret = Path.GetFileName(recipe);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            if (!string.IsNullOrEmpty(ppExt))
            {
                ret = Path.GetExtension(ret) == ppExt ? ret : null;
            }

            return ret;
        }



        //public string GetGenericValueMap(string value)
        //{
        //    var mappedValue =
        //        (from a in Configuration.GenericValueMapping.Map
        //         where a.From == value
        //         select a).FirstOrDefault();

        //    return mappedValue == null ? value : mappedValue.To;
        //}

        #endregion

        #region Private Method

        private int GetVidBySVName(string svname)
        {
            var svid = (from a in Configuration.SVIDMapping.SV
                        where a.Name == svname
                        select a.VID).First();

            return svid;
        }

        private string GetSVTextBySVID(int vid)
        {
            var svText = (from a in Configuration.SVIDMapping.SV
                        where a.VID == vid
                        select a.Name).First();

            return svText;
        }

        private string GetSECSItem(string name)
        {
            return
                (from a in Configuration.SECSItemFormats.SECSItemFormat
                 where a.Name == name
                 select a.Value).FirstOrDefault();
        }

        private StateMapping.ControlStateMapping CompileControlState()
        {
            var controlStateMap = new StateMapping.ControlStateMapping();
            //controlStateMap.ControlStateVID = Configuration.SVIDMapping.SV.ControlStateVID;
            var controlStateSVConfig = (from a in Configuration.SVIDMapping.SV
                                        where a.Name == "ControlState"
                                        select a).First();

            controlStateMap.ControlStateVID = controlStateSVConfig.VID;

            controlStateMap.ValueMap = new Dictionary<int, string>();

            foreach (var valueMap in controlStateSVConfig.ValueMap)
            {
                if (valueMap.Value.Contains("|"))
                {
                    var values = from a in valueMap.Value.Split('|')
                                 select Convert.ToInt32(a);

                    foreach (var value in values)
                    {
                        controlStateMap.ValueMap.Add(value, valueMap.Text);
                    }
                }
                else
                {
                    controlStateMap.ValueMap.Add(Convert.ToInt32(valueMap.Value), valueMap.Text);
                }
            }

            return controlStateMap;
        }

        private StateMapping.ProcessStateMapping CompileProcessState()
        {
            var processStateMap = new StateMapping.ProcessStateMapping();
            var processStateSVConfig = (from a in Configuration.SVIDMapping.SV
                                        where a.Name == "ProcessState"
                                        select a).First();

            processStateMap.ValueMap = new Dictionary<int, string>();

            foreach (var valueMap in processStateSVConfig.ValueMap)
            {
                if (valueMap.Value.Contains("|"))
                {
                    var values = from a in valueMap.Value.Split('|')
                                 select Convert.ToInt32(a);

                    foreach (var value in values)
                    {
                        processStateMap.ValueMap.Add(value, valueMap.Text);
                    }
                }
                else
                {
                    processStateMap.ValueMap.Add(Convert.ToInt32(valueMap.Value), valueMap.Text);
                }
            }

            return processStateMap;
        }

        #endregion
    }
}
