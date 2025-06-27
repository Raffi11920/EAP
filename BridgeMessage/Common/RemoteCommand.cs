using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//add by Fahmi 9/3/2020 | add LotSize for AOI IVE
//add new parameter mLotSize to return lot size

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class RemoteCommand : JobCreate
    {
        private string mCommand;
        private bool mReplyRequired;
        private string mLoadPort;
        private string mLotSize;
        private string mMagazineCount;

        public RemoteCommand(
            string fwEquipmentId, string equipmentId, string jobId, 
            string device, string package, string lot, 
            string ppid, string command, string loadPort, string userId, string lotSize, string magazineCount, bool replyRequired) : 
            base(fwEquipmentId, equipmentId, jobId, 
                device, package, lot, ppid, userId)
        {
            mCommand = command;
            mReplyRequired = replyRequired;
            mLoadPort = loadPort;
            mLotSize = lotSize;
            mMagazineCount = magazineCount;

            CompileData();
        }

        public RemoteCommand(
            string fwEquipmentId,
            string equipmentId,
            string command,
            string jobId,
            int errorCode,
            string errorText) : base (fwEquipmentId, equipmentId, jobId)
        {
            mCommand = command;
            
            ErrorCode = errorCode;
            ErrorText = errorText;

            CompileData();
        }

        public string Command
        {
            get { return mCommand; }
        }

        public string LoadPort
        {
            get { return mLoadPort; }
        }

        public string LotSize
        {
            get { return mLotSize; }
        }

        public string MagazineCount
        {
            get { return mMagazineCount; }
        }

        public bool ReplyRequired
        {
            get { return mReplyRequired; }
        }

        public new void CompileData()
        {
            AddBasicData("COMMAND", mCommand, mCommand.GetType());
            AddBasicData("REPLYREQUIRED", mReplyRequired, typeof(bool));
        }

        protected override void AssignData()
        {
            mCommand = GetBasicData("COMMAND").Value.ToString();
            mReplyRequired = GetBasicData("REPLYREQUIRED") == null ? false : (bool)GetBasicData("REPLYREQUIRED").Value;
        }
    }
}
