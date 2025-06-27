using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage
{
    using Utilities.ExtensionPlug;

    public abstract class BridgeMessagePacket : IBridgeMessage
    {
        #region Private Field

        private IEnumerable<BasicData> mData;
        private IEnumerable<BasicData> mReplyData;

        #endregion

        #region Properties

        public string MessageID { get; set; }

        public string Subject { get; set; }

        public string Sender { get; set; }

        public string PassBySender { get; set; }

        public string Destination { get; set; }

        public bool IsReply { get; set; }

        public int ErrorCode { get; set; }

        public string ErrorText { get; set; }

        public BridgeMessageEnum.MessageType MessageType { get; set; }

        public IEnumerable<BasicData> Data
        {
            get
            {
                return mData;
            }
        }

        public IEnumerable<BasicData> ReplyData
        {
            get
            {
                return mReplyData;
            }
        }

        #endregion

        #region Constructor


        #endregion

        #region Public Method

        public void AddBasicData(string name, object value, Type dataType)
        {
            var basicData = new BasicData(name, value, dataType);

            if (mData == null)
                mData = new List<BasicData>();

            mData = mData.Append(basicData);
        }

        public void AddReplyBasicData(string name, object value, Type dataType)
        {
            var basicData = new BasicData(name, value, dataType);

            if (mReplyData == null)
                mReplyData = new List<BasicData>();

            mReplyData = mReplyData.Append(basicData);
        }

        public BasicData GetBasicData(string name)
        {
            var result =
                from a in mData
                where a.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                select a;

            return result.FirstOrDefault();
        }

        public BasicData GetReplyData(string name)
        {
            var result =
                from a in mReplyData
                where a.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                select a;

            return result.First();
        }

        public virtual void CompileData() { }
        public virtual void CompileReplyData() { }
        protected virtual void AssignData() { }
        protected virtual void AssignReplyData() { }

        public virtual void CopyData(IBridgeMessage message)
        {
            mData = message.Data;
            MessageID = message.MessageID;
            AssignData();
        }

        public virtual void CopyReplyData(IBridgeMessage message)
        {
            mReplyData = message.ReplyData;
            AssignReplyData();
        }

        #endregion

        #region Protected Method

        protected void ClearData()
        {
            mData = new List<BasicData>();
        }

        #endregion
    }
}
