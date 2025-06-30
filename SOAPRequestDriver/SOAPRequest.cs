using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Xml;
using System.Threading;

namespace Qynix.EAP.Drivers.SOAPRequestDriver
{
    using Base.BaseInterfaces;
    using Base.BridgeMessage;
    using Base.BridgeMessage.Common;
    using Base.ModelBase;
    using SECS.SECSFramework.EventManager;
    using EAPMessage.Receive;
    using Objects;
    using Services;
    using Utilities.ExtensionPlug;
    using Generic;
    using Qynix.EAP.Utilities.FileUtilities;

    public class SOAPRequest : IEAPDriver
    {
        #region Private Field

        private string mEquipmentId;
        private string mFWEquipmentId;

        private IEAPDriver mParent;
        private AutoResetEvent mWaitStripReply;
        private Dictionary<string, string> mWebServicePathDict;
        private InsertDataService mInsertDataService;
        private Helper mHelper;
        private Logger mLogger;
        private JobStateChange mJobState;

        private List<StripInfo> mStripInfoList;
        private List<WaitHandleBase> mWaitHandleList;

        private Dictionary<string, string> mBadBinDict;
        private string mDriveUsername;
        private string mDrivePassword;
        private string mDriveMapPath;
        private string mDrivePathValue;
        private string mDriveUploadPath;
        #endregion

        #region Properties

        protected string EquipmentID
        {
            get { return mEquipmentId; }
        }

        protected string FWEquipmentID
        {
            get { return mFWEquipmentId; }
        }

        protected List<StripInfo> StripInfoList
        {
            get
            {
                return mStripInfoList;
            }
        }

        protected Helper Helper
        {
            get
            {
                return mHelper;
            }
        }

        protected Logger Logger
        {
            get
            {
                return mLogger;
            }
        }

        protected JobStateChange JobStateObject
        {
            get
            {
                return mJobState;
            }
        }

        protected Dictionary<string, string> WebServicePathDict
        {
            get
            {
                return mWebServicePathDict;
            }
        }

        protected Dictionary<string, string> badBinDict
        {
            get
            {
                return mBadBinDict;
            }
        }

        protected InsertDataService InsertDataService
        {
            get
            {
                return NewInsertDataService();
            }
        }

        public string DriveMapPath
        {
            get
            {
                return mDriveMapPath;
            }
        }

        public string DriveUploadPath
        {
            get
            {
                return mDriveUploadPath;
            }
        }

        #endregion

        #region Constructor

        public SOAPRequest()
        {
            mWebServicePathDict = new Dictionary<string, string>();
            //mStripMapList = new List<RequestStripMap>();
            mStripInfoList = new List<StripInfo>();
            mWaitHandleList = new List<WaitHandleBase>();
            mWaitStripReply = new AutoResetEvent(false);
            mHelper = new Helper();
            mLogger = new Logger();
        }

        #endregion

        #region Event Handler

        protected void InsertDataService_OnMapInsertFail(object sender, EventManager.EventArguments.ServiceArgs e)
        {
            try
            {
                if (e.NotifyUser) { }

                var insertDataService = sender as InsertDataService;
                insertDataService.OnMapInserted -= InsertDataService_OnMapInserted;
                InsertDataService.OnMapInsertFail -= InsertDataService_OnMapInsertFail;
                insertDataService = null;
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
            }
        }

        protected void InsertDataService_OnMapInserted(object sender, EventManager.EventArguments.ServiceArgs e)
        {
            try
            {
                var insertDataService = sender as InsertDataService;
                insertDataService.OnMapInserted -= InsertDataService_OnMapInserted;
                InsertDataService.OnMapInsertFail -= InsertDataService_OnMapInsertFail;
                insertDataService = null;
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
            }
        }

        protected void RequestStripMapService_OnMapRetrieved(object sender, EventManager.EventArguments.ServiceArgs e)
        {
            try
            {
                var targetStrip = (from a in mStripInfoList
                                   where a.FWEquipmentID == e.FWEquipmentID && a.EquipmentID == e.EquipmentID
                                   select a).FirstOrDefault();
                var requestStripMap = e.RequestStripMap;

                if (targetStrip.Package == "NA")
                {
                    requestStripMap.ReplyMessage.Result = true;
                    requestStripMap.CompileReplyData();
                    mParent.ReceiveMessage(requestStripMap);
                }

                else
                {
                    if (!e.MapFile.Replace(" ","").StartsWith(targetStrip.Package))
                    {
                        requestStripMap.ReplyMessage.Result = false;
                        requestStripMap.CompileReplyData();
                        mParent.ReceiveMessage(requestStripMap);

                        var equipmentTerminalMessage =
                                       new EquipmentTerminalDisplay
                                       (
                                           e.FWEquipmentID,
                                           e.EquipmentID,
                                           "Map file not matched, LotStart MapFile={0}, RetrieveMapFile={1}".FillArguments(targetStrip.Package, e.MapFile)
                                       );

                        equipmentTerminalMessage.Subject = "Equipment.TerminalDisplay";
                        equipmentTerminalMessage.Sender = "SOAPRequest";
                        equipmentTerminalMessage.Destination = "SECSDriver";

                        mParent.ReceiveMessage(equipmentTerminalMessage);

                        var equipmentRemoteCommand =
                            new RemoteCommand
                            (
                                e.FWEquipmentID,
                                e.EquipmentID,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                "Stop",
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                false
                            );

                        equipmentRemoteCommand.Subject = "Equipment.RemoteCommand";
                        equipmentRemoteCommand.Sender = "SOAPRequest";
                        equipmentRemoteCommand.Destination = "SECSDriver";
                        equipmentRemoteCommand.AddBasicData("REPLYREQUIRED", false, typeof(bool));
                        mParent.ReceiveMessage(equipmentRemoteCommand);

                        return;
                    }
                    else
                    {
                        requestStripMap.CompileReplyData();
                        mParent.ReceiveMessage(requestStripMap);
                    }
                }

                var requestStripService = sender as RequestStripMapService;
                requestStripService.OnMapRetrieved -= RequestStripMapService_OnMapRetrieved;
                requestStripService = null;
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
            }
        }

        #endregion

        #region IEAPDriver Method

        public int AssignParent(IBridgeMessage bridgeMessage, object parent)
        {
            mParent = parent as IEAPDriver;

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
            var receive = new InitializationReceive();
            receive.CopyData(bridgeMessage);

            mFWEquipmentId = receive.FWEquipmentName;
            mEquipmentId = receive.EquipmentName;
            var eapConfigFolder = receive.EAPConfigFolder;
            var configFile = Helper.GetConfigurationFile(eapConfigFolder, mEquipmentId);

            if (!Helper.LoadConfiguration(configFile))
            {
                return EAPError.UNHANDLED_ERR;
            }

            Logger.Initialize(Helper.Configuration.Logger);
            Logger.LogHelper.LogInfo("Initializing SOAPRequest Driver...");

            if (!string.IsNullOrEmpty(Helper.Configuration.Setting.DefectCodePrefixType))
            {
                try
                {

                }
                catch
                {

                }
            }

            if (Helper.Configuration.Setting.DefectCodeMapping != null)
            {
                var resultMapping = from a in Helper.Configuration.Setting.DefectCodeMapping
                                    select new KeyValuePair<string, string>(a.DefectCodeMachine, a.DefectCodeMapped);

                mBadBinDict = resultMapping.ToDictionary(x => x.Key, x => x.Value);
            }

            if (Helper.Configuration.Setting.SharedDrivePath != null)
            {
                try
                {
                    var sharedDrive = Helper.Configuration.Setting.SharedDrivePath.First();
                    mDriveUsername = sharedDrive.Username.ToString();
                    mDrivePassword = sharedDrive.Password.ToString();
                    mDrivePathValue = sharedDrive.PathValue.ToString();
                    mDriveMapPath = sharedDrive.MapPath.ToString();
                    mDriveUploadPath = sharedDrive.UploadPath.ToString();
                    StartMappingSharedDrive();
                }
                catch
                {
                    Logger.LogHelper.LogError("Shared Drive Mapping Failed.");
                }
            }
            var result = from a in Helper.Configuration.WebServices.WebService
                         select new KeyValuePair<string, string>(a.ServiceName, Path.Combine(eapConfigFolder, Helper.Configuration.WebServices.WebServicesDir, a.XMLFILE));

            mWebServicePathDict = result.ToDictionary(x => x.Key, x => x.Value);

            //mInsertDataService = new InsertDataService(mHelper, mLogger, mWebServicePathDict, this);
            //mInsertDataService.OnMapInsertFail += InsertDataService_OnMapInsertFail;

            //mRequestStripMapService = new RequestStripMapService(mWebServicePathDict, this);
            //mRequestStripMapService.OnMapRetrieved += RequestStripMapService_OnMapRetrieved;

            Logger.LogHelper.LogInfo("SOAPRequest Driver initialization complete.");

            return EAPError.OK;
        }

        public int ReceiveMessage(IBridgeMessage bridgeMessage)
        {
            try
            {
                Logger.LogHelper.LogInfo("Received Sender:{0}, Destination:{1}, Subject:{2}".FillArguments(bridgeMessage.Sender, bridgeMessage.Destination, bridgeMessage.Subject));
                string subject = bridgeMessage.Subject;

                switch (subject)
                {
                    case "RetrieveStripMap":
                        return HandleRequestStripMap(bridgeMessage);

                    case "Equipment.Event":
                        return HandleEquipmentEvent(bridgeMessage);

                    case "MES.GetRecipeInfo.Reply":
                        //var waitHandle = mWaitHandleList.FirstOrDefault(x => x.UID == bridgeMessage.MessageID);

                        var repliedGetRecipeInfo = bridgeMessage as GetRecipeInfo;

                        var strip = mStripInfoList.FirstOrDefault(x => x.StripID == repliedGetRecipeInfo.MessageID);

                        switch (repliedGetRecipeInfo.Attribute.ToEnum<EnumType.Attribute>())
                        {
                            case EnumType.Attribute.PackageName:
                                Logger.LogHelper.LogDebug("MES Replied package: {0}".FillArguments(repliedGetRecipeInfo.Package));
                                strip.Package = repliedGetRecipeInfo.Package;
                                break;
                        }
                        break;

                    case "JobStateChange":
                        var jobstate = bridgeMessage as JobStateChange;
                        Logger.LogHelper.LogDebug("Job state received: {0}, {1}".FillArguments(jobstate.Lot, jobstate.CurrentJobState));

                        if (jobstate.CurrentJobState == ConstantBank.JobState.Selected.ToString())
                        {
                            mJobState = jobstate;
                            Logger.LogHelper.LogDebug("Job state object created: {0}, {1}".FillArguments(mJobState.Lot, mJobState.CurrentJobState));
                        }
                        break;
                }

                return EAPError.OK;
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
                return EAPError.UNKNOWN_ERR;
            }
        }

        #endregion

        #region Protected Method

        protected virtual int HandleRequestStripMap(IBridgeMessage bridgeMessage)
        {
            try
            {
                RequestStripMap requestStripMap = mHelper.GetRequestStripMap(bridgeMessage);
                RequestStripMapService requestStripMapService = new RequestStripMapService(mHelper, mLogger, mWebServicePathDict, this);
                requestStripMapService.OnMapRetrieved += RequestStripMapService_OnMapRetrieved;

                var stripInfo = getStrip(requestStripMap);

                if (stripInfo == null)
                {
                    Logger.LogHelper.LogDebug("Missing lot start info for FWEquipment={0}, Equipment={1}...".FillArguments(requestStripMap.FWEquipmentID, (object)requestStripMap.EquipmentID));
                    requestStripMap.ReplyMessage = new RequestStripMap.Reply();
                    requestStripMap.ReplyMessage.Result = false;
                    requestStripMap.ReplyMessage.StripID = requestStripMap.StripID;
                    requestStripMap.CompileReplyData();
                    SendMessage(requestStripMap);
                    SendTerminalMessage(requestStripMap.FWEquipmentID, requestStripMap.EquipmentID, "Fail to retrieve Strip Map, missing Lot Start info, StripID={0}", requestStripMap.StripID);
                    SendStopCommand(requestStripMap.FWEquipmentID, requestStripMap.EquipmentID);
                    return EAPError.STRIP_NOT_FOUND;
                }

                stripInfo.StripID = requestStripMap.StripID;
                stripInfo.DefectCode = null;

                string lotId = string.Empty;
                if (!new RequestStripMapService(mHelper, mLogger, mWebServicePathDict, this).IsStripMapWithCorrectLotId(stripInfo.LotID, requestStripMap.StripID, ref lotId))
                {
                    Logger.LogHelper.LogDebug("Another lot Id already exist for strip {0}".FillArguments(requestStripMap.StripID), "HandleRequestStripMap");
                    requestStripMap.ReplyMessage = new RequestStripMap.Reply();
                    requestStripMap.ReplyMessage.Result = false;
                    requestStripMap.ReplyMessage.StripID = requestStripMap.StripID;
                    requestStripMap.CompileReplyData();
                    SendMessage(requestStripMap);
                    SendTerminalMessage(requestStripMap.FWEquipmentID, requestStripMap.EquipmentID, "Strip {0} belong to lot {1}", requestStripMap.StripID, lotId);
                    SendStopCommand(requestStripMap.FWEquipmentID, requestStripMap.EquipmentID);
                    return EAPError.STRIP_NOT_FOUND;
                }

                if (!new RequestStripMapService(mHelper, mLogger, mWebServicePathDict, this).IsStripMapExist(requestStripMap.StripID))
                {
                    Logger.LogHelper.LogDebug("StripID, {0} not exist, perform insert strip map...".FillArguments(requestStripMap.StripID), "HandleRequestStripMap");

                    mInsertDataService = new InsertDataService(mHelper, mLogger, mWebServicePathDict, this);
                    mInsertDataService.OnMapInsertFail += InsertDataService_OnMapInsertFail;
                    mInsertDataService.OnMapInserted += InsertDataService_OnMapInserted;

                    if (!mInsertDataService.InsertStripMap(stripInfo, true))
                    {
                        requestStripMap.ReplyMessage = new RequestStripMap.Reply();
                        requestStripMap.ReplyMessage.Result = false;
                        requestStripMap.ReplyMessage.StripID = requestStripMap.StripID;
                        requestStripMap.CompileReplyData();
                        SendMessage(requestStripMap);
                        //SendTerminalMessage(requestStripMap.FWEquipmentID, requestStripMap.EquipmentID, "Fail to insert Strip Map, StripID={0}", requestStripMap.StripID);
                        SendMapNotExistTerminalMessage(requestStripMap.FWEquipmentID, requestStripMap.EquipmentID, requestStripMap.StripID);
                        SendStopCommand(requestStripMap.FWEquipmentID, requestStripMap.EquipmentID);
                        return EAPError.STRIP_NOT_FOUND;
                    }
                }
                else
                {
                    //requestStripMap.ReplyMessage.EMapLoc = RetrieveDefectLocation(requestStripMap.StripID);
                }

                requestStripMapService.PerformRetrieveStripMapAndReply(requestStripMap);
                return 0;
            }
            catch (Exception ex)
            {
                this.Logger.LogHelper.LogException(ex, "HandleRequestStripMap");
                return EAPError.UNKNOWN_ERR;
            }
        }

        protected virtual int HandleRequestStripMap2(IBridgeMessage bridgeMessage)
        {
            try
            {
                RequestStripMap requestStripMap = mHelper.GetRequestStripMap(bridgeMessage);
                RequestStripMapService requestStripMapService = new RequestStripMapService(mHelper, mLogger, mWebServicePathDict, this);
                requestStripMapService.OnMapRetrieved += RequestStripMapService_OnMapRetrieved;

                var stripInfo = (from a in mStripInfoList
                                 where a.FWEquipmentID == requestStripMap.FWEquipmentID &&
                                       a.EquipmentID == requestStripMap.EquipmentID
                                 select a).FirstOrDefault();

                if (stripInfo == null)
                {
                    Logger.LogHelper.LogDebug("Missing lot start info for FWEquipment={0}, Equipment={1}...".FillArguments(requestStripMap.FWEquipmentID, (object)requestStripMap.EquipmentID));
                    requestStripMap.ReplyMessage = new RequestStripMap.Reply();
                    requestStripMap.ReplyMessage.Result = false;
                    requestStripMap.ReplyMessage.StripID = requestStripMap.StripID;
                    requestStripMap.CompileReplyData();
                    SendMessage(requestStripMap);
                    SendTerminalMessage(requestStripMap.FWEquipmentID, requestStripMap.EquipmentID, "Fail to retrieve Strip Map, missing Lot Start info, StripID={0}", requestStripMap.StripID);
                    SendStopCommand(requestStripMap.FWEquipmentID, requestStripMap.EquipmentID);
                    return EAPError.STRIP_NOT_FOUND;
                }

                stripInfo.StripID = requestStripMap.StripID;
                stripInfo.DefectCode = null;

                string lotId = string.Empty;
                if (!new RequestStripMapService(mHelper, mLogger, mWebServicePathDict, this).IsStripMapWithCorrectLotId(stripInfo.LotID, requestStripMap.StripID, ref lotId))
                {
                    Logger.LogHelper.LogDebug("Another lot Id already exist for strip {0}".FillArguments(requestStripMap.StripID), "HandleRequestStripMap");
                    requestStripMap.ReplyMessage = new RequestStripMap.Reply();
                    requestStripMap.ReplyMessage.Result = false;
                    requestStripMap.ReplyMessage.StripID = requestStripMap.StripID;
                    requestStripMap.CompileReplyData();
                    SendMessage(requestStripMap);
                    SendTerminalMessage(requestStripMap.FWEquipmentID, requestStripMap.EquipmentID, "Strip {0} belong to lot {1}", requestStripMap.StripID, lotId);
                    SendStopCommand(requestStripMap.FWEquipmentID, requestStripMap.EquipmentID);
                    return EAPError.STRIP_NOT_FOUND;
                }

                if (!new RequestStripMapService(mHelper, mLogger, mWebServicePathDict, this).IsStripMapExist(requestStripMap.StripID))
                {
                    Logger.LogHelper.LogDebug("StripID, {0} not exist, perform insert strip map...".FillArguments(requestStripMap.StripID), "HandleRequestStripMap");

                    mInsertDataService = new InsertDataService(mHelper, mLogger, mWebServicePathDict, this);
                    mInsertDataService.OnMapInsertFail += InsertDataService_OnMapInsertFail;
                    mInsertDataService.OnMapInserted += InsertDataService_OnMapInserted;

                    if (!mInsertDataService.InsertStripMap(stripInfo, true))
                    {
                        requestStripMap.ReplyMessage = new RequestStripMap.Reply();
                        requestStripMap.ReplyMessage.Result = false;
                        requestStripMap.ReplyMessage.StripID = requestStripMap.StripID;
                        requestStripMap.CompileReplyData();
                        SendMessage(requestStripMap);
                        //SendTerminalMessage(requestStripMap.FWEquipmentID, requestStripMap.EquipmentID, "Fail to insert Strip Map, StripID={0}", requestStripMap.StripID);
                        SendMapNotExistTerminalMessage(requestStripMap.FWEquipmentID, requestStripMap.EquipmentID, requestStripMap.StripID);
                        SendStopCommand(requestStripMap.FWEquipmentID, requestStripMap.EquipmentID);
                        return EAPError.STRIP_NOT_FOUND;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                this.Logger.LogHelper.LogException(ex, "HandleRequestStripMap");
                return EAPError.UNKNOWN_ERR;
            }
        }

        protected virtual int HandleEquipmentEvent(IBridgeMessage bridgeMessage)
        {
            return EAPError.METHOD_NOT_IMPLEMENTED;
        }

        protected virtual void SendMapNotExistTerminalMessage(string fwEquipmentId, string equipmentId, string StripID)
        {
            SendTerminalMessage(fwEquipmentId, equipmentId, "Fail to insert Strip Map, StripID={0}", StripID);
        }

        protected virtual void SendTerminalMessage(string fwEquipmentId, string equipmentId, string message, params string[] args)
        {
            try
            {
                EquipmentTerminalDisplay equipmentTerminalDisplay = new EquipmentTerminalDisplay(fwEquipmentId, equipmentId, message.FillArguments(args));
                equipmentTerminalDisplay.Subject = "Equipment.TerminalDisplay";
                equipmentTerminalDisplay.Sender = "SOAPRequest";
                equipmentTerminalDisplay.Destination = "SECSDriver";
                mParent.ReceiveMessage(equipmentTerminalDisplay);
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex, "SendTerminalMessage", "C:\\EAP\\SOAPRequestDriver\\SOAPRequest.cs");
            }
        }

        protected virtual void SendTerminalMessageMulti(string fwEquipmentId, string equipmentId, string[] message, params string[] args)
        {
            try
            {
                EquipmentTerminalDisplayMulti equipmentTerminalDisplayMulti = new EquipmentTerminalDisplayMulti(fwEquipmentId, equipmentId, message);
                equipmentTerminalDisplayMulti.Subject = "Equipment.TerminalDisplayMulti";
                equipmentTerminalDisplayMulti.Sender = "SOAPRequest";
                equipmentTerminalDisplayMulti.Destination = "SECSDriver";
                mParent.ReceiveMessage(equipmentTerminalDisplayMulti);
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex, "SendTerminalMessageMulti", "C:\\EAP\\SOAPRequestDriver\\SOAPRequest.cs");
            }
        }

        protected virtual void SendStopCommand(string fwEquipmentId, string equipmentId)
        {
            try
            {
                RemoteCommand remoteCommand = new RemoteCommand(fwEquipmentId, equipmentId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "Stop", string.Empty, string.Empty, string.Empty, string.Empty, false);
                remoteCommand.Subject = "Equipment.RemoteCommand";//
                remoteCommand.Sender = "SOAPRequest";
                remoteCommand.Destination = "SECSDriver";
                remoteCommand.AddBasicData("REPLYREQUIRED", false, typeof(bool));
                SendMessage(remoteCommand);
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
            }
        }
        protected virtual void SendInhibitAutoCommand(string fwEquipmentId, string equipmentId)
        {
            try
            {
                RemoteCommand remoteCommand = new RemoteCommand(fwEquipmentId, equipmentId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "InhibitAuto", string.Empty, string.Empty, string.Empty, string.Empty, false);
                remoteCommand.Subject = "Equipment.RemoteCommand";//
                remoteCommand.Sender = "SOAPRequest";
                remoteCommand.Destination = "SECSDriver";
                remoteCommand.AddBasicData("REPLYREQUIRED", false, typeof(bool));
                SendMessage(remoteCommand);
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
            }
        }

        protected virtual void SendReleaseInhibitAutoCommand(string fwEquipmentId, string equipmentId)
        {
            try
            {
                RemoteCommand remoteCommand = new RemoteCommand(fwEquipmentId, equipmentId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "ReleaseInhibitAuto", string.Empty, string.Empty, string.Empty, string.Empty, false);
                remoteCommand.Subject = "Equipment.RemoteCommand";//
                remoteCommand.Sender = "SOAPRequest";
                remoteCommand.Destination = "SECSDriver";
                remoteCommand.AddBasicData("REPLYREQUIRED", false, typeof(bool));
                SendMessage(remoteCommand);
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
            }
        }

        protected virtual void SendPauseCommand(string fwEquipmentId, string equipmentId)
        {
            try
            {
                RemoteCommand remoteCommand = new RemoteCommand(fwEquipmentId, equipmentId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "Pause", string.Empty, string.Empty, string.Empty, string.Empty, false);
                remoteCommand.Subject = "Equipment.RemoteCommand";//
                remoteCommand.Sender = "SOAPRequest";
                remoteCommand.Destination = "SECSDriver";
                remoteCommand.AddBasicData("REPLYREQUIRED", false, typeof(bool));
                SendMessage(remoteCommand);
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
            }
        }

        protected virtual void SendStartCommand(string fwEquipmentId, string equipmentId)
        {
            try
            {
                RemoteCommand remoteCommand = new RemoteCommand(fwEquipmentId, equipmentId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "StartJob", string.Empty, string.Empty, string.Empty, string.Empty, false);
                remoteCommand.Subject = "Equipment.RemoteCommand";//
                remoteCommand.Sender = "SOAPRequest";
                remoteCommand.Destination = "SECSDriver";
                remoteCommand.AddBasicData("REPLYREQUIRED", false, typeof(bool));
                SendMessage(remoteCommand);
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
            }
        }

        protected void RequestRecipeInfo(string lotId, string stripId, string reqAttr)
        {
            try
            {
                var getRecipeInfo = new GetRecipeInfo(mFWEquipmentId, mEquipmentId, lotId, reqAttr);
                getRecipeInfo.MessageID = stripId;
                getRecipeInfo.Subject = "MES.GetRecipeInfo";
                getRecipeInfo.Sender = "SOAPRequestDriver";
                getRecipeInfo.Destination = "NATSCommunicator";

                SendMessage(getRecipeInfo);
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
            }
        }

        protected virtual FW_TrackIn_RMSWebService.TrackIn RMS_TrackIn(string lotId, string eqId, string userId)
        {
            FW_TrackIn_RMSWebService.FW_TrackIn_RMSWebServiceSoapClient rmsAPI = new FW_TrackIn_RMSWebService.FW_TrackIn_RMSWebServiceSoapClient();
            var rmsResult = rmsAPI.RMSTrackIn(lotId, eqId, userId); //RMS track in

            return rmsResult;
        }


        protected virtual void StartMappingSharedDrive()
        {
            var networkDrive = new NetworkDrive();

            networkDrive.LocalDrive = mDriveMapPath;
            networkDrive.Persistent = true;
            networkDrive.SaveCredentials = true;
            networkDrive.ShareName = mDrivePathValue;
            try
            {
                networkDrive.MapDrive(mDriveUsername, mDrivePassword);
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogError("Map Drive Failed.");
                Logger.LogHelper.LogException(ex);
            }
            string text;
            if (Directory.Exists(mDriveMapPath))
            {
                Logger.LogHelper.LogDebug("{0} Shared drive exist, continue create destination folder.".FillArguments(mDriveMapPath));
                FileHelper.CreateDirIfNotExist(mDriveMapPath + mDriveUploadPath, out text);
                mLogger.LogHelper.LogInfo(text);
                mLogger.LogHelper.LogInfo("Shared Drive initialization done.");
            }
            else
                Logger.LogHelper.LogDebug("{0} Shared drive not exist, not able create destination folder.".FillArguments(mDriveMapPath));

        }

        protected virtual void SendStripMapToSharedDrive(string mapData, string stripId)
        {
            try
            {
                var filewrite = File.CreateText(mDriveMapPath + mDriveUploadPath + "\\" + stripId + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xml");
                filewrite.WriteLine(mapData);
                filewrite.Dispose();
                filewrite.Close();
                Logger.LogHelper.LogDebug("Write File to {0} success, strip ID = {1}.".FillArguments(mDriveMapPath + mDriveUploadPath, stripId));
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogError("Write File to {0} failed, strip ID = {1}.".FillArguments(mDriveMapPath + mDriveUploadPath, stripId));
                Logger.LogHelper.LogException(ex);
            }

        }

        protected virtual void DeleteDefect(StripInfo stripInfo)
        {
            RequestStripMapService requestStripMapService = new RequestStripMapService(mHelper, mLogger, mWebServicePathDict, this);
            var eMapDefect = requestStripMapService.RetrieveEMapLocation(stripInfo.StripID);

            if (string.IsNullOrEmpty(eMapDefect))
            {
                Logger.LogHelper.LogDebug("No Defect from EMAP for stripId={0}, skiping delete defect".FillArguments(stripInfo.StripID));
                return;
            }
            else
            {
                Logger.LogHelper.LogDebug("StripId={0}, EMAP defect location={1}".FillArguments(stripInfo.StripID, eMapDefect));
            }

            var defects =
                from a in eMapDefect.Split(';')
                select Tuple.Create(a.Split('@')[1].ToInt(), a.Split('@')[2].ToInt(), a.Split('@')[0].ToString());
            var tupleDefects = defects.ToArray();
            var tableDefects = GenerateMachineMatrix(stripInfo.DefectCode, stripInfo.Row, stripInfo.Column, stripInfo.OriginLocation);
            //convert asm like insert then only compare and delete
            var deleteDefectStringList = GenerateDeleteDefectString(tupleDefects, tableDefects, stripInfo);
            if (deleteDefectStringList.Count == 0)
            {
                Logger.LogHelper.LogDebug("No Defect become Good unit for stripId={0}, skiping delete defect".FillArguments(stripInfo.StripID));
                return;
            }
            else
            {
                //loop delete data service here
                DeleteDefectService deleteDefectService = new DeleteDefectService(mHelper, mLogger, mWebServicePathDict, this);
                foreach (string deleteDefectString in deleteDefectStringList)
                {
                    deleteDefectService.DeleteDefect(deleteDefectString);
                }
            }
        }

        protected virtual InsertDataService NewInsertDataService()
        {
            return new InsertDataService(mHelper, mLogger, mWebServicePathDict, this);
        }

        protected virtual StripInfo getStrip(RequestStripMap requestStripMap)
        {
            return (from a in mStripInfoList
                    where a.FWEquipmentID == requestStripMap.FWEquipmentID &&
                    a.EquipmentID == requestStripMap.EquipmentID
                    select a).FirstOrDefault();
        }
        #endregion

        #region Internal Method

        //internal void AddStripMap(RequestStripMap requestStripMap)
        //{
        //    mStripMapList.Add(requestStripMap);
        //}
        internal void CheckRequestStripMap(RequestStripMap requestStripMap)
        {

        }

        internal void SendMessage(RequestStripMap requestStripMap)
        {
            mParent.ReceiveMessage(requestStripMap);
        }

        internal void SendMessage(InsertStripMap requestStripMap)
        {
            mParent.ReceiveMessage(requestStripMap);
        }

        protected void SendMessage(IBridgeMessage bridgeMessage)
        {
            try
            {
                mParent.ReceiveMessage(bridgeMessage);
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex, "SendMessage", "C:\\EAP\\SOAPRequestDriver\\SOAPRequest.cs");
            }
        }

        //internal void RemoveStripMap(string stripID)
        //{
        //    var removeInfo = (from a in mStripInfoList
        //                      where a.StripID == stripID
        //                      select a).FirstOrDefault();

        //    if (removeInfo != null)
        //        mStripInfoList.Remove(removeInfo);
        //    else
        //        Logger.LogHelper.LogError("Delete saved strip map failed. Strip not found, Strip ID = {0}".FillArguments(stripID));
        //}

        internal void RemoveStripMap(string lotID)
        {
            var removeInfos = from a in mStripInfoList
                              where a.LotID == lotID
                              select a;

            if (removeInfos == null)
            {
                Logger.LogHelper.LogError("Delete saved strip map failed. Lot not found, Lot ID = {0}".FillArguments(lotID));
                return;
            }

            foreach (var info in removeInfos)
            {
                mStripInfoList.Remove(info);
            }
        }

        internal string GetMapFile(string stripID)
        {
            string mapFile = mStripInfoList.FirstOrDefault(x => x.StripID == stripID).MapFile;

            return mapFile;
        }

        #endregion

        #region Private Method
        //check defect become pass (delete)
        private List<string> GenerateDeleteDefectString(Tuple<int, int, string>[] emapDefectLoc, string[][] stripDefectLoc, StripInfo stripInfoDeleteDefect)
        {
            List<string> deleteDefectList = new List<string>();
            foreach (Tuple<int, int, string> a in emapDefectLoc)
            {
                if (stripDefectLoc[a.Item1 - 1][a.Item2 - 1] == "0")
                {
                    deleteDefectList.Add(string.Format("{0}:{1}:{2}::{3}:{4}",
                    stripInfoDeleteDefect.StripID,
                    a.Item1,
                    a.Item2,
                    stripInfoDeleteDefect.Operator,
                    DateTime.Now.ToString("yyyyMMddHHmmss")));
                }
            }
            return deleteDefectList;
        }

        private string[][] GenerateMachineMatrix(string[] defectCodeArray, int row, int column, int originLocation)
        {
            try
            {
                var matrix = new string[row][];

                for (int i = 0; i < row; i++)
                {
                    matrix[i] = new string[column];
                    for (int j = 0; j < column; j++)
                    {
                        switch (originLocation)
                        {
                            case 1:
                                matrix[i][j] = defectCodeArray[(i + 1) * column - j - 1];
                                break;

                            case 2:
                                matrix[i][j] = defectCodeArray[i * column + j];
                                break;

                            case 3:
                                matrix[i][j] = defectCodeArray[(row - i - 1) * column + j];
                                break;

                            case 4:
                                matrix[i][j] = defectCodeArray[row * column - (i + 1) * j - 1];
                                break;
                        }
                    }
                }

                return matrix;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion
    }
}
