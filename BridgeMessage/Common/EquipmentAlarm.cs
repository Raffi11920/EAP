using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class EquipmentAlarmPacket : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentID;
        private string mEquipmentID;
        private long mAlarmID; 
        private string mAlarmText;
        private byte mAlarmSet;

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

        public long AlarmID
        {
            get { return mAlarmID; }
        }

        public string AlarmText
        {
            get { return mAlarmText; }
        }

        public byte AlarmSet
        {
            get { return mAlarmSet; }
        }

        #endregion

        #region Constructor

        public EquipmentAlarmPacket()
        {

        }

        public EquipmentAlarmPacket(string fwEquipmentID, string equipmentID, long alarmID, string alarmText, byte alarmSet)
        {
            mFWEquipmentID = fwEquipmentID;
            mEquipmentID = equipmentID;
            mAlarmID = alarmID;
            mAlarmText = alarmText;
            mAlarmSet = alarmSet;

            CompileData();
        }

        #endregion

        #region Protected Method

        protected override void AssignData()
        {
            mFWEquipmentID = GetBasicData("FWEQUIPMENTID").Value.ToString();
            mEquipmentID = GetBasicData("EQUIPMENTID").Value.ToString();
            mAlarmID = Convert.ToInt64(GetBasicData("ALARMID").Value);
            mAlarmSet = Convert.ToByte(GetBasicData("ALARMSET").Value);
            mAlarmText = GetBasicData("ALARMTEXT").Value.ToString();
        }

        #endregion

        #region Public Method

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentID, mFWEquipmentID.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentID, mEquipmentID.GetType());
            AddBasicData("ALARMID", mAlarmID, mAlarmID.GetType());
            AddBasicData("ALARMTEXT", mAlarmText, mAlarmText.GetType());
            AddBasicData("ALARMSET", mAlarmSet, mAlarmSet.GetType());
        }

        #endregion
    }
}
