using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SOAPRequestDriver.EAPMessage.Receive
{
    using Base.BridgeMessage;

    internal sealed class InitializationReceive : BridgeMessagePacket
    {
        private string mEAPConfigFolder;
        private string mEquipmentName;
        private string mFWEquipmentName;
        private string mLogDir;

        public string EAPConfigFolder
        {
            get { return mEAPConfigFolder; }
        }

        public string EquipmentName
        {
            get { return mEquipmentName; }
        }

        public string FWEquipmentName
        {
            get { return mFWEquipmentName; }
        }

        public string LogDir
        {
            get { return mLogDir; }
        }

        protected override void AssignData()
        {
            mEAPConfigFolder = GetBasicData("EAPCONFIGFOLDER").Value.ToString();
            mEquipmentName = GetBasicData("EQUIPMENTNAME").Value.ToString();
            mFWEquipmentName = GetBasicData("FWEQUIPMENTNAME").Value.ToString();
            mLogDir = GetBasicData("LOGDIR").Value.ToString();
        }

        public override void CompileData()
        {
            throw new NotImplementedException();
        }
    }
}
