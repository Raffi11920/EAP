using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SOAPRequestDriver.EventManager.EventArguments
{
    using EAPMessage.Receive;

    public class ServiceArgs
    {
        #region Private Field

        private bool mNotifyUser;
        private string mFWEquipmentID;
        private string mEquipmentID;
        private string mStripID;
        private string mMapFile;
        private int mUC;
        private int mUR;
        private int mCC;
        private int mCR;
        private RequestStripMap mRequestStripMap;

        #endregion

        #region Properties Field

        public bool NotifyUser
        {
            get { return mNotifyUser; }
        }

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

        public string MapFile
        {
            get { return mMapFile; }
        }

        public int UC
        {
            get { return UC; }
        }
        
        public int UR
        {
            get { return mUR; }
        }

        public int CC
        {
            get { return mCC; }
        }

        public int CR
        {
            get { return mCR; }
        }

        public RequestStripMap RequestStripMap
        {
            get { return mRequestStripMap; }
        }
        
        #endregion

        public ServiceArgs(string fwEquipmentId, string equipmentId, string stripId, bool notifyUser)
        {
            mFWEquipmentID = fwEquipmentId;
            mEquipmentID = equipmentId;
            mStripID = stripId;
            mNotifyUser = notifyUser;
        }

        public ServiceArgs(string fwEquipmentId, string equipmentId, string stripId, string mapFile, int uc, int ur, int cc, int cr)
        {
            mStripID = stripId;
            mMapFile = mapFile;
            mFWEquipmentID = fwEquipmentId;
            mEquipmentID = equipmentId;
            mUC = uc;
            mUR = ur;
            mCC = cc;
            mCR = cr;
        }

        public ServiceArgs(string fwEquipmentId, string equipmentId, string stripId, string mapFile, int uc, int ur, int cc, int cr, RequestStripMap requestStripMap)
        {
            mStripID = stripId;
            mMapFile = mapFile;
            mFWEquipmentID = fwEquipmentId;
            mEquipmentID = equipmentId;
            mUC = uc;
            mUR = ur;
            mCC = cc;
            mCR = cr;
            mRequestStripMap = requestStripMap;
        }
    }
}
