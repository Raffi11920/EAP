using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.EquipmentCentralDriver
{
    internal sealed class Helper
    {
        #region Private Field

        private static EquipmentCentralConfig mConfiguration;

        #endregion

        #region Properties

        public static EquipmentCentralConfig Configuration
        {
            get
            {
                return mConfiguration.SerializedObject;
            }
        }

        #endregion

        #region Public Method

        public static bool LoadConfiguration(string configFilePath)
        {
            mConfiguration = new EquipmentCentralConfig(configFilePath);

            if (mConfiguration.Deserialize())
            {
                return true;
            }
            else
            {
                mConfiguration = null;
                return false;
            }
        }

        public static string GetConfigurationFile(string eapConfigFolder)
        {
            return Path.Combine(eapConfigFolder, ConstantBank.FilePath.CONFIGDIR, ConstantBank.FilePath.CONFIGFILE);
        }

        #endregion
    }
}
