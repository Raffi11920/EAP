using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SECSDriver.EAPMessages.Send
{
    using Base.BridgeMessage;

    internal sealed class EventReportMessage : BridgeMessagePacket
    {
        private string mFWEquipmentId;
        private string mEquipmentId;
        private SimpleItem mEventItems;

        public EventReportMessage(string fwEquipmentId, string equipmentId, SimpleItem eventItems)
        {
            Subject = "Equipment.Event";
            Sender = "SECSDriver";
            Destination = "All";
            mFWEquipmentId = fwEquipmentId;
            mEquipmentId = equipmentId;
            mEventItems = eventItems;
            CompileData();
        }

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentId, mFWEquipmentId.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentId, mEquipmentId.GetType());
            AddBasicData("EVENTITEMS", mEventItems, mEventItems.GetType());
        }

    }
}
