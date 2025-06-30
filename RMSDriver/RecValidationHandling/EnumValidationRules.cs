using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.RMSDriver.RecValidationHandling
{
    public enum EnumValidationRules
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
}
