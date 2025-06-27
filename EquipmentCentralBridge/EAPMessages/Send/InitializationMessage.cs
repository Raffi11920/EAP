using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.EquipmentCentralDriver.EAPMessages.Send
{
    using Base.BridgeMessage;

    internal sealed class InitializationMessage : BridgeMessagePacket
    {
        #region Private Field

        private string mEAPConfigFolder;
        private string mFWEquipmentName;
        private string mEquipmentName;

        #endregion

        #region Properties

        public string EAPConfigFolder
        {
            get { return mEAPConfigFolder; }
        }

        #endregion

        #region Constructor

        public InitializationMessage()
        {

        }

        public InitializationMessage(string eapConfigFolder, string fwEquipmentName, string equipmentName)
        {
            mEAPConfigFolder = eapConfigFolder;
            mFWEquipmentName = fwEquipmentName;
            mEquipmentName = equipmentName;

            this.CompileData();
        }

        #endregion

        #region Public Method

        public override void CompileData()
        {
            AddBasicData("EAPCONFIGFOLDER", mEAPConfigFolder, mEAPConfigFolder.GetType());
            AddBasicData("FWEQUIPMENTNAME", mFWEquipmentName, mFWEquipmentName.GetType());
            AddBasicData("EQUIPMENTNAME", mEquipmentName, mEquipmentName.GetType());
        }

        #endregion
    }
}
