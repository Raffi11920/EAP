using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class AMREvent : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentID;
        private string mEquipmentID;

        private int mStripCount;
        private string mAction;
        private string mLotID;
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

        public int StripCount
        {
            get { return mStripCount; }
        }

        public string Action
        {
            get { return mAction; }
        }

        public string LotID
        {
            get { return mLotID; }
        }

        #endregion

        #region Constructor

        public AMREvent(string fwEquipmentId, string equipmentId, int stripCount, string action, string lotId)
        {
            mFWEquipmentID = fwEquipmentId;
            mEquipmentID = equipmentId;
            mStripCount = stripCount;
            mAction = action;
            mLotID = lotId;
            CompileData();
        }

        #endregion

        #region Public Method 

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentID, mFWEquipmentID.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentID, mEquipmentID.GetType());
            AddBasicData("STRIPCOUNT", mStripCount, mStripCount.GetType());
            AddBasicData("ACTION", mAction, mAction.GetType());
            AddBasicData("LOTID", mLotID, mLotID.GetType());
        }

        #endregion

    }
}
