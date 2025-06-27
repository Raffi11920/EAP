using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.EAPWebDriver
{
    using Base.BaseInterfaces;
    using Base.BridgeMessage;

    class EAPWeb : IEAPDriver
    {
        #region Private Field

        private IEAPCentral mCentral;

        #endregion

        #region IEAPDriver Method

        public int AssignParent(IBridgeMessage bridgeMessage, object parent)
        {
            mCentral = parent as IEAPCentral;

            return EAPError.OK;
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public string GetHTTP(IBridgeMessage bridgeMessage)
        {
            throw new NotImplementedException();
        }

        public int Initialize(IBridgeMessage bridgeMessage)
        {
            try
            {
                return EAPError.OK;
            }
            catch (Exception ex)
            {

                return EAPError.UNKNOWN_ERR;
            }
        }

        public int ReceiveMessage(IBridgeMessage bridgeMessage)
        {
            return EAPError.OK;
        }

        #endregion
    }
}
