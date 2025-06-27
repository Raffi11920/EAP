using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class EquipmentConstantPacket : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentID;
        private string mEquipmentID;
        private Dictionary<string, string> mDictEC;

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

        public Dictionary<string, string> DictEC
        {
            get { return mDictEC; }
        }

        #endregion

        #region Constructor

        public EquipmentConstantPacket()
        {

        }

        public EquipmentConstantPacket(string fwEquipment, string equipment, Dictionary<string, string> dictEC)
        {
            mFWEquipmentID = fwEquipment;
            mEquipmentID = equipment;
            mDictEC = dictEC;

            CompileData();
        }

        #endregion

        #region Public Method

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentID, mFWEquipmentID.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentID, mEquipmentID.GetType());
            AddBasicData("ECLIST", mDictEC, typeof(Dictionary<string, string>));
        }

        #endregion

        #region Protected Method

        protected override void AssignData()
        {
            mFWEquipmentID = GetBasicData("FWEQUIPMENTID").Value.ToString();
            mEquipmentID = GetBasicData("EQUIPMENTID").Value.ToString();
            mDictEC = GetBasicData("ECLIST").Value as Dictionary<string, string>;
        }

        #endregion

        #region Private Method


        #endregion
    }
}
