using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class EquipmentStatusPacket : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentID;
        private string mEquipmentID;
        private Dictionary<string, string> mDictSV;

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

        public Dictionary<string, string> DictSV
        {
            get { return mDictSV; }
        }

        #endregion

        #region Constructor

        public EquipmentStatusPacket()
        {

        }

        public EquipmentStatusPacket(string fwEquipment, string equipment, Dictionary<string, string> dictSV)
        {
            mFWEquipmentID = fwEquipment;
            mEquipmentID = equipment;
            mDictSV = dictSV;

            CompileData();
        }

        #endregion

        #region Public Method

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentID, mFWEquipmentID.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentID, mEquipmentID.GetType());
            AddBasicData("SVLIST", mDictSV, typeof(Dictionary<string, string>));
        }

        #endregion

        #region Protected Method

        protected override void AssignData()
        {
            mFWEquipmentID = GetBasicData("FWEQUIPMENTID").Value.ToString();
            mEquipmentID = GetBasicData("EQUIPMENTID").Value.ToString();
            mDictSV = GetBasicData("SVLIST").Value as Dictionary<string, string>;
        }

        #endregion

        #region Private Method


        #endregion
    }
}
