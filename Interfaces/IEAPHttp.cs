using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BaseInterfaces
{
    using BridgeMessage;

    public interface IEAPHttp
    {
        /// <summary>
        /// To view the http page of the driver
        /// </summary>
        /// <param name="bridgeMessage">Bridge message</param>
        /// <returns>HTTP in string format</returns>
        string GetHTTP(IBridgeMessage bridgeMessage);
    }
}
