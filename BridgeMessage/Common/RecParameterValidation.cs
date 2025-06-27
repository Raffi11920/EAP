using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    using Base.BridgeMessage;

    public class RecParameterValidation : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentID;
        private string mEquipmentID;
        private string mPPID;
        private string mCommand;
        private string mCode;
        private string mText;
        private List<RecipeParameter> mRecipeParameterList;

        #endregion

        #region Custom Class

        public class RecipeParameter : RecParameterUpload.RecipeParameterUploadData
        {
            public EnumValidationRule ValidationRule { get; set; }
            public string MinValue { get; set; }
            public string MaxValue { get; set; }
            public string ParameterErrorText { get; set; }
        }

        public enum EnumValidationRule
        {
            Range,
            Equal,
            LessThan,
            MoreThan,
            LessThanEqual,
            MoreThanEqual,
            Contains,
            NotContains
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

        public string Command
        {
            get { return mCommand; }
        }

        public string Code
        {
            get { return mCode; }
            set { mCode = value; }
        }

        public string Text
        {
            get { return mText; }
            set { mText = value; }
        }

        public List<RecipeParameter> RecipeParam
        {
            get { return mRecipeParameterList; }
            set { mRecipeParameterList = value; }
        }

        #endregion

        #region Consturctor

        public RecParameterValidation(
            string fwEquipmentID,
            string equipmentID,
            string ppID,
            List<RecipeParameter> recipeParam
        )
        {
            mFWEquipmentID = fwEquipmentID;
            mEquipmentID = equipmentID;
            mPPID = ppID;
            mRecipeParameterList = recipeParam;

            CompileData();
        }

        #endregion

        #region Public method

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentID, mFWEquipmentID.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentID, mEquipmentID.GetType());
            AddBasicData("PPID", mPPID, mPPID.GetType());
            AddBasicData("RECIPEPARAMETERLIST", mRecipeParameterList, mRecipeParameterList?.GetType());
        }

        #endregion
    }
}
