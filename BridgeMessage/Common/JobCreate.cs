using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    using Base.BridgeMessage;

    public class JobCreate : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentID;
        private string mEquipmentID;
        private string mJobID;
        private string mDevice;
        private string mPackage;
        private string mLot;
        private string mPPID;
        private string mUserID;

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

        public string JobID
        {
            get { return mJobID; }
        }

        public string UserID
        {
            get { return mUserID; }
        }

        public string Device
        {
            get { return mDevice; }
            set { mDevice = value; }
        }

        public string Package
        {
            get { return mPackage; }
            set { mDevice = value; }
        }

        public string Lot
        {
            get { return mLot; }
            set { mDevice = value; }
        }

        public string PPID
        {
            get { return mPPID; }
            set { mPPID = value; }
        }

        #endregion

        #region Constructor

        public JobCreate(
            string fwEquipmentId, 
            string equipmentId, 
            string jobId,
            string device,
            string package,
            string lot,
            string ppid,
            string userId)
        {
            mFWEquipmentID = fwEquipmentId;
            mEquipmentID = equipmentId;
            mJobID = jobId;
            mDevice = device;
            mPackage = package;
            mLot = lot;
            mPPID = ppid;
            mUserID = userId;
            CompileData();
        }

        public JobCreate(
            string fwEquipmentId,
            string equipmentId)
        {
            mFWEquipmentID = fwEquipmentId;
            mEquipmentID = equipmentId;

            CompileData();
        }

        public JobCreate(
            string fwEquipmentId,
            string equipmentId,
            string jobId)
        {
            mFWEquipmentID = fwEquipmentId;
            mEquipmentID = equipmentId;
            mJobID = jobId;

            CompileData();
        }

        #endregion

        #region Public Method

        public override sealed void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentID, typeof(string));
            AddBasicData("EQUIPMENTID", mEquipmentID, typeof(string));
            AddBasicData("JOBID", mJobID, typeof(string));
            AddBasicData("DEVICE", mDevice, typeof(string));
            AddBasicData("PACKAGE", mPackage, typeof(string));
            AddBasicData("LOT", mLot, typeof(string));
            AddBasicData("PPID", mPPID, typeof(string));
            AddBasicData("USERID", mUserID, typeof(string));
        }

        #endregion

        #region Protected Method

        protected override void AssignData()
        {
            mFWEquipmentID = GetBasicData("FWEQUIPMENTID").Value.ToString();
            mEquipmentID = GetBasicData("EQUIPMENTID").Value.ToString();
            mJobID = GetBasicData("JOBID").Value.ToString();
            mDevice = GetBasicData("DEVICE").Value.ToString();
            mPackage= GetBasicData("PACKAGE").Value.ToString();
            mLot = GetBasicData("LOT").Value.ToString();
            mPPID = GetBasicData("PPID").Value.ToString();
            mUserID = GetBasicData("USERID").Value.ToString();
        }

        #endregion
    }
}
