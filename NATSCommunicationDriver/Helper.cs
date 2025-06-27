using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.NATSCommunicationDriver
{
    using Utilities.ExtensionPlug;

    public sealed class Helper
    {
        #region Private Field

        private NATSClientConfig mNATSClientConfig;

        #endregion

        #region Properties

        public NATSClientConfig Configuration
        {
            get { return mNATSClientConfig.SerializedObject; }
        }

        #endregion

        #region Public Method

        public bool LoadConfiguration(string configFIlePath)
        {
            mNATSClientConfig = new NATSClientConfig(configFIlePath);

            if (mNATSClientConfig.Deserialize())
            {
                return true;
            }
            else
            {
                mNATSClientConfig = null;
                return false;
            }
        }

        public string GetConfigFilePath(string eapConfigFolder)
        {
            return Path.Combine(eapConfigFolder, ConstantBank.FilePath.CONFIGDIR, ConstantBank.FilePath.CONFIGFILE);
        }

        #endregion
    }
}
