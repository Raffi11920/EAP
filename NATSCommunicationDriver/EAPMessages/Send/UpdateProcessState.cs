using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.NATSCommunicationDriver.EAPMessages.Send
{
    using Base.BridgeMessage;

    public class UpdateProcessState : BridgeMessagePacket
    {
        private string mFWEquipmentId;
        private string mEquipmentId;
        private string mProcessState;
        private string mPreviousProcessState;

        public UpdateProcessState(string fwEquipmentId, string equipmentId, string processState, string previousProcessState)
        {
            Subject = "UpdateProcessState";
            mFWEquipmentId = fwEquipmentId;
            mEquipmentId = equipmentId;
            mProcessState = processState;
            mPreviousProcessState = previousProcessState;
            CompileData();
        }

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentId, mFWEquipmentId.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentId, mEquipmentId.GetType());
            AddBasicData("PROCESSSTATE", mProcessState, mProcessState.GetType());
            AddBasicData("PREVIOUSPROCESSSTATE", mPreviousProcessState, mPreviousProcessState.GetType());
        }
    }
}
