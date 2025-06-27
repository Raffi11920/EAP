using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    using BridgeMessage;

    public class RecParameterUpload : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentID;
        private string mEquipmentID;
        private string mPPID;
        private string mCode;
        private string mText;
        private string mFilePath;
        private List<RecipeParameterUploadData> mRecipeParameterList;

        #endregion

        #region Custom Class

        public class RecipeParameterUploadData
        {
            public string Name { get; set; }
            public string DataType { get; set; }
            public string Value { get; set; }
        }

        #endregion

        #region Public Properties

        public string FWEquipmentID
        {
            get { return mFWEquipmentID; }
        }

        public string EquipmentID
        {
            get { return mEquipmentID; }
        }

        public string PPID
        {
            get { return mPPID; }
        }

        public string Code
        {
            get { return mCode; }
        }

        public string Text
        {
            get { return mText; }
        }

        public string FilePath
        {
            get { return mFilePath; }
            set { mFilePath = value; }
        }

        public List<RecipeParameterUploadData> RecipeParamUpload
        {
            get { return mRecipeParameterList; }
            set { mRecipeParameterList = value; }
        }

        #endregion

        #region Consturctor

        public RecParameterUpload(
            string fwEquipmentID,
            string equipmentID,
            string ppID,
            List<RecipeParameterUploadData> paramList
        )
        {
            mFWEquipmentID = fwEquipmentID;
            mEquipmentID = equipmentID;
            mPPID = ppID;
            mRecipeParameterList = paramList;

            CompileData();
        }

        #endregion

        #region Public Method

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentID, mFWEquipmentID.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentID, mEquipmentID.GetType());
            AddBasicData("PPID", mPPID, mPPID.GetType());
            AddBasicData("RECIPEPARAMETERUPLOAD", mRecipeParameterList, typeof(List<RecipeParameterUploadData>));
            if (mFilePath != null)
            {
                AddBasicData("FILEPATH", mFilePath, mFilePath.GetType());
            }
        }

        #endregion
    }
}
