using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage
{
    public interface IErrorInfo
    {
        int ErrorCode { get; set; }
        string ErrorText { get; set; }
    }
}
