using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.NATSCommunicationDriver.NATSEngine
{
    using Base.BridgeMessage.Common;
    using Generic;
    using EAPMessages.Send;
    using NATS.Client;
    using RMS.Common;
    using Base.BridgeMessage;
    using Utilities.ExtensionPlug;

    public class NATSController
    {
        #region Private Field

        private string mUrl;
        private Helper mHelper;
        private Logger mLogger;
        private NATSCommunicator mParent;

        private NATSReplier mNATSReplier;
        private NATSRequestor mNATSRequestor;
        private NATSRequestor mNATSMESRequestor;
        private NATSPublisher mNATSPublisher;
        private NATSPublisher mNATSEMapPublisher;
        private NATSSubscriber mNATSSubscriber;

        private List<Msg> mIncomingMessages;
        private List<IBridgeMessage> mMessageList;

        #endregion

        #region Custom Class


        #endregion

        #region Constructor

        public NATSController(string url, NATSCommunicator parent, Helper helper, Logger logger)
        {
            mUrl = url;
            mParent = parent;
            mHelper = helper;
            mLogger = logger;

            Initialize();
        }

        #endregion

        #region Public Method

        public void SendInitialization(InitializationMessage initializationMessage)
        {
            mNATSPublisher.Publish(initializationMessage);
        }

        public void Publish(BaseMessage message)
        {
            message.UserId = Environment.UserName;
            message.HostName = Environment.MachineName;
            mNATSPublisher.Publish(message);
        }

        public void PublishEMapsUpdate(BaseMessage message)
        {
            message.UserId = Environment.UserName;
            message.HostName = Environment.MachineName;
            mNATSEMapPublisher.Publish(message);
        }

        public void ReplyRequest(string replySubject, BaseMessage message)
        {
            var messages = mIncomingMessages.ToArray();
            var requestMsg = (from a in messages
                              where a.Reply == replySubject
                              select a).FirstOrDefault();

            message.UserId = Environment.UserName;
            message.HostName = Environment.MachineName;
            mNATSReplier.Reply(replySubject, message, requestMsg);
        }

        public void RequestGetRecipe(GetRecipeInfo getRecipeInfo)
        {
            mMessageList.Add(getRecipeInfo);
            RecipeMessage recipeMessage = new RecipeMessage();
            recipeMessage.UserId = Environment.UserName;
            recipeMessage.HostName = Environment.MachineName;
            recipeMessage.FWMachineName = getRecipeInfo.FWEquipmentID;
            recipeMessage.MachineName = getRecipeInfo.EquipmentID;
            recipeMessage.Command = EnumType.Command.GetRecipe;
            recipeMessage.Lot = getRecipeInfo.Lot;
            recipeMessage.Attributes.Add(getRecipeInfo.Attribute.ToEnum<EnumType.Attribute>());
            recipeMessage.Subject = getRecipeInfo.MessageID;
            mNATSMESRequestor.Request(recipeMessage);
        }

        #endregion

        #region Event Handler

        private void NATSSubscriber_OnMessageReceived(object sender, EventManager.EventArgument.NATSMessageEventArgs e)
        {
            try
            {
                switch (e.Message.Command)
                {
                    case EnumType.Command.JobCreated:
                        var jobCreateMessage = (JobMessage)e.Message;

                        var jobCreate =
                            new JobCreate(
                                jobCreateMessage.FWMachineName,
                                jobCreateMessage.MachineName,
                                jobCreateMessage.JobId,
                                jobCreateMessage.Device,
                                jobCreateMessage.Package,
                                jobCreateMessage.Lot,
                                jobCreateMessage.PPID,
                                jobCreateMessage.UserId);

                        // No need reply, assign guid
                        jobCreate.MessageID = Guid.NewGuid().ToString();
                        jobCreate.Sender = "NATSCommunicator";
                        jobCreate.Destination = "JobControllerDriver";
                        jobCreate.Subject = "CreateJob.Receive";

                        mParent.SendMessage(jobCreate);
                        break;

                    case EnumType.Command.JobStateChanged:
                        var jobStateChangeMessage = (JobMessage)e.Message;

                        var jobStateChange =
                            new JobStateChange(
                                jobStateChangeMessage.FWMachineName,
                                jobStateChangeMessage.MachineName,
                                jobStateChangeMessage.JobId,
                                jobStateChangeMessage.Device,
                                jobStateChangeMessage.Package,
                                jobStateChangeMessage.Lot,
                                jobStateChangeMessage.PPID,
                                jobStateChangeMessage.CurrentProcessState.ToString(),
                                jobStateChangeMessage.PreviousProcessState.ToString());

                        // No need reply, assign guid
                        jobStateChange.MessageID = Guid.NewGuid().ToString();
                        jobStateChange.Sender = "NATSCommunicator";
                        jobStateChange.Destination = "JobControllerDriver";
                        jobStateChange.Subject = "JobStateChange.Receive";

                        mParent.SendMessage(jobStateChange);
                        break;

                }

            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }

        private void NATSReplier_OnRequestReceived(object sender, EventManager.EventArgument.NATSMessageEventArgs e)
        {
            try
            {
                mIncomingMessages.Add(e.RawMessage);

                switch (e.Message.Command)
                {
                    case EnumType.Command.Initialization:
                        HandleEquipmentInitializationRequest(e);
                        break;

                    case EnumType.Command.VIDQuery:
                        HandleVIDQueryRequest(e);
                        break;

                    case EnumType.Command.RemoteCommand:
                        HandleRemoteCommandRequest(e);
                        break;

                    case EnumType.Command.RecList:
                        HandleRecipeListRequest(e);
                        break;

                    case EnumType.Command.RecParameterUpload:
                        HandleRecipeParameterUploadRequest(e);
                        break;

                    case EnumType.Command.RecParameterValidation:
                        HandleRecipeParameterValidationRequest(e);
                        break;

                    case EnumType.Command.RecBodyUpload:
                        HandleRecipeBodyUploadRequest(e);
                        break;

                    case EnumType.Command.RecBodyValidation:
                        HandleRecipeBodyValidationRequest(e);
                        break;

                    case EnumType.Command.CustomVIDQuery:
                        mLogger.LogHelper.LogInfo("CustomVIDQuery");
                        HandleCustomVIDQueryRequest(e);
                        break;
                    case EnumType.Command.CustomConstantQuery:
                        mLogger.LogHelper.LogInfo("CustomConstantQuery");
                        HandleCustomConstantQueryRequest(e);
                        break;

                    case EnumType.Command.TerminalMessage:
                        mLogger.LogHelper.LogInfo("TerminalMessage");
                        HandleTerminalMessageRequest(e);
                        break;
                }
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }

        private void NATSMESRequestor_OnMessageRequested(object sender, EventManager.EventArgument.NATSMessageEventArgs e)
        {
            try
            {
                if (e.Message.Command != EnumType.Command.GetRecipe)
                    return;

                var recipeMessage = (RecipeMessage)Common.ByteArrayToObject(e.RawMessage.Data);
                //var recipeMessage = (RecipeMessage)e.Message;
                var bridgeMessage = mMessageList.FirstOrDefault(x => x.MessageID == recipeMessage.Subject);

                if (bridgeMessage == null)
                {
                    mLogger.LogHelper.LogError("Message not found for MessageID={0},".FillArguments(recipeMessage.Subject), "NATSMESRequestor_OnMessageRequested", "C:\\EAP\\NATSCommunicationDriver\\NATSEngine\\NATSController.cs");
                }
                else
                {
                    mLogger.LogHelper.LogDebug("MES Replied package = {0}".FillArguments(recipeMessage.Package));

                    GetRecipeInfo getRecipeInfo = bridgeMessage as GetRecipeInfo;
                    getRecipeInfo.Package = recipeMessage.Package;
                    getRecipeInfo.Destination = getRecipeInfo.Sender;
                    getRecipeInfo.Sender = "NATSCommunicationDriver";
                    getRecipeInfo.Subject = "MES.GetRecipeInfo.Reply";
                    //mParent.ReceiveMessage(getRecipeInfo);
                    mParent.SendMessage(getRecipeInfo);
                }
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex, "NATSMESRequestor_OnMessageRequested", "C:\\EAP\\NATSCommunicationDriver\\NATSEngine\\NATSController.cs");
            }
        }

        #endregion

        #region Private Method

        private void Initialize()
        {
            var publishSubject = mHelper.Configuration.Subjects.Subject.Single(x => x.Type == "Publish");
            mNATSPublisher = new NATSPublisher(mUrl, publishSubject.Name, mHelper, mLogger);

            var eMapsPublishSubject = mHelper.Configuration.Subjects.Subject.SingleOrDefault(x => x.Type == "Publish.EMAPS");
            mNATSEMapPublisher = eMapsPublishSubject == null ? null : new NATSPublisher(mUrl, eMapsPublishSubject.Name, mHelper, mLogger);

            var replierSubject = mHelper.Configuration.Subjects.Subject.Single(x => x.Type == "Reply");
            mNATSReplier = new NATSReplier(mUrl, replierSubject.Name, mHelper, mLogger);
            mNATSReplier.OnRequestReceived += NATSReplier_OnRequestReceived;

            var subscribeSubject = mHelper.Configuration.Subjects.Subject.Single(x => x.Type == "Subscribe");
            mNATSSubscriber = new NATSSubscriber(mUrl, subscribeSubject.Name, mHelper, mLogger);
            mNATSSubscriber.OnMessageReceived += NATSSubscriber_OnMessageReceived;

            var requestSubject = mHelper.Configuration.Subjects.Subject.Single(x => x.Type == "Request");
            mNATSMESRequestor = new NATSRequestor(mUrl, requestSubject.Name, mHelper, mLogger);
            mNATSMESRequestor.OnMessageRequested += NATSMESRequestor_OnMessageRequested;

            mIncomingMessages = new List<Msg>();
            mMessageList = new List<IBridgeMessage>();
        }

        private void HandleEquipmentInitializationRequest(EventManager.EventArgument.NATSMessageEventArgs e)
        {
            var initializationMessage = (InitializationMessage)e.Message;
            var initializeEquipment = new EquipmentInitialization(true, e.ReplySubject, initializationMessage.FWMachineName, initializationMessage.MachineName);
            initializeEquipment.MessageID = e.ReplySubject;
            initializeEquipment.Sender = "NATSCommunicator";
            initializeEquipment.Subject = "Equipment.Initialization";
            initializeEquipment.Destination = "EquipmentConnectionDriver";

            mParent.SendMessage(initializeEquipment);
        }

        private void HandleVIDQueryRequest(EventManager.EventArgument.NATSMessageEventArgs e)
        {
            var vidMessage = (VariableMessage)e.Message;
            var equipmentStatusRequest = new EquipmentStatusPacket(vidMessage.FWMachineName, vidMessage.MachineName, vidMessage.Vids);
            equipmentStatusRequest.MessageID = e.ReplySubject;
            equipmentStatusRequest.Sender = "NATSCommunicator";
            equipmentStatusRequest.Destination = "SECSDriver";
            equipmentStatusRequest.Subject = "Equipment.EquipmentStatusRequest";

            mIncomingMessages.Add(e.RawMessage);
            mParent.SendMessage(equipmentStatusRequest);
        }
        private void HandleCustomVIDQueryRequest(EventManager.EventArgument.NATSMessageEventArgs e)
        {
            var vidMessage = (VariableMessage)e.Message;
            var equipmentStatusRequest = new EquipmentStatusPacket(vidMessage.FWMachineName, vidMessage.MachineName, vidMessage.Vids);
            equipmentStatusRequest.MessageID = e.ReplySubject;
            equipmentStatusRequest.Sender = "NATSCommunicator";
            equipmentStatusRequest.Destination = "SECSDriver";
            equipmentStatusRequest.Subject = "Equipment.CustomEquipmentStatusRequest";

            mIncomingMessages.Add(e.RawMessage);
            mParent.SendMessage(equipmentStatusRequest);
        }

        private void HandleCustomConstantQueryRequest(EventManager.EventArgument.NATSMessageEventArgs e)
        {
            var ecidMessage = (VariableMessage)e.Message;
            var equipmentConstantRequest = new EquipmentConstantPacket(ecidMessage.FWMachineName, ecidMessage.MachineName, ecidMessage.ECIDs);
            equipmentConstantRequest.MessageID = e.ReplySubject;
            equipmentConstantRequest.Sender = "NATSCommunicator";
            equipmentConstantRequest.Destination = "SECSDriver";
            equipmentConstantRequest.Subject = "Equipment.CustomEquipmentConstantRequest";

            mIncomingMessages.Add(e.RawMessage);
            mParent.SendMessage(equipmentConstantRequest);
        }

        private void HandleRemoteCommandRequest(EventManager.EventArgument.NATSMessageEventArgs e)
        {
            var remoteCommandMessage = (RemoteCommandMessage)e.Message;
            var remoteCommand =
                new RemoteCommand(
                    remoteCommandMessage.FWMachineName,
                    remoteCommandMessage.MachineName,
                    remoteCommandMessage.JobId,
                    remoteCommandMessage.Device,
                    remoteCommandMessage.Package,
                    remoteCommandMessage.Lot,
                    remoteCommandMessage.PPID,
                    remoteCommandMessage.CommandType.ToString(),
                    remoteCommandMessage.LoadPort,
                    remoteCommandMessage.UserId,
                    remoteCommandMessage.LotSize, //LotSize from new generic.dll fahmi 9/3/2020 | add LotSize for AOI IVE
                    remoteCommandMessage.MagazineCount,
                    true);

            mLogger.LogHelper.LogDebug("Job ID = " + remoteCommand.JobID);

            remoteCommand.MessageID = e.ReplySubject;
            remoteCommand.Sender = "NATSCommunicator";
            remoteCommand.Destination = "JobControllerDriver";
            remoteCommand.Subject = "Equipment.RemoteCommand";

            mParent.SendMessage(remoteCommand);
        }

        private void HandleRecipeListRequest(EventManager.EventArgument.NATSMessageEventArgs e)
        {
            var recipeMessage = (RecipeMessage)e.Message;
            var equipmentRecipeList = new EquipmentRecipeList(recipeMessage.FWMachineName, recipeMessage.MachineName, null);
            equipmentRecipeList.MessageID = e.ReplySubject;
            equipmentRecipeList.Sender = "NATSCommunicator";
            equipmentRecipeList.Destination = "SECSDriver";
            equipmentRecipeList.Subject = "Equipment.GetRecipeList";

            mParent.SendMessage(equipmentRecipeList);
        }

        private void HandleRecipeParameterUploadRequest(EventManager.EventArgument.NATSMessageEventArgs e)
        {
            var recipeUploadMessage = (RecipeParameterMessage)e.Message;
            var recipeUpload = new RecParameterUpload(
                    recipeUploadMessage.FWMachineName,
                    recipeUploadMessage.MachineName,
                    recipeUploadMessage.PPID,
                    null
                );
            recipeUpload.MessageID = e.ReplySubject;
            recipeUpload.Sender = "NATSCommunicator";
            recipeUpload.Destination = "RMSDriver";
            recipeUpload.Subject = "FormattedPPUpload.Request";

            mParent.SendMessage(recipeUpload);
        }

        private void HandleRecipeParameterValidationRequest(EventManager.EventArgument.NATSMessageEventArgs e)
        {
            var recipeParameterMessage = (RecipeParameterMessage)e.Message;
            var recipeParamList = new RecParameterValidation(
                    recipeParameterMessage.FWMachineName,
                    recipeParameterMessage.MachineName,
                    recipeParameterMessage.PPID,
                    GetParamList(recipeParameterMessage)
                );
            recipeParamList.MessageID = e.ReplySubject;
            recipeParamList.Sender = "NATSCommunicator";
            recipeParamList.Destination = "RMSDriver";
            recipeParamList.Subject = "RecParameterValidation.Request";

            mParent.SendMessage(recipeParamList);
        }

        private void HandleRecipeBodyUploadRequest(EventManager.EventArgument.NATSMessageEventArgs e)
        {
            var recipeUploadMessage = (RecipeBodyMessage)e.Message;
            var recipeUpload = new UnformattedRecipeUpload
                (
                    recipeUploadMessage.FWMachineName,
                    recipeUploadMessage.MachineName,
                    recipeUploadMessage.PPID,
                    null,
                    true,
                    false
                );

            recipeUpload.MessageID = e.ReplySubject;
            recipeUpload.Sender = "NATSCommunicator";
            recipeUpload.Destination = "RMSDriver";
            recipeUpload.Subject = "RecipeBodyUpload";

            mParent.SendMessage(recipeUpload);
        }

        private void HandleRecipeBodyValidationRequest(EventManager.EventArgument.NATSMessageEventArgs e)
        {
            var recipeUploadMessage = (RecipeBodyMessage)e.Message;
            var recipeUpload = new UnformattedRecipeUpload
                (
                    recipeUploadMessage.FWMachineName,
                    recipeUploadMessage.MachineName,
                    recipeUploadMessage.FilePath,
                    recipeUploadMessage.PPID,
                    null,
                    true,
                    false
                );

            recipeUpload.MessageID = e.ReplySubject;
            recipeUpload.Sender = "NATSCommunicator";
            recipeUpload.Destination = "RMSDriver";
            recipeUpload.Subject = "UnformattedPPValidation.Request";

            mParent.SendMessage(recipeUpload);
        }

        private List<RecParameterValidation.RecipeParameter> GetParamList(RecipeParameterMessage message)
        {
            try
            {
                List<RecParameterValidation.RecipeParameter> paramList = new List<RecParameterValidation.RecipeParameter>();

                foreach (var param in message.RecipeParameter)
                {
                    RecParameterValidation.RecipeParameter recipeParameter = new RecParameterValidation.RecipeParameter();
                    recipeParameter.Name = param.Name;
                    recipeParameter.DataType = param.DataType;
                    recipeParameter.Value = param.Value;
                    recipeParameter.ValidationRule = (RecParameterValidation.EnumValidationRule)param.ValidationRule;
                    recipeParameter.MaxValue = param.MaxValue;
                    recipeParameter.MinValue = param.MinValue;

                    paramList.Add(recipeParameter);
                }

                return paramList;
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
                return null;
            }
        }
        private void HandleTerminalMessageRequest(EventManager.EventArgument.NATSMessageEventArgs e)
        {
            var terminalMsg = (TerminalMessage)e.Message;
            var equipmentTerminalDisplay = new EquipmentTerminalDisplay
                (
                 terminalMsg.FWMachineName,
                 terminalMsg.MachineName,
                 terminalMsg.Message,
                 terminalMsg.TerminalNumber
                );

            equipmentTerminalDisplay.MessageID = e.ReplySubject;
            equipmentTerminalDisplay.Sender = "NATSCommunicator";
            equipmentTerminalDisplay.Destination = "SECSDriver";
            equipmentTerminalDisplay.Subject = "Equipment.TerminalDisplay";

            mParent.SendMessage(equipmentTerminalDisplay);

            ReplyRequest(e.ReplySubject, new BaseMessage() { FWMachineName = terminalMsg.FWMachineName });
        }

        #endregion

    }
}
