using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SECSDriver
{
    using Base.BaseInterfaces;
    using Base.BridgeMessage;
    using Base.BridgeMessage.Common;
    using Base.ModelBase;
    using EAPMessages.Send;
    using SECS.SECSFramework;
    using SECS.SECSFramework.Model;
    using SECSDriver.Tasks;
    using Utilities.ExtensionPlug;

    public class CSECSDriver : IEAPDriver
    {
        #region Private Field

        private IEAPDriver mParent;
        private string mEAPConfigFolder;
        private string mFWEquipmentName;
        public string mEquipmentName;

        private Helper mHelper;
        private Logger mLogger;
        private SECSEngine mSECSEngine;
        private SECSGEMActionTask mSECSGEMActionTask;
        private StateMapping mStateMap;

        private List<IBridgeMessage> mMessageList;

        #endregion

        #region Protected Field

        public SECSEngine SECSEngine
        {
            get
            {
                return mSECSEngine;
            }
            set
            {
                mSECSEngine = value;
            }
        }

        protected Logger Logger
        {
            get
            {
                return mLogger;
            }
        }
        protected Helper Helper
        {
            get
            {
                return mHelper;
            }
        }

        protected StateMapping StateMap
        {
            get
            {
                return mStateMap;
            }
        }

        protected IEAPDriver Parent
        {
            get
            {
                return mParent;
            }
            set
            {
                mParent = value;
            }
        }
        #endregion

        #region Constructor

        public CSECSDriver()
        {
            mHelper = new Helper();
            mLogger = new Logger();
            mMessageList = new List<IBridgeMessage>();
        }

        #endregion

        #region Event Handler

        private void SECSEngine_OnEstablishComm(object sender, SECS.SECSFramework.EventManager.EventArguments.SECSEngineEventArgs e)
        {
            try
            {
                if (e.ArgItem.Value.ToString() == "EquipmentCommunication")
                {
                    var commState = (EAPEquipmentCommState)e.ArgItem.Childs[0].Value;

                    if (commState == EAPEquipmentCommState.Communicating)
                    {
                        var mdln = e.ArgItem.Childs[1].Value.ToString();
                        var softRev = e.ArgItem.Childs[2].Value.ToString();

                        mLogger.LogHelper.LogDebug("Item3 =" + e.ArgItem.Childs[3].Value.ToString());
                        mLogger.LogHelper.LogDebug("Item4 =" + e.ArgItem.Childs[4].Value.ToString());

                        var replyRequired = (bool)e.ArgItem.Childs[3].Value;
                        var replySubject = e.ArgItem.Childs[4].Value.ToString();

                        var establishCommMessage = new EstablishCommMessage(commState, mdln, softRev, replyRequired, replySubject);
                        establishCommMessage.Sender = "SECSDriver";
                        establishCommMessage.Destination = "EquipmentConnectionDriver";
                        mParent.ReceiveMessage(establishCommMessage);
                    }
                }

            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }

        private void SECSEngine_OnGetAttrData(object sender, SECS.SECSFramework.EventManager.EventArguments.SECSEngineEventArgs e)
        {
            try
            {
                if (e.ArgItem.Value.ToString() == "StripMap")
                {
                    var stripId = e.ArgItem.Childs[0].Value.ToString();
                    var retrieveStripMap = new RetrieveStripMapMessage(mFWEquipmentName, mEquipmentName, stripId);
                    mMessageList.Add(retrieveStripMap);
                    mParent.ReceiveMessage(retrieveStripMap);
                }
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }

        private void SECSEngine_OnEventReport(object sender, SECS.SECSFramework.EventManager.EventArguments.SECSEngineEventArgs e)
        {
            try
            {
                var eventItems = e.ArgItem;

                //var equipmentEvent = new EquipmentEventPacket();

                var eventReportMessage = new EventReportMessage(mFWEquipmentName, mEquipmentName, e.ArgItem);
                mMessageList.Add(eventReportMessage);
                mParent.ReceiveMessage(eventReportMessage);
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }

        private void SECSEngine_OnVIDQuery(object sender, SECS.SECSFramework.EventManager.EventArguments.SECSEngineEventArgs e)
        {
            try
            {
                if (e.ArgItem.Value.ToString() == "EquipmentStatusQuery")
                {
                    var externalRequest = e.ArgItem.Childs[0].Childs;
                    var replyRequired = (bool)externalRequest[0].Value;

                    var vidItems = e.ArgItem.Childs[1].Childs;

                    if (replyRequired)
                    {
                        var replySubject = externalRequest[1].Value.ToString();
                        var dictSv = new Dictionary<string, string>();

                        foreach (var items in vidItems)
                        {
                            var vid = (int)items.Childs[0].Value;
                            var val = Convert.ToInt32(items.Childs[1].Value);

                            var variableText = mHelper.GetVariableTextByID(vid);
                            var valueText = mHelper.GetVariableValInText(vid, val);

                            dictSv.Add(variableText, valueText);
                        }

                        var message = mMessageList.First(x => x.MessageID == replySubject);
                        var equipmentStatusRequest = new EquipmentStatusPacket(mFWEquipmentName, mEquipmentName, dictSv);
                        equipmentStatusRequest.Destination = message.Sender;
                        equipmentStatusRequest.MessageID = message.MessageID;
                        equipmentStatusRequest.Subject = "Equipment.EquipmentStatusRequest.Reply";

                        mParent.ReceiveMessage(equipmentStatusRequest);
                    }
                    else
                    {
                        foreach (var items in vidItems)
                        {
                            var vid = (int)items.Childs[0].Value;
                            var val = Convert.ToInt32(items.Childs[1].Value);

                            if (vid == mHelper.GetControlStateVid())
                            {
                                mSECSEngine.UpdateControlState(mStateMap.ControlStateMap.ValueMap[val]);

                            }
                            else if (vid == mHelper.GetProcessStateVid())
                            {
                                mSECSEngine.UpdateProcessState(mStateMap.ProcessStateMap.ValueMap[val]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }
        protected virtual void SECSEngine_OnCustomVIDQuery(object sender, SECS.SECSFramework.EventManager.EventArguments.SECSEngineEventArgs e)
        {
            try
            {
                if (e.ArgItem.Value.ToString() == "CustomEquipmentStatusQuery")
                {
                    var externalRequest = e.ArgItem.Childs[0].Childs;
                    var replyRequired = (bool)externalRequest[0].Value;

                    var vidItems = e.ArgItem.Childs[1].Childs;

                    if (replyRequired)
                    {
                        var replySubject = externalRequest[1].Value.ToString();
                        var dictSv = new Dictionary<string, string>();

                        foreach (var items in vidItems)
                        {
                            var vid = (int)items.Childs[0].Value;
                            var val = items.Childs[1].Value == null ? "" : items.Childs[1].Value.ToString();

                            //var val2 = items.Childs[1].ke == null ? "" : items.Childs[1].Value.ToString();
                            //Logger.LogHelper.LogInfo("val2=" + vvin)

                            dictSv.Add(vid.ToString(), val.ToString());
                        }

                        var equipmentStatusRequest = new EquipmentStatusPacket(mEquipmentName, mEquipmentName, dictSv);

                        var message = mMessageList.First(x => x.MessageID == replySubject);
                        equipmentStatusRequest.Destination = message.Sender;
                        equipmentStatusRequest.MessageID = message.MessageID;
                        equipmentStatusRequest.Subject = "Equipment.EquipmentStatusRequest.Reply";

                        mParent.ReceiveMessage(equipmentStatusRequest);

                        //equipmentStatusRequest.Sender = "SECSDriver";
                        //equipmentStatusRequest.Destination = "NATSCommunicator";
                        //equipmentStatusRequest.Subject = "Equipment.Report";

                        //Parent.ReceiveMessage(equipmentStatusRequest);

                    }
                    else
                    {
                        foreach (var items in vidItems)
                        {
                            var vid = (int)items.Childs[0].Value;
                            var val = Convert.ToInt32(items.Childs[1].Value);

                            if (vid == Helper.GetControlStateVid())
                            {
                                SECSEngine.UpdateControlState(StateMap.ControlStateMap.ValueMap[val]);

                            }
                            else if (vid == Helper.GetProcessStateVid())
                            {
                                SECSEngine.UpdateProcessState(StateMap.ProcessStateMap.ValueMap[val]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
            }
        }

        protected virtual void SECSEngine_OnCustomConstantQuery(object sender, SECS.SECSFramework.EventManager.EventArguments.SECSEngineEventArgs e)
        {
            try
            {
                if (e.ArgItem.Value.ToString() == "CustomEquipmentConstantQuery")
                {
                    var externalRequest = e.ArgItem.Childs[0].Childs;
                    var replyRequired = (bool)externalRequest[0].Value;

                    var vidItems = e.ArgItem.Childs[1].Childs;

                    if (replyRequired)
                    {
                        var replySubject = externalRequest[1].Value.ToString();
                        var dictEc = new Dictionary<string, string>();

                        foreach (var items in vidItems)
                        {
                            var ecid = (int)items.Childs[0].Value;
                            var val = items.Childs[1].Value == null ? "" : items.Childs[1].Value.ToString();

                            dictEc.Add(ecid.ToString(), val.ToString());

                            Logger.LogHelper.LogInfo(ecid.ToString() + ":"+ val.ToString());
                        }

                        var equipmentConstantRequest = new EquipmentConstantPacket(mEquipmentName, mEquipmentName, dictEc);

                        var message = mMessageList.First(x => x.MessageID == replySubject);
                        equipmentConstantRequest.Destination = message.Sender;
                        equipmentConstantRequest.MessageID = message.MessageID;
                        equipmentConstantRequest.Subject = "Equipment.EquipmentConstantRequest.Reply";

                        mParent.ReceiveMessage(equipmentConstantRequest);

                    }
                   
                }
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
            }
        }

        private void SECSEngine_OnRecipeListQuery(object sender, SECS.SECSFramework.EventManager.EventArguments.SECSEngineEventArgs e)
        {
            try
            {
                if (e.ArgItem.Value.ToString() == "RecipeListQuery")
                {
                    var externalRequest = e.ArgItem.Childs[0].Childs;
                    var replyRequired = (bool)externalRequest[0].Value;

                    var recipeItems = e.ArgItem.Childs[1].Childs;
                    var errorInfoItems = e.ArgItem.Childs[2].Childs;

                    if (replyRequired)
                    {
                        var replySubject = externalRequest[1].Value.ToString();
                        var recipes = new List<string>();

                        foreach (var items in recipeItems)
                        {
                            var ppid = items.Value.ToString();

                            // Recipe filtering
                            ppid = mHelper.FilterRecipe(ppid);

                            if (!string.IsNullOrEmpty(ppid))
                                recipes.Add(ppid);
                        }

                        var message = mMessageList.First(x => x.MessageID == replySubject);
                        var equipmentStatusRequest = new EquipmentRecipeList(mFWEquipmentName, mEquipmentName, recipes.ToArray());
                        equipmentStatusRequest.ErrorCode = (int)errorInfoItems[0].Value;
                        equipmentStatusRequest.ErrorText = errorInfoItems[1].Value.ToString();
                        equipmentStatusRequest.Destination = "All";
                        equipmentStatusRequest.MessageID = message.MessageID;
                        equipmentStatusRequest.Subject = "Equipment.GetRecipeList.Reply";

                        mParent.ReceiveMessage(equipmentStatusRequest);
                    }
                }

            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }

        private void SECSEngine_OnFormattedRecipeQuery(object sender, SECS.SECSFramework.EventManager.EventArguments.SECSEngineEventArgs e)
        {
            try
            {
                mLogger.LogHelper.LogInfo("FormattedRecipeQuery event.");

                var formattedRecipeEventArgs = e as SECS.SECSFramework.EventManager.EventArguments.FormattedRecipeEventArgs;

                var replyRequired = formattedRecipeEventArgs.ReplyRequired;

                if (replyRequired)
                {
                    var replySubject = formattedRecipeEventArgs.ReplySubject;

                    mLogger.LogHelper.LogDebug("Searching for message {0}".FillArguments(replySubject));
                    var message = mMessageList.FirstOrDefault(x => x.MessageID == replySubject);
                    mLogger.LogHelper.LogDebug("Found message {0}".FillArguments(replySubject));

                    var recParamValidationMessage = message as RecParameterUpload;

                    var recParamCollection = formattedRecipeEventArgs.RecipeParameterCollection;

                    ConvertRecipeParameter(recParamCollection, ref recParamValidationMessage);

                    recParamValidationMessage.ErrorCode = formattedRecipeEventArgs.ErrorCode;
                    recParamValidationMessage.ErrorText = formattedRecipeEventArgs.ErrorText;
                    recParamValidationMessage.Destination = recParamValidationMessage.Sender;
                    recParamValidationMessage.Sender = "SECSDriver";
                    recParamValidationMessage.Subject = "Equipment.FormattedPPUpload.Reply";

                    mParent.ReceiveMessage(recParamValidationMessage);
                }
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogInfo("*Hit Exception at csecsbase" + ex);
                mLogger.LogHelper.LogException(ex);
            }
        }

        private void SECSEngine_OnRemoteCommand(object sender, SECS.SECSFramework.EventManager.EventArguments.SECSEngineEventArgs e)
        {
            try
            {
                mLogger.LogHelper.LogDebug("Received SECSEngine OnRemoteCommand event.");

                var rcmdEventArgs = e as SECS.SECSFramework.EventManager.EventArguments.RCMDEventArgs;

                mLogger.LogHelper.LogDebug("RemoteCommand.ReplyRequired={0}".FillArguments(rcmdEventArgs.ReplyRequired.ToString()));

                if (rcmdEventArgs.ReplyRequired)
                {
                    var replySubject = rcmdEventArgs.ReplySubject;
                    var message = mMessageList.First(x => x.MessageID == replySubject);
                    var command = rcmdEventArgs.Command;
                    var changeJobState = rcmdEventArgs.ChangeJobState;

                    var remoteCommand =
                        new RemoteCommand
                        (
                            mFWEquipmentName,
                            mEquipmentName,
                            command,
                            rcmdEventArgs.JobId,
                            rcmdEventArgs.ErrorCode,
                            rcmdEventArgs.ErrorText
                        );

                    remoteCommand.Destination = message.Sender;
                    remoteCommand.Sender = "SECSDriver";
                    remoteCommand.MessageID = message.MessageID;
                    remoteCommand.Subject = "Equipment.RemoteCommand.Reply";

                    mParent.ReceiveMessage(remoteCommand);

                    if (changeJobState)
                    {
                        var jobId = rcmdEventArgs.JobId;

                        if ((rcmdEventArgs.Command == "PPSelect" || rcmdEventArgs.Command == "CheckRecipe") && rcmdEventArgs.ErrorCode == EAPError.OK)
                        {
                            var jobStateChange =
                                new JobStateChange
                                (
                                    mFWEquipmentName,
                                    mEquipmentName,
                                    jobId,
                                    string.Empty,
                                    string.Empty,
                                    string.Empty,
                                    string.Empty,
                                    "Selected",
                                    "Queued"
                                );

                            jobStateChange.Sender = "SECSDriver";
                            jobStateChange.Destination = "JobControllerDriver";
                            jobStateChange.Subject = "Equipment.PPSelected";
                            mParent.ReceiveMessage(jobStateChange);
                        }
                        else if (rcmdEventArgs.Command == "StartJob" && rcmdEventArgs.ErrorCode == EAPError.OK)
                        {
                            var jobStateChange =
                                new JobStateChange
                                (
                                    mFWEquipmentName,
                                    mEquipmentName,
                                    jobId,
                                    string.Empty,
                                    string.Empty,
                                    string.Empty,
                                    string.Empty,
                                    "WaitingForStart",
                                    "Selected"
                                );

                            jobStateChange.Sender = "SECSDriver";
                            jobStateChange.Destination = "JobControllerDriver";
                            jobStateChange.Subject = "Equipment.StartJob";
                            mParent.ReceiveMessage(jobStateChange);
                        }
                        else if (rcmdEventArgs.Command == "Stop" && rcmdEventArgs.ErrorCode == EAPError.OK)
                        {
                            var jobStateChange =
                                new JobStateChange
                                (
                                    mFWEquipmentName,
                                    mEquipmentName,
                                    jobId,
                                    string.Empty,
                                    string.Empty,
                                    string.Empty,
                                    string.Empty,
                                    "Stopped",
                                    "InProcess"
                                );

                            jobStateChange.Sender = "SECSDriver";
                            jobStateChange.Destination = "JobControllerDriver";
                            jobStateChange.Subject = "Equipment.StopJob";
                            mParent.ReceiveMessage(jobStateChange);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }

        private void SECSGEMActionTask_OnActionCompleted(object sender, EventManager.EventArguments.SECSGEMActionEventArgs e)
        {
            try
            {
                if (e.ActionName == "EquipmentInitialization")
                {
                    var messageId = e.MessageID;

                    // To be fill up in EquipmentConnection
                    var initializationMessage =
                        new InitializationMessage
                        (
                            mFWEquipmentName,
                            mEquipmentName,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty
                        );

                    initializationMessage.Sender = "SECSDriver";
                    initializationMessage.Destination = "All";
                    initializationMessage.Subject = e.ReplyRequired ? "Equipment.Initialization.Reply" : "Equipment.Initialization.Notify";
                    initializationMessage.MessageID = messageId;
                    initializationMessage.ErrorCode = e.ErrorCode;
                    initializationMessage.ErrorText = e.ErrorText;
                    mParent.ReceiveMessage(initializationMessage);

                    var commControlState =
                        new EquipmentCommControlState
                        (
                            mFWEquipmentName,
                            mEquipmentName,
                            mSECSEngine.CommmunicationState,
                            mSECSEngine.ControlState
                        );

                    commControlState.Sender = "SECSDriver";
                    commControlState.Destination = "All";
                    commControlState.Subject = "Equipment.CommControlState";
                    commControlState.ErrorCode = e.ErrorCode;
                    commControlState.ErrorText = e.ErrorText;
                    mParent.ReceiveMessage(commControlState);

                    var processState =
                        new EquipmentProcessState
                        (
                            mFWEquipmentName,
                            mEquipmentName,
                            mSECSEngine.CurrentProcessState,
                            mSECSEngine.PreviousProcessState
                        );

                    processState.Sender = "SECSDriver";
                    processState.Destination = "All";
                    processState.Subject = "Equipment.ProcessState";
                    processState.ErrorCode = e.ErrorCode;
                    processState.ErrorText = e.ErrorText;
                    mParent.ReceiveMessage(processState);
                }

            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }

        private void SECSEngine_OnDisconnect(object sender, SECS.SECSFramework.EventManager.EventArguments.SECSEngineEventArgs e)
        {
            try
            {
                mLogger.LogHelper.LogInfo("SECSEngine fired OnDisconnect event.");
                var disconnectEngineEventArgs = e as SECS.SECSFramework.EventManager.EventArguments.DisconnectEventArgs;

                var commControlState =
                    new EquipmentCommControlState
                    (
                        mFWEquipmentName,
                        mEquipmentName,
                        mSECSEngine.CommmunicationState,
                        mSECSEngine.ControlState
                    );

                commControlState.Sender = "SECSDriver";
                commControlState.Destination = "All";
                commControlState.Subject = "Equipment.CommControlState";
                commControlState.ErrorCode = disconnectEngineEventArgs.ErrorCode;
                commControlState.ErrorText = disconnectEngineEventArgs.ErrorText;
                mParent.ReceiveMessage(commControlState);

                var processState =
                    new EquipmentProcessState
                    (
                        mFWEquipmentName,
                        mEquipmentName,
                        mSECSEngine.CurrentProcessState,
                        mSECSEngine.PreviousProcessState
                    );

                processState.Sender = "SECSDriver";
                processState.Destination = "All";
                processState.Subject = "Equipment.ProcessState";
                processState.ErrorCode = disconnectEngineEventArgs.ErrorCode;
                processState.ErrorText = disconnectEngineEventArgs.ErrorText;
                mParent.ReceiveMessage(processState);

            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }

        private void SECSEngine_OnAlarmReport(object sender, SECS.SECSFramework.EventManager.EventArguments.SECSEngineEventArgs e)
        {
            try
            {
                var alarmEventArgs = e as SECS.SECSFramework.EventManager.EventArguments.AlarmEventArgs;

                var alarmMessage =
                    new EquipmentAlarmPacket
                    (
                        mFWEquipmentName,
                        mEquipmentName,
                        alarmEventArgs.AlarmID,
                        alarmEventArgs.AlarmText,
                        alarmEventArgs.AlarmCode
                    );

                alarmMessage.Subject = "Equipment.Alarm";
                alarmMessage.Sender = "SECSDriver";
                alarmMessage.Destination = "All";

                mParent.ReceiveMessage(alarmMessage);
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }

        private void SECSEngine_OnTerminalDisplayReport(object sender, SECS.SECSFramework.EventManager.EventArguments.SECSEngineEventArgs e)
        {
            try
            {
                var terminalDisplayEventArgs = e as SECS.SECSFramework.EventManager.EventArguments.TerminalDisplayEventArgs;

                var terminalDisplayMessage =
                    new EquipmentTerminalDisplay
                    (
                        mFWEquipmentName,
                        mEquipmentName,
                        terminalDisplayEventArgs.TerminalDisplayText
                    );

                terminalDisplayMessage.Subject = "Equipment.TerminalDisplay.ADC";
                terminalDisplayMessage.Sender = "SECSDriver";
                terminalDisplayMessage.Destination = "All";

                mLogger.LogHelper.LogInfo("DEBUG HERE: " + terminalDisplayMessage.Message);

                //mParent.ReceiveMessage(terminalDisplayMessage);
                ReceiveMessage(terminalDisplayMessage);
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }
        private void SECSEngine_OnPPUpload(object sender, SECS.SECSFramework.EventManager.EventArguments.SECSEngineEventArgs e)
        {
            try
            {
                var unformattedRecipeEventArgs = e as SECS.SECSFramework.EventManager.EventArguments.UnformattedRecipeEventArgs;

                var unformattedRecipeUpload =
                    new UnformattedRecipeUpload
                    (
                        mFWEquipmentName,
                        mEquipmentName,
                        unformattedRecipeEventArgs.PPID,
                        unformattedRecipeEventArgs.PPBody,
                        true
                    );

                unformattedRecipeUpload.Subject = "Equipment.PPBodyUpload";
                unformattedRecipeUpload.Sender = "SECSDriver";
                unformattedRecipeUpload.Destination = "All";
                unformattedRecipeUpload.MessageID =
                    unformattedRecipeEventArgs.ReplyRequired ?
                    unformattedRecipeEventArgs.ReplySubject : string.Empty;

                mParent.ReceiveMessage(unformattedRecipeUpload);
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }

        private void SECSEngine_OnPPRequest(object sender, SECS.SECSFramework.EventManager.EventArguments.SECSEngineEventArgs e)
        {
            try
            {
                var PPRequestEventArgs = e as SECS.SECSFramework.EventManager.EventArguments.PPRequestEventArgs;

                var unformattedRecipeUpload =
                    new UnformattedRecipeUpload
                    (
                        mFWEquipmentName,
                        mEquipmentName,
                        PPRequestEventArgs.PPID,
                        null,
                        true
                    );

                unformattedRecipeUpload.MessageID = PPRequestEventArgs.ReplySubject;
                unformattedRecipeUpload.Subject = "Equipment.PPBody.Request";
                unformattedRecipeUpload.Sender = "SECSDriver";
                unformattedRecipeUpload.Destination = "All";

                mLogger.LogHelper.LogDebug("MessageID={0}".FillArguments(unformattedRecipeUpload.MessageID));

                mParent.ReceiveMessage(unformattedRecipeUpload);
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }

        private void SECSEngine_OnPPSend(object sender, SECS.SECSFramework.EventManager.EventArguments.SECSEngineEventArgs e)
        {
            var PPSendEventArgs = e as SECS.SECSFramework.EventManager.EventArguments.PPRequestEventArgs;

            var unformattedRecipeUpload =
                    new UnformattedRecipeUpload
                    (
                        mFWEquipmentName,
                        mEquipmentName,
                        PPSendEventArgs.PPID,
                        null,
                        true
                    );

            unformattedRecipeUpload.MessageID = PPSendEventArgs.ReplySubject;
            unformattedRecipeUpload.Subject = "Equipment.PPBody.Send";
            unformattedRecipeUpload.Sender = "SECSDriver";
            unformattedRecipeUpload.Destination = "RMSDriver";

            mLogger.LogHelper.LogDebug("MessageID={0}".FillArguments(unformattedRecipeUpload.MessageID));

            mParent.ReceiveMessage(unformattedRecipeUpload);
        }

        #endregion

        #region IEAPDriver

        public int AssignParent(IBridgeMessage bridgeMessage, object parent)
        {
            if (parent is IEAPDriver)
            {
                mParent = parent as IEAPDriver;
                return 0;
            }

            return -1;
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
                mEAPConfigFolder = bridgeMessage.GetBasicData("EAPCONFIGFOLDER").Value.ToString();
                mFWEquipmentName = bridgeMessage.GetBasicData("FWEQUIPMENTNAME").Value.ToString();
                mEquipmentName = bridgeMessage.GetBasicData("EQUIPMENTNAME").Value.ToString();
                var logDir = bridgeMessage.GetBasicData("LOGDIR").Value.ToString();
                var logFileName = "CSECS_{0}.log".FillArguments(mEquipmentName);

                mLogger.Initialize(mEquipmentName, Path.Combine(logDir, logFileName));
                mLogger.LogHelper.LogInfo("Initializing SECS Driver for {0}.{1}...".FillArguments(mFWEquipmentName, mEquipmentName));

                var configFile = mHelper.GetConfigFilePath(mEAPConfigFolder, mEquipmentName);

                if (!mHelper.LoadConfiguration(configFile))
                {
                    return EAPError.UNHANDLED_ERR;
                }

                mHelper.CollectSECSFunctionFiles(mHelper.GetTransactionDirPath(mEAPConfigFolder));

                var connSettings = mHelper.ExtractConnectionSettings(bridgeMessage);
                var secsFormatSettings = mHelper.CompileSECSFormatSettingsFromConfig();
                var hcackMapping = mHelper.CompileHCACKMappingCollectionFromConfig();
                mStateMap = mHelper.CompileStateMapFromConfig();
                LogConnectionSettings(bridgeMessage);
                InitializeSECSEngine(connSettings, secsFormatSettings, hcackMapping);

                mSECSGEMActionTask = new SECSGEMActionTask(mSECSEngine, mHelper, mLogger);
                mSECSGEMActionTask.OnActionCompleted += SECSGEMActionTask_OnActionCompleted;

                mLogger.LogHelper.LogInfo("SECS Driver initialization done for {0}.{1}".FillArguments(mFWEquipmentName, mEquipmentName));
                return 0;
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
                return EAPError.UNKNOWN_ERR;
            }
        }

        public int ReceiveMessage(IBridgeMessage bridgeMessage)
        {
            try
            {
                mLogger.LogHelper.LogInfo("Received Sender:{0}, Destination:{1}, Subject:{2}".FillArguments(bridgeMessage.Sender, bridgeMessage.Destination, bridgeMessage.Subject));

                if (bridgeMessage.Destination.Equals("All", StringComparison.CurrentCultureIgnoreCase) || bridgeMessage.Destination == "SECSDriver")
                {
                    switch (bridgeMessage.Subject)
                    {
                        case "SECSGEMAction":
                            var action = bridgeMessage.GetBasicData("ACTION").Value.ToString();
                            var replyRequired = Convert.ToBoolean(bridgeMessage.GetBasicData("REPLYREQUIRED").Value);
                            mSECSGEMActionTask.RunAction(action, replyRequired, bridgeMessage.MessageID);
                            break;

                        case "EstablishCommunication":
                            var replyRequired2 = Convert.ToBoolean(bridgeMessage.GetBasicData("REPLYREQUIRED").Value);

                            mSECSEngine.EstablishCommunication(replyRequired2, bridgeMessage.MessageID);
                            break;

                        case "RetrieveStripMap.Reply":
                            var findRetrieveStripMessage = mMessageList.FirstOrDefault(x => x.MessageID == bridgeMessage.MessageID);
                            if (findRetrieveStripMessage != null)
                            {
                                var retrieveStripMessage = (RetrieveStripMapMessage)findRetrieveStripMessage;
                                retrieveStripMessage.CopyReplyData(bridgeMessage);

                                mSECSEngine.ReplyGetAttrData(
                                    retrieveStripMessage.Reply.Result,
                                    retrieveStripMessage.Reply.StripID,
                                    retrieveStripMessage.Reply.Row,
                                    retrieveStripMessage.Reply.Column,
                                    retrieveStripMessage.Reply.OriginLocation,
                                    retrieveStripMessage.Reply.EMapLoc,
                                    retrieveStripMessage.Reply.StripMapData,
                                    retrieveStripMessage.Reply.LotID);
                            }
                            else
                            {
                                mLogger.LogHelper.LogDebug("No strip message retrieved, and reply is not required.");
                            }
                            break;

                        case "Equipment.EquipmentStatusRequest":
                            var equipmentStatusRequest = bridgeMessage as EquipmentStatusPacket;
                            var requestSVList = equipmentStatusRequest.DictSV;
                            var convertedSVList = mHelper.ConvertToInternalSVList(requestSVList);
                            mMessageList.Add(equipmentStatusRequest);

                            mSECSEngine.EquipmentStatusRequest(true, equipmentStatusRequest.MessageID, convertedSVList);
                            break;

                        case "Equipment.GetRecipeList":
                            HandleEquipmentGetRecipeList(bridgeMessage);
                            break;

                        case "UpdateControlState":
                            var controlState = bridgeMessage.GetBasicData("CONTROLSTATE").Value.ToString();
                            mSECSEngine.UpdateControlState(controlState);
                            break;

                        case "UpdateProcessState":
                            var processState = bridgeMessage.GetBasicData("PROCESSSTATE").Value.ToString();
                            mSECSEngine.UpdateProcessState(processState);
                            break;

                        case "Equipment.RemoteCommand":
                            HandleRemoteCommand(bridgeMessage);
                            break;

                        case "Equipment.TerminalDisplay":
                            HandleEquipmentTerminalDisplay(bridgeMessage);
                            break;

                        case "Equipment.TerminalDisplayMulti":
                            HandleEquipmentTerminalDisplayMulti(bridgeMessage);
                            break;

                        case "Equipment.PPBody.Request.Reply":
                            var ppBodyRequestReply = bridgeMessage as UnformattedRecipeUpload;
                            mSECSEngine.ReplyPPBodyRequest(ppBodyRequestReply.Result, ppBodyRequestReply.MessageID, ppBodyRequestReply.PPBody);
                            break;

                        case "Equipment.PPBody.Request":
                            var recReq = bridgeMessage as RecParameterUpload;
                            mSECSEngine.UnformattedPPRequest(true, recReq.MessageID, recReq.PPID);
                            break;

                        case "Equipment.FormattedPPUpload.Request":
                            var formattedPPReq = bridgeMessage as RecParameterUpload;
                            mMessageList.Add(bridgeMessage);
                            mSECSEngine.FormattedPPRequest(true, formattedPPReq.MessageID, formattedPPReq.PPID);
                            break;

                        case "Equipment.PPBody.Send":
                            var ppBodySend = bridgeMessage as UnformattedRecipeUpload;
                            mSECSEngine.UnformattedPPSend(ppBodySend.PPID, ppBodySend.FilePath, ppBodySend.PPBody);
                            break;

                        case "Equipment.TerminalDisplay.ADC":
                            HandleCustomEquipmentStatusRequest(bridgeMessage);
                            break;

                        case "Equipment.CustomEquipmentStatusRequest":
                            var customEquipmentStatusRequest = bridgeMessage as EquipmentStatusPacket;
                            var customRequestSVList = customEquipmentStatusRequest.DictSV;

                            var internalSVList = new Dictionary<int, string>();

                            foreach (var kvp in customRequestSVList)
                            {
                                var svText = kvp.Key;

                                internalSVList.Add(int.Parse(svText), null);
                            }

                            mMessageList.Add(customEquipmentStatusRequest);

                            mSECSEngine.CustomEquipmentStatusRequest(true, customEquipmentStatusRequest.MessageID, internalSVList);
                            break;

                        case "Equipment.CustomEquipmentConstantRequest":
                            var customEquipmentConstantRequest = bridgeMessage as EquipmentConstantPacket;
                            var customRequestECList = customEquipmentConstantRequest.DictEC;

                            var internalECList = new Dictionary<int, string>();

                            foreach (var kvp in customRequestECList)
                            {
                                var ecText = kvp.Key;

                                internalECList.Add(int.Parse(ecText), null);
                            }

                            mMessageList.Add(customEquipmentConstantRequest);

                            mSECSEngine.CustomEquipmentConstantRequest(true, customEquipmentConstantRequest.MessageID, internalECList);
                            break;
                    }
                }

                return EAPError.OK;
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
                return EAPError.UNKNOWN_ERR;
            }
        }

        #endregion

        #region Protected Method        

        protected virtual SECSEngine NewSECSEngine()
        {
            mSECSEngine = new SECSEngine();

            return mSECSEngine;
        }

        protected virtual void RecipeBodyUpload(string ppid, byte[] ppBody)
        {

        }

        protected virtual void HandleRemoteCommand(IBridgeMessage bridgeMessage)
        {
            var remoteCommand = bridgeMessage as RemoteCommand;
            var replyRequired = Convert.ToBoolean(bridgeMessage.GetBasicData("REPLYREQUIRED").Value);

            mMessageList.Add(remoteCommand);

            var parameters = new Dictionary<string, string>();
            parameters.Add("JOBID", remoteCommand.JobID);
            parameters.Add("DEVICE", remoteCommand.Device);
            parameters.Add("PACKAGE", remoteCommand.Package);
            parameters.Add("LOT", remoteCommand.Lot);
            parameters.Add("PPID", remoteCommand.PPID);
            parameters.Add("LoadPort", remoteCommand.LoadPort);
            parameters.Add("UserId", remoteCommand.UserID);
            parameters.Add("LotSize", remoteCommand.LotSize);
            parameters.Add("MagazineCount", remoteCommand.MagazineCount);

            mSECSEngine.RemoteCommand(true, remoteCommand.MessageID, remoteCommand.Command, parameters);
        }

        protected virtual void HandleEquipmentTerminalDisplay(IBridgeMessage bridgeMessage)
        {
            var equipmentTerminalDisplay = bridgeMessage as EquipmentTerminalDisplay;

            mSECSEngine.TerminalDisplay(equipmentTerminalDisplay.TerminalNumber, equipmentTerminalDisplay.Message);
        }

        protected virtual void HandleEquipmentTerminalDisplayMulti(IBridgeMessage bridgeMessage)
        {
            var equipmentTerminalDisplayMulti = bridgeMessage as EquipmentTerminalDisplayMulti;

            mSECSEngine.TerminalDisplayMulti(0x00, equipmentTerminalDisplayMulti.Message);
        }

        protected virtual void HandleEquipmentGetRecipeList(IBridgeMessage bridgeMessage)
        {
            var equipmentRecipeList = bridgeMessage as Qynix.EAP.Base.BridgeMessage.Common.EquipmentRecipeList;
            mMessageList.Add(equipmentRecipeList);

            mSECSEngine.EPPDRequest(true, equipmentRecipeList.MessageID);
        }

        protected virtual void ConvertRecipeParameter(RecipeParameterCollection source, ref RecParameterUpload destination)
        {

        }
        protected virtual void HandleCustomEquipmentStatusRequest(IBridgeMessage bridgeMessage)
        {
        }
        #endregion

        #region Private Field

        private void InitializeSECSEngine(ConnectionSettings connSettings, SECSFormatSetting secsFormatSettings, HCACKMappingCollection hcackMapping)
        {
            mSECSEngine = NewSECSEngine();

            mSECSEngine.Initialize(connSettings, secsFormatSettings, hcackMapping, mHelper.Configuration.HostSettings.EstablishCommTimer);
            mSECSEngine.AssignLogger(mLogger.LogHelper);
            mSECSEngine.SetDefaultLibrary(mHelper.Configuration.SECSProConfiguration.DefaultSECSLibrary);
            mSECSEngine.SetLogFileName(mHelper.Configuration.SECSProConfiguration.LogFileName);
            mSECSEngine.SetLogFolder(mHelper.Configuration.SECSProConfiguration.LogFileDir);
            mSECSEngine.SetLogFileTraceLevel(mHelper.Configuration.SECSProConfiguration.LogFileTraceLevel);
            if (mHelper.Configuration.SECSProConfiguration.LogFileMaxDays > 0)
                mSECSEngine.SetLogFileMaxDays(mHelper.Configuration.SECSProConfiguration.LogFileMaxDays);
            mSECSEngine.OnEstablishComm += SECSEngine_OnEstablishComm;
            mSECSEngine.OnGetAttrData += SECSEngine_OnGetAttrData;
            mSECSEngine.OnEventReport += SECSEngine_OnEventReport;
            mSECSEngine.OnVIDQuery += SECSEngine_OnVIDQuery;
            mSECSEngine.OnCustomVIDQuery += SECSEngine_OnCustomVIDQuery;
            mSECSEngine.OnCustomConstantQuery += SECSEngine_OnCustomConstantQuery;
            mSECSEngine.OnRecipeListQuery += SECSEngine_OnRecipeListQuery;
            mSECSEngine.OnFormattedRecipeQuery += SECSEngine_OnFormattedRecipeQuery;
            mSECSEngine.OnRemoteCommand += SECSEngine_OnRemoteCommand;
            mSECSEngine.OnAlarmReport += SECSEngine_OnAlarmReport;
            mSECSEngine.OnPPUpload += SECSEngine_OnPPUpload;
            mSECSEngine.OnPPRequest += SECSEngine_OnPPRequest;
            mSECSEngine.OnPPSend += SECSEngine_OnPPSend;
            mSECSEngine.OnDisconnect += SECSEngine_OnDisconnect;
            mSECSEngine.OnTerminalDisplayReport += SECSEngine_OnTerminalDisplayReport;

            mSECSEngine.FireDisconnectEvent();
            mSECSEngine.OpenPort();
        }        

        private void LogConnectionSettings(IBridgeMessage message)
        {
            foreach (var data in message.Data)
            {
                if (data.Name == "EAPCONFIGFOLDER")
                    continue;

                mLogger.LogHelper.LogDebug("{0} = {1}".FillArguments(data.Name, data.Value));
            }
        }

        #endregion

    }
}
