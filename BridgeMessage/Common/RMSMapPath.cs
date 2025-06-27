using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class RMSMapPath : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentName;
        private string mEquipmentName;
        private string mRecipeUploadPath;

        #endregion

        #region Properties

        public string FWEquipmentName
        {
            get { return mFWEquipmentName; }
        }

        public string EquipmentName
        {
            get { return mEquipmentName; }
        }

        public string RecipeUploadPath
        {
            get { return mRecipeUploadPath; }
        }

        #endregion

        #region Constructor

        public RMSMapPath()
        {

        }

        public RMSMapPath(string fwEquipmentId, string equipmentId)
        {
            mFWEquipmentName = fwEquipmentId;
            mEquipmentName = equipmentId;

            CompileData();
        }

        public RMSMapPath(string fwEquipmentId, string equipmentId, string recipeUploadPath)
        {
            mFWEquipmentName = fwEquipmentId;
            mEquipmentName = equipmentId;
            mRecipeUploadPath = recipeUploadPath;

            CompileData();
        }

        #endregion

        #region Public Method

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentName, mFWEquipmentName.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentName, mEquipmentName.GetType());
            AddBasicData("RECIPEUPLOADPATH", mRecipeUploadPath, typeof(string));
        }

        #endregion

        #region Protected Method

        protected override void AssignData()
        {
            mFWEquipmentName = GetBasicData("FWEquipmentName").Value.ToString();
            mEquipmentName = GetBasicData("EquipmentName").Value.ToString();
            mRecipeUploadPath = GetBasicData("RECIPEUPLOADPATH").Value?.ToString();
        }

        #endregion
    }
}
