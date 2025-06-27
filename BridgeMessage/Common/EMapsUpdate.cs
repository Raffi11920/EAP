using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class EMapsUpdate : BridgeMessagePacket
    {
        private string mFWEquipmentID;
        private string mEquipmentID;
        private string mStripID;
        private string mPath;
        private int mTimeout;

        public string FWEquipmentID
        {
            get
            {
                return this.mFWEquipmentID;
            }
        }

        public string EquipmentID
        {
            get
            {
                return this.mEquipmentID;
            }
        }

        public string StripID
        {
            get
            {
                return this.mStripID;
            }
        }

        public string Path
        {
            get
            {
                return this.mPath;
            }
        }

        public int Timeout
        {
            get
            {
                return mTimeout;
            }
        }

        public EMapsUpdate(string fwEquipmentId, string equipmentId, string stripID, string path, int timeout)
        {
            mFWEquipmentID = fwEquipmentId;
            mEquipmentID = equipmentId;
            mStripID = stripID;
            mPath = path;
            mTimeout = timeout;
            CompileData();
        }

        public override void CompileData()
        {
            this.AddBasicData("FWEQUIPMENTID", mFWEquipmentID, typeof(string));
            this.AddBasicData("EQUIPMENTID", mEquipmentID, typeof(string));
            this.AddBasicData("STRIPID", mStripID, typeof(string));
            this.AddBasicData("PATH", mPath, typeof(string));
            this.AddBasicData("TIMEOUT", mTimeout, typeof(int));
        }
    }
}
