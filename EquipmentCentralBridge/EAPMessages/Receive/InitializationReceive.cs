using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.EquipmentCentralDriver.EAPMessages.Receive
{
    using Base.BridgeMessage;

    internal class InitializationReceive : BridgeMessagePacket
    {
        private string mDriverName;
        private string mEAPConfigFolder;

        public string EAPConfigFolder
        {
            get { return mEAPConfigFolder; }
        }

        public string DriverName
        {
            get { return mDriverName; }
        }

        public override void CopyData(IBridgeMessage message)
        {
            mEAPConfigFolder = message.GetBasicData("EAPCONFIGFOLDER").Value.ToString();
            mDriverName = message.GetBasicData("DRIVERNAME").Value.ToString();
        }

    }
}
