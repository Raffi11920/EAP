using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.EAPCentral.EAPMessages.Send
{
    using Base.BridgeMessage;

    internal sealed class InitializationMessage : BridgeMessagePacket
    {
        #region Private Field

        private string mDriverName;
        private string mEAPConfigFolder;

        #endregion

        #region Properties

        public string DriverName
        {
            get { return mDriverName; }
        }

        #endregion

        #region Constructor

        public InitializationMessage(string eapConfigFolder)
        {
            mEAPConfigFolder = eapConfigFolder;
            this.CompileData();
        }

        public InitializationMessage(string eapConfigFolder, string driverName)
        {
            mEAPConfigFolder = eapConfigFolder;
            mDriverName = driverName;
            this.CompileData();
        }

        #endregion

        #region Public Method

        public override void CompileData()
        {
            AddBasicData("EAPConfigFolder", mEAPConfigFolder, mEAPConfigFolder.GetType());

            if (!string.IsNullOrEmpty(mDriverName))
            {
                AddBasicData("DRIVERNAME", mDriverName, mDriverName.GetType());
            }
        }

        #endregion


    }
}
