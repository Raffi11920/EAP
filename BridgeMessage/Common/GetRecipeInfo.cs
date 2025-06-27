using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class GetRecipeInfo : BridgeMessagePacket
    {
        private string mFWEquipmentID;
        private string mEquipmentID;
        private string mPackage;
        private string mLot;
        private string mAttribute;

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

        public string Package
        {
            get
            {
                return this.mPackage;
            }
            set
            {
                this.mPackage = value;
            }
        }

        public string Lot
        {
            get
            {
                return this.mLot;
            }
        }

        public string Attribute
        {
            get
            {
                return this.mAttribute;
            }
        }

        public GetRecipeInfo(string fwEquipmentId, string equipmentId, string lot, string attribute)
        {
            this.mFWEquipmentID = fwEquipmentId;
            this.mEquipmentID = equipmentId;
            this.mLot = lot;
            this.mAttribute = attribute;
            CompileData();
        }

        public override void CompileData()
        {
            this.AddBasicData("FWEQUIPMENTID", (object)this.mFWEquipmentID, typeof(string));
            this.AddBasicData("EQUIPMENTID", (object)this.mEquipmentID, typeof(string));
            this.AddBasicData("LOT", (object)this.mLot, typeof(string));
            this.AddBasicData("ATTRIBUTE", (object)this.mAttribute, typeof(string));
        }
    }
}
