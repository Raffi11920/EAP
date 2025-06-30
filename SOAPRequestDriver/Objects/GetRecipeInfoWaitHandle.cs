using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SOAPRequestDriver.Objects
{
    using Base.BridgeMessage.Common;
    using Base.ModelBase;

    public class GetRecipeInfoWaitHandle : WaitHandleBase
    {
        private volatile GetRecipeInfo mGetRecipeInfo;

        public GetRecipeInfo GetRecipeInfoResult
        {
            get { return mGetRecipeInfo; }
            set { mGetRecipeInfo = value; }
        }

        public GetRecipeInfoWaitHandle(string uid) : base(uid)
        {
        }

        public GetRecipeInfoWaitHandle(GetRecipeInfo getRecipeInfo) : base(getRecipeInfo.MessageID)
        {
            mGetRecipeInfo = getRecipeInfo;
        }
    }
}
