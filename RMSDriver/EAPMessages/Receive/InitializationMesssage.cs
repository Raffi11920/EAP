using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.RMSDriver.EAPMessages.Receive
{
    using Base.BridgeMessage;

    internal sealed class InitializationMessage : BridgeMessagePacket
    {
        #region Private Field

        private string mEAPConfigFolder;
        private string mFWEquipmentName;
        private string mEquipmentName;
        private string mLogDir;

        #endregion

        #region Properties

        public string EAPConfigFolder
        {
            get { return mEAPConfigFolder; }
        }

        public string FWEquipmentName
        {
            get { return mFWEquipmentName; }
        }

        public string EquipmentName
        {
            get { return mEquipmentName; }
        }

        public string LogDir
        {
            get { return mLogDir; }
        }

        #endregion

        #region Protected Method

        protected override void AssignData()
        {
            mEAPConfigFolder = GetBasicData("EAPConfigFolder").Value.ToString();
            mFWEquipmentName = GetBasicData("FWEquipmentName").Value.ToString();
            mEquipmentName = GetBasicData("EquipmentName").Value.ToString();
            mLogDir = GetBasicData("LogDir").Value.ToString();
        }

        #endregion
    }
}
