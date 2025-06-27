using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class EquipmentTerminalDisplayMulti : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentID;
        private string mEquipmentID;
        private string[] mMessage;

        #endregion

        #region Properties

        public string FWEquipmentID
        {
            get { return mFWEquipmentID; }
        }

        public string EquipmentID
        {
            get { return mEquipmentID; }
        }

        public string[] Message
        {
            get
            {
                return mMessage;
            }
        }

        #endregion

        #region Constructor

        public EquipmentTerminalDisplayMulti(string fwEquipmentId, string equipmentId, string[] message)
        {
            mFWEquipmentID = fwEquipmentId;
            mEquipmentID = equipmentId;
            mMessage = message;
            CompileData();
        }

        #endregion

        #region Public Method 

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentID, mFWEquipmentID.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentID, mEquipmentID.GetType());
            AddBasicData("MESSAGE", mMessage, mMessage.GetType());
        }

        #endregion
    }
}
