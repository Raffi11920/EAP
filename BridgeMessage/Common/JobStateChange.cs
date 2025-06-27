using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class JobStateChange : JobCreate
    {
        #region Private Field

        private string mCurrentJobState;
        private string mPreviousJobState;
        private bool mJCProcessed;

        #endregion

        #region Properties

        public string CurrentJobState
        {
            get { return mCurrentJobState; }
        }

        public string PreviousJobState
        {
            get { return mPreviousJobState; }
        }

        //public bool JCProcessed
        //{
        //    get { return mJCProcessed; }
        //    set { mJCProcessed = value; }
        //}

        #endregion

        #region Constructor

        public JobStateChange(
            string fwEquipmentId, string equipmentId, string jobId, 
            string device, string package, string lot, 
            string ppid, string currentJobState, string previousJobState) :
            base(fwEquipmentId, equipmentId, jobId, device, package, lot, ppid, string.Empty)
        {
            mCurrentJobState = currentJobState;
            mPreviousJobState = previousJobState;

            CompileData();
        }

        #endregion

        #region Public Method

        public new void CompileData()
        {
            AddBasicData("CURRENTJOBSTATE", mCurrentJobState, mCurrentJobState.GetType());
            AddBasicData("PREVIOUSJOBSTATE", mPreviousJobState, mPreviousJobState.GetType());
        }

        #endregion

        #region Protected Method

        protected new void AssignData()
        {
            base.AssignData();

            mCurrentJobState = GetBasicData("CURRENTJOBSTATE").Value.ToString();
            mPreviousJobState = GetBasicData("PREVIOUSJOBSTATE").Value.ToString();
        }

        #endregion
    }
}
