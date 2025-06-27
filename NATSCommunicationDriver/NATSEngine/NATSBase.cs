using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NATS.Client;
using System.Diagnostics;

namespace Qynix.EAP.Drivers.NATSCommunicationDriver.NATSEngine
{
    public class NATSBase : IDisposable
    {
        #region Private Field

        private string mUrl;
        private string mSubject;
        private Options mNATSConnOpts;
        private IConnection mConnection;
        private Helper mHelper;
        private Logger mLogger;
        private object mSyncRoot = new object();

        #endregion

        #region Properties

        public string Url
        {
            get { return mUrl; }
        }

        public string Subject
        {
            get { return mSubject; }
        }

        public IConnection Connection
        {
            get
            {
                return mConnection;
            }
        }

        public Options NATSConnOpts
        {
            get { return mNATSConnOpts; }
        }

        protected Helper Helper
        {
            get { return mHelper; }
        }

        protected Logger Logger
        {
            get { return mLogger; }
        }

        #endregion

        #region Constructor

        public NATSBase(string url, string subject, Helper helper, Logger logger)
        {
            mUrl = url;
            mSubject = subject;
            mHelper = helper;
            mLogger = logger;
            mConnection = CreateConnection();
        }

        #endregion

        #region Public Method

        public virtual void Dispose()
        {
            mConnection.Flush();
            mConnection.Dispose();
        }

        #endregion

        #region Protected Method

        protected IConnection CreateConnection()
        {
            return new ConnectionFactory().CreateConnection(mUrl);
        }

        #endregion

        #region Private Method



        #endregion
    }
}
