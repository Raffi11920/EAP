using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BaseInterfaces
{
    using BridgeMessage;

    public interface IEAPDriver : IEAPMessaging, IEAPHttp
    {
        /// <summary>
        /// This is called everytime the driver is instantiated.
        /// The driver will then read its own configuration.
        /// </summary>
        /// <param name="bridgeMessage"></param>
        /// <returns>Error Code.</returns>
        int Initialize(IBridgeMessage bridgeMessage);

        /// <summary>
        /// Assign parent module, normally is central bridge module.
        /// </summary>
        /// <param name="bridgeMessage">Addition data to be acknowledge.</param>
        /// <param name="parent">The parent who instantiate the driver.</param>
        /// <returns>Error Code.</returns>
        int AssignParent(IBridgeMessage bridgeMessage, object parent);

        /// <summary>
        /// To be called when Service is closing.
        /// This method will dispose object to clear up memory.
        /// </summary>
        void Close();
    }
}
