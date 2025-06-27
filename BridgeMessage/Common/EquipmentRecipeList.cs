using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class EquipmentRecipeList : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentID;
        private string mEquipmentID;
        private SimpleItem mRecipeItems;
        private string[] mRecipes;

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

        public string[] Recipes
        {
            get { return mRecipes; }
        }

        #endregion

        #region Constructor

        public EquipmentRecipeList()
        {

        }

        public EquipmentRecipeList(string fwEquipmentID, string equipmentID, string[] recipes)
        {
            mFWEquipmentID = fwEquipmentID;
            mEquipmentID = equipmentID;
            mRecipes = recipes;

            CompileData();
        }

        #endregion

        #region Public Method

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentID, mFWEquipmentID.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentID, mEquipmentID.GetType());
            AddBasicData("RECIPES", mRecipes, typeof(string[]));
        }

        #endregion

        #region Protected Method

        protected override void AssignData()
        {
            mFWEquipmentID = GetBasicData("FWEQUIPMENTID").Value.ToString();
            mEquipmentID = GetBasicData("EQUIPMENTID").Value.ToString();

            mRecipeItems = GetBasicData("RECIPEITEMS").Value as SimpleItem;
        }

        #endregion

        #region Private Method

        private string[] ConvertToRecipeList(SimpleItem recipeItems)
        {
            var recipes = new string[recipeItems.Childs.Length];
                
            for (int i = 0; i < recipeItems.Childs.Length; i++)
            {
                recipes[i] = recipeItems.Childs[i].Value.ToString();
            }

            return recipes;
        }

        #endregion
    }
}
