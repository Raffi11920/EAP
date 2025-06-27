using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class EquipmentTerminalDisplay : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentID;
        private string mEquipmentID;
        private string mMessage;
        private byte mTerminalNumber;

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

        public string Message
        {
            get
            {
                return mMessage;
            }
        }
        public byte TerminalNumber
        {
            get { return mTerminalNumber; }
        }
        #endregion

        #region Constructor

        public EquipmentTerminalDisplay(string fwEquipmentId, string equipmentId, string message)
        {
            mFWEquipmentID = fwEquipmentId;
            mEquipmentID = equipmentId;
            mMessage = message;
            mTerminalNumber = 0;
            CompileData();
        }
        public EquipmentTerminalDisplay(string fwEquipmentId, string equipmentId, string message, byte terminalNumber = 0)
        {
            mFWEquipmentID = fwEquipmentId;
            mEquipmentID = equipmentId;
            mMessage = message;
            mTerminalNumber = terminalNumber;
            CompileData();
        }
        #endregion

        #region Public Method 

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentID, mFWEquipmentID.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentID, mEquipmentID.GetType());
            AddBasicData("MESSAGE", mMessage, mMessage.GetType());
            AddBasicData("TERMINALNUMBER", mTerminalNumber, mTerminalNumber.GetType());
        }

        #endregion
    }
}
