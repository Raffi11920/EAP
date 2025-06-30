using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SOAPRequestDriver
{
    using Base.BridgeMessage;
    using EAPMessage.Receive;
    using Utilities.ExtensionPlug;
    using static ConstantBank;

    public sealed class Helper
    {
        #region Private Field

        private SOAPRequestConfig mConfiguration;
        private delegate object DefectCodePrefixHandle(object prefix, object defectCodes);
        private Dictionary<DefectCodePrefixType, DefectCodePrefixHandle> mDictDefectCodeHandler;

        #endregion

        #region Properties

        public SOAPRequestConfig Configuration
        {
            get
            {
                return mConfiguration.SerializedObject;
            }
        }

        #endregion

        #region Public Method

        public void Initialize()
        {
            mDictDefectCodeHandler = new Dictionary<DefectCodePrefixType, DefectCodePrefixHandle>();
            mDictDefectCodeHandler.Add(DefectCodePrefixType.Add, HandleDefectCodeAddition);
            mDictDefectCodeHandler.Add(DefectCodePrefixType.Concat, HandleDefectCodeConcat);
        }

        public bool LoadConfiguration(string configFilePath)
        {
            mConfiguration = new SOAPRequestConfig(configFilePath);

            if (mConfiguration.Deserialize())
            {
                Initialize();
                return true;
            }
            else
            {
                mConfiguration = null;
                return false;
            }
        }

        public string GetConfigurationFile(string eapConfigFolder, string equipmentName)
        {
            return Path.Combine(eapConfigFolder, FilePath.CONFIGDIR, "EQC_{0}".FillArguments(equipmentName), FilePath.CONFIGFILE);
        }

        public RequestStripMap GetRequestStripMap(IBridgeMessage bridgeMessage)
        {
            RequestStripMap requestStripMap = new RequestStripMap();
            requestStripMap.CopyData(bridgeMessage);

            return requestStripMap;
        }

        public InsertStripMap GetInsertStripMap(IBridgeMessage bridgeMessage)
        {
            InsertStripMap insertStripMap = new InsertStripMap();
            insertStripMap.CopyData(bridgeMessage);

            return insertStripMap;
        }

        public EquipmentEventReceive GetEquipmentEvent(IBridgeMessage bridgeMessage)
        {
            EquipmentEventReceive equipmentEventReceive = new EquipmentEventReceive();
            equipmentEventReceive.CopyData(bridgeMessage);

            return equipmentEventReceive;
        }

        public object HandleDefectCode(object defectCode)
        {
            var defectCodePrefixType = Configuration.Setting.DefectCodePrefixType;
            var defectCodePrefixTypeEnum = defectCodePrefixType.ToEnum<DefectCodePrefixType>();

            var handler = mDictDefectCodeHandler[defectCodePrefixTypeEnum];
            var defectCodePrefix = Configuration.Setting.DefectCodePrefix;

            return handler(defectCodePrefix, defectCode);
        }

        public string GenerateEMapPath(string lotID)
        {
            return
                Path.Combine(
                    Configuration.Setting.Path,
                    DateTime.Now.Year.ToString(),
                    DateTime.Now.Month.ToString("D2"),
                    DateTime.Now.Day.ToString("D2"),
                    lotID);
        }

        #endregion

        #region Private Method

        private object HandleDefectCodeAddition(object prefix, object defectCode)
        {
            return Convert.ToInt32(prefix) + Convert.ToInt32(defectCode);
        }

        private object HandleDefectCodeConcat(object prefix, object defectCode)
        {
            return prefix.ToString() + Convert.ToInt32(defectCode).ToString("D3");
        }

        #endregion
    }
}
