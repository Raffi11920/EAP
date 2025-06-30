using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace Qynix.EAP.Drivers.RMSDriver
{
    using Base.BridgeMessage;
    using Utilities.ExtensionPlug;
    using Utilities.FileUtilities;
    using Base.BridgeMessage.Common;
    using RecValidationHandling;

    public sealed class Helper
    {
        #region Private Field

        private RMSConfig mRMSConfig;

        #endregion

        #region Public Properties

        public RMSConfig Configuration
        {
            get { return mRMSConfig.SerializedObject; }
        }

        #endregion

        #region Public Method

        public string GetRecipeUploadPath(string equipmentId)
        {
            var uploadPath = Configuration.RecipeUploadSetting.UploadPath;

            if (!string.IsNullOrEmpty(uploadPath))
            {
                var matches = Regex.Matches(uploadPath, @"(?<=\{)[^}]*(?=\})");

                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        foreach (Group group in match.Groups)
                        {
                            var matchString = group.Value;
                            var split = matchString.Split(':');

                            switch (split[0])
                            {
                                case "MAPPATH":
                                    var pathName = split[1];
                                    var val = GetMapPathValue(pathName).MapPath;
                                    uploadPath = Regex.Replace(uploadPath, @"\{" + matchString + @"\}", val);
                                    break;

                                case "VAL":
                                    var mapVal = split[1];
                                    if (mapVal == "EQUIPMENTID")
                                    {
                                        uploadPath = Regex.Replace(uploadPath, @"\{" + matchString + @"\}", equipmentId);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            
            return uploadPath;
        }

        public string NewValue(string value, string paramName)
        {
            var param = Configuration.Parameters.Param.FirstOrDefault(x => x.Name == paramName);

            if (param.Operator == string.Empty || param.Suffix == string.Empty)
            {
                return value;
            }

            var result = 0.0f;

            if (!float.TryParse(value, out result) || !float.TryParse(param.Suffix, out result))
            {
                return value;
            }

            switch (param.Operator)
            {
                case "+":
                    return (Convert.ToSingle(value) + Convert.ToSingle(param.Suffix)).ToString();

                case "-":
                    return (Convert.ToSingle(value) - Convert.ToSingle(param.Suffix)).ToString();

                case "*":
                    return (Convert.ToSingle(value) * Convert.ToSingle(param.Suffix)).ToString();

                case "/":
                    return (Convert.ToSingle(value) / Convert.ToSingle(param.Suffix)).ToString();

                default:
                    return value;
            }
        }

        public int MapAllPath()
        {
            return MapAllPath(string.Empty, string.Empty);
        }

        public int MapAllPath(string fwEquipmentId, string equipmentId)
        {
            var pathInfos = Configuration.MapPath.Path;
            var result = EAPError.OK;

            foreach (var pathInfo in pathInfos)
            {
                result = MapDrive(pathInfo.PathName, fwEquipmentId, equipmentId);

                if (result != EAPError.OK)
                    return result;
            }

            return result;
        }

        public int MapDrive(string pathName)
        {
            return MapDrive(pathName, string.Empty, string.Empty);
        }

        public int MapDrive(string pathName, string fwEquipmentId, string equipmentId)
        {
            var pathInfo = GetMapPathValue(pathName);

            if (pathInfo == null)
                return EAPError.DATA_NOT_FOUND;

            var pathValue = pathInfo.PathValue;
            if (pathValue.Contains("{FWEQUIPMENTID}"))
            {
                pathValue = pathValue.Replace("{FWEQUIPMENTID}", fwEquipmentId);
            }

            if (pathValue.Contains("{EQUIPMENTID}"))
            {
                pathValue = pathValue.Replace("{EQUIPMENTID}", equipmentId);
            }

            if (Directory.Exists(pathInfo.MapPath))
                return EAPError.OK;

            var networkDrive = new NetworkDrive();
            networkDrive.LocalDrive = pathInfo.MapPath;
            networkDrive.Persistent = true;
            networkDrive.SaveCredentials = true;
            networkDrive.ShareName = pathValue;

            networkDrive.MapDrive(pathInfo.Username, pathInfo.Password);
            return EAPError.OK;
        }

        public RMSConfig.MapPathSection.PathSection GetMapPathValue(string pathName)
        {
            return (from a in Configuration.MapPath.Path
                    where a.PathName == pathName
                    select a).FirstOrDefault();
        }

        public bool LoadConfiguration(string configFilePath)
        {
            mRMSConfig = new RMSConfig(configFilePath);

            if (mRMSConfig.Deserialize())
            {
                return true;
            }
            else
            {
                mRMSConfig = null;
                return false;
            }
        }

        public string GetConfigFilePath(string eapConfigFolder, string equipmentName)
        {
            return Path.Combine(eapConfigFolder, ConstantBank.FilePath.CONFIGDIR, "EQC_{0}".FillArguments(equipmentName), ConstantBank.FilePath.CONFIGFILE);
        }

        public string GetParamsFilePath(string eapConfigFolder, string equipmentName)
        {
            return Path.Combine(eapConfigFolder, ConstantBank.FilePath.CONFIGDIR, "EQC_{0}".FillArguments(equipmentName), ConstantBank.FilePath.PARAMSFILE);
        }

        #endregion
    }
}
