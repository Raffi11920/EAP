using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.RMSDriver.Model
{
    using Base.BridgeMessage;
    using RecValidationHandling;

    internal sealed class RecipeParameter
    {
        #region Public Field

        public string FWEquipmentID { get; set; }
        public string EquipmentID { get; set; }
        public string PPID { get; set; }
        public string Command { get; set; }
        public List<Parameters> ParamList { get; set; }

        #endregion

        #region Custom Class

        public class Parameters
        {
            public string Name { get; set; }
            public string DataType { get; set; }
            public string Value { get; set; }
            public EnumValidationRules ValidationRule { get; set; }
            public string MinValue { get; set; }
            public string MaxValue { get; set; }
            public string ParameterErrorText { get; set; }
        }

        #endregion

        #region Public Method


        #endregion
    }
}
