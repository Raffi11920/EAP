using Qynix.EAP.Utilities.ExtensionPlug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SOAPRequestDriver.Services
{
    public class DeleteDefectService : SOAPServiceBase
    {
        #region Private field
        private SOAPRequest mParent;
        #endregion

        #region Constructor
        public DeleteDefectService(Helper helper, Logger logger, Dictionary<string, string> webServicePathDict, SOAPRequest parent) : base(helper, logger, webServicePathDict)
        {
            mParent = parent;
        }
        #endregion

        #region Public Method
        public bool DeleteDefect(string deleteSoapInput)
        {
            try
            {
                string EMapResult = RequestService("DeleteDefect", deleteSoapInput);

                Logger.LogHelper.LogDebug("soapResult={0}".FillArguments(EMapResult));

                if (EMapResult.Contains("PASS"))
                {
                    Logger.LogHelper.LogDebug("Delete Defect successfully!");
                    return true;
                }
                else
                {
                    Logger.LogHelper.LogDebug("Delete Defect failed!");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
                return false;
            }
        }
        #endregion
    }
}
