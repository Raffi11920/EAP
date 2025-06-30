using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Qynix.EAP.Drivers.SOAPRequestDriver.Services
{
    using Utilities.ExtensionPlug;

    public abstract class SOAPServiceBase
    {
        #region Private Field

        private string mSOAPUrl;
        private int mTimeout;
        private int mRetryCount;
        private IAsyncResult mAsynResult;
        private string mAction;
        private string mResultContent;
        private Dictionary<string, string> mWebServicePathDict;
        private AutoResetEvent mWaitWebResponse;
        private Helper mHelper;
        private Logger mLogger;

        #endregion

        #region Properties

        protected int RetryCount
        {
            get { return mRetryCount; }
            set { mRetryCount = value; }
        }

        protected int Timeout
        {
            get { return mTimeout; }
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

        public SOAPServiceBase(Helper helper, Logger logger, Dictionary<string, string> webServicePathDict)
        {
            mHelper = helper;
            mLogger = logger;
            mWaitWebResponse = new AutoResetEvent(false);
            mWebServicePathDict = webServicePathDict;
            mSOAPUrl = mHelper.Configuration.EMAPURL;
            mTimeout = mHelper.Configuration.Setting.RequestTimeout;
            mRetryCount = mHelper.Configuration.Setting.RetryCount;
        }
        
        #endregion

        #region Protected Method

        protected virtual void WaitOrTimeoutCallback(object state, bool timedOut)
        {
            var request = state as HttpWebRequest;
            string soapResult;

            if (timedOut)
            {
                if (request != null)
                {
                    request.Abort();
                }

                mResultContent = string.Empty;
            }
            else
            {
                using (WebResponse webResponse = request.EndGetResponse(mAsynResult))
                {
                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        soapResult = rd.ReadToEnd();

                        mResultContent = GetContent(soapResult, mAction);
                        Logger.LogHelper.LogInfo("EMAP Reply: {0}".FillArguments(mResultContent));
                    }
                }

            }

            mWaitWebResponse.Set();
        }

        protected string RequestService(string action, string parameter = "")
        {
            try
            {
                mAction = action;

                Logger.LogHelper.LogInfo("Action={0}, Parameter={1}".FillArguments(action, parameter));

                XmlDocument xmlEnvelop = CreateEnvelope(mAction, parameter);

                HttpWebRequest webRequest = CreateWebRequest(mSOAPUrl, mAction);

                InsertSoapEnvelopeIntoWebRequest(xmlEnvelop, webRequest);

                // begin async call to web request.
                mAsynResult = webRequest.BeginGetResponse(null, null);

                // this line implements the timeout, if there is a timeout, the callback fires and the request becomes aborted
                ThreadPool.RegisterWaitForSingleObject(mAsynResult.AsyncWaitHandle, new WaitOrTimerCallback(WaitOrTimeoutCallback), webRequest, mTimeout, true);
                mWaitWebResponse.WaitOne();

                if (string.IsNullOrEmpty(mResultContent.Trim()))
                {
                    if (mRetryCount > 0)
                    {
                        mRetryCount--;
                        RequestService(action, parameter);
                    }
                }
                return mResultContent;
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
                return null;
            }
        }

        #endregion

        #region Private Method

        private HttpWebRequest CreateWebRequest(string url, string action)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Add("SOAPAction", string.Format("http://tempuri.org/{0}", action));
            webRequest.ContentType = "text/xml; charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";

            return webRequest;
        }

        private XmlDocument CreateEnvelope(string service, string parameter = "")
        {
            XmlDocument soapEnvelop = new XmlDocument();
            var path = mWebServicePathDict[service];
            soapEnvelop.Load(path);

            switch (service)
            {
                case "RetrieveAllMapSetting":
                    break;
                case "RetrieveStripMap":
                    soapEnvelop.GetElementsByTagName("StripID", "http://tempuri.org/")[0].InnerXml = parameter;
                    break;
                case "RetrieveStripMapWithLotId":
                    soapEnvelop.GetElementsByTagName("LotID", "http://tempuri.org/")[0].InnerXml = parameter;
                    break;
                case "RetrieveEMapLoc":
                    soapEnvelop.GetElementsByTagName("StripID", "http://tempuri.org/")[0].InnerXml = parameter;
                    break;
                case "InsertData":
                    soapEnvelop.GetElementsByTagName("Parameter", "http://tempuri.org/")[0].InnerXml = parameter;
                    break;
                case "DeleteDefect":
                    soapEnvelop.GetElementsByTagName("Parameter", "http://tempuri.org/")[0].InnerXml = parameter;
                    break;
                case "RetrieveAlarm":
                    soapEnvelop.GetElementsByTagName("Parameter", "http://tempuri.org/")[0].InnerXml = parameter;
                    break;
                case "TransferSMTStrip":
                    soapEnvelop.GetElementsByTagName("Parameter", "http://tempuri.org/")[0].InnerXml = parameter;
                    break;
                case "InsertWafer":
                    soapEnvelop.GetElementsByTagName("Parameter", "http://tempuri.org/")[0].InnerXml = parameter;
                    break;
                case "GetLotIDbyStrip":
                    soapEnvelop.GetElementsByTagName("Parameter", "http://tempuri.org/")[0].InnerXml = parameter;
                    break;
                case "GetFWLotID":
                    soapEnvelop.GetElementsByTagName("Parameter", "http://tempuri.org/")[0].InnerXml = parameter;
                    break;
                case "GetMagazineTrackIn":
                    soapEnvelop.GetElementsByTagName("Parameter", "http://tempuri.org/")[0].InnerXml = parameter;
                    break;
                case "RetrieveLotIDbyStrip":
                    soapEnvelop.GetElementsByTagName("Parameter", "http://tempuri.org/")[0].InnerXml = parameter;
                    break;
                case "GetFWMachineState":
                    soapEnvelop.GetElementsByTagName("Parameter", "http://tempuri.org/")[0].InnerXml = parameter;
                    break;
                case "LogAutoTrackIn":
                    soapEnvelop.GetElementsByTagName("Parameter", "http://tempuri.org/")[0].InnerXml = parameter;
                    break;
                default:
                    soapEnvelop.GetElementsByTagName("Parameter", "http://tempuri.org/")[0].InnerXml = parameter;
                    break;
            }
            return soapEnvelop;
        }

        private void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
        }

        private string GetContent(string soapResult, string action)
        {
            if (!string.IsNullOrEmpty(soapResult))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(soapResult);
                string tag = string.Format("{0}Result", action);
                string content = doc.GetElementsByTagName(tag, "http://tempuri.org/")[0].InnerXml;

                return content;
            }
            else
            {
                return "";
            }
        }

        #endregion

    }
}
