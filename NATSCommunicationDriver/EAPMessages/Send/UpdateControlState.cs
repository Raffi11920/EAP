using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.NATSCommunicationDriver.EAPMessages.Send
{
    using Base.BridgeMessage;

    public class UpdateControlState : BridgeMessagePacket
    {
        private string mFWEquipmentId;
        private string mEquipmentId;
        private string mControlState;

        public UpdateControlState(string fwEquipmentId, string equipmentId, string controlState)
        {
            Subject = "UpdateControlState";
            mFWEquipmentId = fwEquipmentId;
            mEquipmentId = equipmentId;
            mControlState = controlState;
            CompileData();
        }

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentId, mFWEquipmentId.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentId, mEquipmentId.GetType());
            AddBasicData("CONTROLSTATE", mControlState, mControlState.GetType());
        }
    }
}
