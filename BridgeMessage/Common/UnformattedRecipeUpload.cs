using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class UnformattedRecipeUpload : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentID;
        private string mEquipmentID;
        private string mFilePath;
        private string mPPID;
        private byte[] mPPBody;
        private bool mResult;
        private bool mReplyRequired;

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

        public string FilePath
        {
            get { return mFilePath; }
            //set { mFilePath = FilePath; }
            set { mFilePath = value; }
        }

        public string PPID
        {
            get { return mPPID; }
        }

        public byte[] PPBody
        {
            get { return mPPBody; }
            set { mPPBody = value; }
        }

        public bool ReplyRequired
        {
            get { return mReplyRequired; }
        }

        public bool Result
        {
            get { return mResult; }
            set { mReplyRequired = value; }
        }

        #endregion

        #region Constructor

        public UnformattedRecipeUpload()
        {

        }

        public UnformattedRecipeUpload(string fwEquipmentId, string equipmentId, string filePath, string ppid, byte[] ppBody, bool replyRequired, bool result)
        {
            mFWEquipmentID = fwEquipmentId;
            mEquipmentID = equipmentId;
            mPPID = ppid;
            mPPBody = ppBody;
            mFilePath = filePath;
            mReplyRequired = replyRequired;
            mResult = result;

            CompileData();
        }

        public UnformattedRecipeUpload(string fwEquipmentId, string equipmentId, string ppid, byte[] ppBody, bool replyRequired, bool result)
        {
            mFWEquipmentID = fwEquipmentId;
            mEquipmentID = equipmentId;
            mPPID = ppid;
            mPPBody = ppBody;
            mReplyRequired = replyRequired;
            mResult = result;

            CompileData();
        }
        
        public UnformattedRecipeUpload(string fwEquipmentId, string equipmentId, string ppid, byte[] ppBody, bool result)
        {
            mFWEquipmentID = fwEquipmentId;
            mEquipmentID = equipmentId;
            mPPID = ppid;
            mPPBody = ppBody;
            mReplyRequired = false;
            mResult = result;

            CompileData();
        }

        #endregion

        #region Public Method

        public override void CompileData()
        {
            ClearData();
            AddBasicData("FWEQUIPMENTID", mFWEquipmentID, mFWEquipmentID.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentID, mEquipmentID.GetType());
            AddBasicData("PPID", mPPID, mPPID.GetType());
            AddBasicData("PPBODY", mPPBody, typeof(byte[]));
            AddBasicData("FILEPATH", mFilePath, typeof(string));
            AddBasicData("REPLYREQUIRED", mReplyRequired, typeof(bool));
            AddBasicData("RESULT", mResult, typeof(bool));
        }

        #endregion

        #region Protected Method

        protected override void AssignData()
        {
            mFWEquipmentID = GetBasicData("FWEQUIPMENTID").Value.ToString();
            mEquipmentID = GetBasicData("EQUIPMENTID").Value.ToString();
            mEquipmentID = GetBasicData("PPID").Value.ToString();
            mPPBody = GetBasicData("PPBODY").Value == null ? null : GetBasicData("PPBODY").Value as byte[];
            mReplyRequired = GetBasicData("REPLYREQUIRED").Value == null ? false : (bool)GetBasicData("REPLYREQUIRED").Value;
            mReplyRequired = GetBasicData("RESULT").Value == null ? false : (bool)GetBasicData("RESULT").Value;
            mFilePath = GetBasicData("FILEPATH").Value?.ToString();
        }

        #endregion
    }
}
