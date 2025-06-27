using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class EquipmentProcessState : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentID;
        private string mEquipmentID;
        private string mPreviousProcessState;
        private string mCurrentProcessState;

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

        public string PreviousProcessState
        {
            get { return mPreviousProcessState; }
        }

        public string CurrentProcessState
        {
            get { return mCurrentProcessState; }
        }

        #endregion

        #region Constructor

        public EquipmentProcessState()
        {

        }

        public EquipmentProcessState(string fwEquipmentID, string equipmentID, string currentProcessState, string previousProcessState)
        {
            mFWEquipmentID = fwEquipmentID;
            mEquipmentID = equipmentID;
            mCurrentProcessState = currentProcessState;
            mPreviousProcessState = previousProcessState;

            CompileData();
        }

        #endregion

        #region Public Method

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentID, mFWEquipmentID.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentID, mEquipmentID.GetType());
            AddBasicData("CURRENTPROCESSSTATE", mCurrentProcessState, mCurrentProcessState.GetType());
            AddBasicData("PREVIOUSPROCESSSTATE", mPreviousProcessState, mPreviousProcessState.GetType());
        }

        #endregion

        #region Protected Method

        protected override void AssignData()
        {
            mFWEquipmentID = GetBasicData("FWEQUIPMENTID").Value.ToString();
            mEquipmentID = GetBasicData("EQUIPMENTID").Value.ToString();
            mPreviousProcessState = GetBasicData("COMMSTATE").Value.ToString();
            mCurrentProcessState = GetBasicData("CONTROLSTATE").Value.ToString();
        }

        #endregion
    }
}
