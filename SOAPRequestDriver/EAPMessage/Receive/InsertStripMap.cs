using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SOAPRequestDriver.EAPMessage.Receive
{
    using Base.BridgeMessage;

    public sealed class InsertStripMap : BridgeMessagePacket
    {
        #region Private Field

        private bool mResult;
        private string mFWEquipmentID;
        private string mEquipmentID;

        #endregion

        #region Public Properties

        public string FWEquipmentID
        {
            get { return mFWEquipmentID; }
        }

        public string EquipmentID
        {
            get { return mEquipmentID; }
        }

        public bool Result
        {
            get { return mResult; }
            set { mResult = value; }
        }


        #endregion

        #region Constructor


        #endregion

        #region Public Method

        public override void CompileData()
        {
        }

        protected override void AssignData()
        {
            mFWEquipmentID = GetBasicData("FWEQUIPMENTID").Value.ToString();
            mEquipmentID = GetBasicData("EQUIPMENTID").Value.ToString();
        }

        #endregion

    }
}
