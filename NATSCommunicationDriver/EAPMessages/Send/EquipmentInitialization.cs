using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.NATSCommunicationDriver.EAPMessages.Send
{
    using Base.BridgeMessage;

    internal class EquipmentInitialization : BridgeMessagePacket
    {
        private string mFWEquipmentID;
        private string mEquipmentID;
        private bool mReplyRequired;

        public EquipmentInitialization(bool replyRequired, string replySubject, string fwEquipmentID, string equipmentID)
        {
            MessageID = replySubject;
            mFWEquipmentID = fwEquipmentID;
            mEquipmentID = equipmentID;
            mReplyRequired = replyRequired;

            CompileData();
        }

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentID, mFWEquipmentID.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentID, mEquipmentID.GetType());
            AddBasicData("REPLYREQUIRED", mReplyRequired, mReplyRequired.GetType());
        }
    }
}
