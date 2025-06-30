using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.RMSDriver.RecValidationHandling
{
    public struct ValidationText
    {
        public const string RANGE = "The value \"{0}\" is not in range of \"{1}\" and \"{2}\".";
        public const string MORETHAN = "The value \"{0}\" is more than the validation rules value \"{1}\".";
        public const string LESSTHAN = "The value \"{0}\" is less than the validation rules value \"{1}\".";
        public const string NOTEQUAL = "The value \"{0}\" is not equal to the validation rules value \"{1}\".";
        public const string EQUAL = "The value \"{0}\" is equal to the validation rules value \"{1}\".";
        public const string NOTCONTAIN = "The value \"{0}\" does not contain validation rules value \"{1}\".";
        public const string CONTAIN = "The value \"{0}\" contain in the validation rules value \"{1}\".";
        public const string OK = "OK";
    }
}
