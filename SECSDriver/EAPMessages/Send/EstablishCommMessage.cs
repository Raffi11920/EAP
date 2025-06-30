using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SECSDriver.EAPMessages.Send
{
    using Base.BridgeMessage;

    internal sealed class EstablishCommMessage : BridgeMessagePacket
    {
        private EAPEquipmentCommState mEqCommState;
        private string mMDLN;
        private string mSoftRev;
        private bool mReplyRequired;

        public EstablishCommMessage(EAPEquipmentCommState eqCommState, string mdln, string softRev, bool replyRequired, string replySubject)
        {
            Subject = "EstablishCommunication";
            mMDLN = mdln;
            mSoftRev = softRev;
            mEqCommState = eqCommState;
            mReplyRequired = replyRequired;
            MessageID = mReplyRequired ? replySubject : Guid.NewGuid().ToString();
            CompileData();
        }

        public override void CompileData()
        {
            AddBasicData("EQCOMMSTATE", mEqCommState, typeof(EAPEquipmentCommState));
            AddBasicData("MDLN", mMDLN, mMDLN.GetType());
            AddBasicData("SOFTREV", mSoftRev, mSoftRev.GetType());
            AddBasicData("REPLYREQUIRED", mReplyRequired, typeof(bool));

        }
    }
}
