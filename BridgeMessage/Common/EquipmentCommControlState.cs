using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class EquipmentCommControlState : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentID;
        private string mEquipmentID;
        private string mCommState;
        private string mControlState;

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

        public string CommState
        {
            get { return mCommState; }
        }

        public string ControlState
        {
            get { return mControlState; }
        }

        #endregion

        #region Constructor

        public EquipmentCommControlState()
        {

        }

        public EquipmentCommControlState(string fwEquipmentID, string equipmentID, string commState, string controlState)
        {
            mFWEquipmentID = fwEquipmentID;
            mEquipmentID = equipmentID;
            mCommState = commState;
            mControlState = controlState;

            CompileData();
        }

        #endregion

        #region Public Method

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentID, mFWEquipmentID.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentID, mEquipmentID.GetType());
            AddBasicData("COMMSTATE", mCommState, mCommState.GetType());
            AddBasicData("CONTROLSTATE", mControlState, mControlState.GetType());
        }

        #endregion

        #region Protected Method

        protected override void AssignData()
        {
            mFWEquipmentID = GetBasicData("FWEQUIPMENTID").Value.ToString();
            mEquipmentID = GetBasicData("EQUIPMENTID").Value.ToString();
            mCommState = GetBasicData("COMMSTATE").Value.ToString();
            mControlState = GetBasicData("CONTROLSTATE").Value.ToString();
        }

        #endregion
    }
}
