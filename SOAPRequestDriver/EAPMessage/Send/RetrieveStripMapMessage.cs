using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SOAPRequestDriver.EAPMessage.Send
{
    using Base.BridgeMessage;

    public sealed class RetrieveStripMapMessage : BridgeMessagePacket
    {
        private string mFWEquipmentId;
        private string mEquipmentId;
        private string mStripId;
        private ReplyItem mReply;

        public ReplyItem Reply
        {
            get { return mReply; }
        }

        public class ReplyItem
        {
            public bool Result { get; set; }
            public string StripID { get; set; }
            public int Row { get; set; }
            public int Column { get; set; }
            public int OriginLocation { get; set; }
        }

        public RetrieveStripMapMessage(string fwEquipmentId, string equipmentId, string stripId)
        {
            Subject = "RetrieveStripMap";
            Sender = "SECSDriver";
            Destination = "SOAPRequest";
            MessageID = Guid.NewGuid().ToString();
            mFWEquipmentId = fwEquipmentId;
            mEquipmentId = equipmentId;
            mStripId = stripId;

            CompileData();
        }

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentId, mFWEquipmentId.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentId, mEquipmentId.GetType());
            AddBasicData("STRIPID", mStripId, mStripId.GetType());
        }

        protected override void AssignReplyData()
        {
            mReply = new ReplyItem();
            mReply.Result = (bool)GetReplyData("RESULT").Value;
            mReply.StripID = GetReplyData("STRIPID").Value.ToString();
            mReply.Row = (int)GetReplyData("ROW").Value;
            mReply.Column = (int)GetReplyData("COLUMN").Value;
            mReply.OriginLocation = (int)GetReplyData("ORIGINLOCATION").Value;
        }
    }
}
