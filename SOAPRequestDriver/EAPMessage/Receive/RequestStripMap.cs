using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SOAPRequestDriver.EAPMessage.Receive
{
    using Base.BridgeMessage;

    public sealed class RequestStripMap : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentID;
        private string mEquipmentID;
        private string mStripID;
        private Reply mReplyMessage;

        #endregion

        #region Custom Class

        public class Reply
        {
            public bool Result { get; set; }
            public string StripID { get; set; }
            public int Row { get; set; }
            public int Column { get; set; }
            public int OriginLocation { get; set; }
            public Tuple<int, int, string>[] EMapLoc { get; set; }
            public string MapFile { get; set; }
            public string stripMapData { get; set; }
            public string LotID { get; set; }
        }

        #endregion

        #region Constructor

        public RequestStripMap()
        {

        }

        public RequestStripMap(bool requestState)
        {
            mReplyMessage = new Reply();
            mReplyMessage.Result = requestState;
        }

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

        public string StripID
        {
            get { return mStripID; }
        }
        public Reply ReplyMessage
        {
            get { return mReplyMessage; }
            set { mReplyMessage = value; }
        }

        #endregion

        #region Public Method

        public override void CompileReplyData()
        {
            Sender = "SOAPRequest";
            Destination = "SECSDriver";
            Subject = "RetrieveStripMap.Reply";

            AddReplyBasicData("RESULT", mReplyMessage.Result, mReplyMessage.Result.GetType());
            AddReplyBasicData("STRIPID", mReplyMessage.StripID, mReplyMessage.StripID.GetType());
            AddReplyBasicData("ROW", mReplyMessage.Row, mReplyMessage.GetType());
            AddReplyBasicData("COLUMN", mReplyMessage.Column, mReplyMessage.GetType());
            AddReplyBasicData("ORIGINLOCATION", mReplyMessage.OriginLocation, mReplyMessage.GetType());
            AddReplyBasicData("MAPFILE", mReplyMessage.MapFile, mReplyMessage.GetType());
            AddReplyBasicData("EMAPLOC", mReplyMessage.EMapLoc, mReplyMessage.GetType());
            AddReplyBasicData("STRIPMAPDATA", mReplyMessage.stripMapData, mReplyMessage.GetType());
            AddReplyBasicData("LOTID", mReplyMessage.LotID, mReplyMessage.GetType());

        }

        protected override void AssignData()
        {
            //mDataID = GetBasicData("DATAID").Value.ToString();
            mStripID = GetBasicData("STRIPID").Value.ToString();
            mFWEquipmentID = GetBasicData("FWEQUIPMENTID").Value.ToString();
            mEquipmentID = GetBasicData("EQUIPMENTID").Value.ToString();
        }

        #endregion
    }
}
