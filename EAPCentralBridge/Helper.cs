using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.EAPCentral
{
    internal sealed class Helper
    {
        #region Private Field

        private static EAPCentralConfig mEAPCentralConfig;

        #endregion

        #region Properties

        public static EAPCentralConfig EAPCentralConfigurations
        {
            get { return mEAPCentralConfig.SerializedObject; }
        }

        #endregion

        #region Public Method

        public static bool LoadConfiguration(string configFilePath)
        {
            mEAPCentralConfig = new EAPCentralConfig(configFilePath);

            if (mEAPCentralConfig.Deserialize())
            {
                return true;
            }
            else
            {
                mEAPCentralConfig = null;
                return false;
            }
        }

        public static string GetConfigurationFile(string eapConfigFolder)
        {
            try
            {
                return Path.Combine(eapConfigFolder, ConstantBank.FilePath.CONFIGDIRNAME, ConstantBank.FilePath.CONFIGFILENAME);
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
                return string.Empty;
            }
        }

        #endregion
    }
}
