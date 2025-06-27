using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.NATSCommunicationDriver
{
    using Base.BridgeMessage.Common;
    using EAP.Base.BaseInterfaces;
    using EAP.Base.BridgeMessage;
    using Generic;
    using NATSEngine;
    using Utilities.ExtensionPlug;

    public class NATSCommunicator : IEAPDriver
    {
        #region Private Field

        private string mEAPConfigFolder;

        private IEAPDriver mParent;
        private Helper mHelper;
        private Logger mLogger;
        private NATSController mNATS;

        private string mFWEquipmentID;
        private string mEquipmentID;

        #endregion

        #region Protected Field

        protected NATSController NATSController
        {
            get { return mNATS; }
        }

        protected Logger Logger
        {
            get { return mLogger; }
        }

        #endregion

        #region Constructor

        public NATSCommunicator()
        {
            mHelper = new Helper();
            mLogger = new Logger();
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
            mEAPConfigFolder = bridgeMessage.GetBasicData("EAPCONFIGFOLDER").Value.ToString();
            var configFile = mHelper.GetConfigFilePath(mEAPConfigFolder);

            if (!mHelper.LoadConfiguration(configFile))
            {
                return EAPError.UNHANDLED_ERR;
            }

            try
            {
                mLogger.Initialize(mHelper.Configuration.Logger);
                mLogger.LogHelper.LogInfo("Initializing NATS Communicator...");

                mFWEquipmentID = bridgeMessage.GetBasicData("FWEQUIPMENTNAME").Value.ToString();
                mEquipmentID = bridgeMessage.GetBasicData("EQUIPMENTNAME").Value.ToString();
                mNATS = new NATSController(mHelper.Configuration.NATSConnectionSettings.URL, this, mHelper, mLogger);

                mLogger.LogHelper.LogInfo("NATS Communicator initialization done.");
                mLogger.LogHelper.LogInfo("NATS Url:{0}".FillArguments(mHelper.Configuration.NATSConnectionSettings.URL));

                return EAPError.OK;

            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
                return EAPError.UNHANDLED_ERR;
            }
        }

        public int ReceiveMessage(IBridgeMessage bridgeMessage)
        {
            try
            {
                mLogger.LogHelper.LogInfo("Received Subject:{0}, Sender:{1}, PassBySender:{2}, Destination:{3}".FillArguments(bridgeMessage.Subject, bridgeMessage.Sender, bridgeMessage.PassBySender, bridgeMessage.Destination));

                var fwEquipmentId = bridgeMessage.GetBasicData("FWEQUIPMENTID").Value.ToString();
                var equipmentId = bridgeMessage.GetBasicData("EQUIPMENTID").Value.ToString();

                if (!(fwEquipmentId == mFWEquipmentID && equipmentId == mEquipmentID))
                {
                    return EAPError.OK;
                }

                if (bridgeMessage.Destination.Equals("All", StringComparison.InvariantCultureIgnoreCase) || bridgeMessage.Destination == "NATSCommunicator")
                {
                    switch (bridgeMessage.Subject)
                    {
                        case "Equipment.Initialization.Notify":
                            EquipmentInitialization(bridgeMessage);
                            break;

                        case "Equipment.Initialization.Reply":
                            EquipmentInitializationReply(bridgeMessage);
                            break;

                        case "Equipment.Event":
                            EquipmentEvent(bridgeMessage);
                            break;

                        case "Equipment.Alarm":
                            EquipmentAlarm(bridgeMessage);
                            break;

                        case "Equipment.CommControlState":
                            EquipmentCommControlState(bridgeMessage);
                            break;

                        case "Equipment.ProcessState":
                            EquipmentProcessState(bridgeMessage);
                            break;

                        case "Equipment.PPSelected":
                            HandleJobStateChange(bridgeMessage);
                            break;

                        case "Equipment.GetRecipeList.Reply":
                            EquipmentRecipeListReply(bridgeMessage);
                            break;

                        case "Equipment.EquipmentStatusRequest.Reply":
                            EquipmentStatusRequestReply(bridgeMessage);
                            break;
                        case "Equipment.EquipmentConstantRequest.Reply":
                            EquipmentConstantRequestReply(bridgeMessage);
                            break;

                        case "RecParameterValidation.Reply":
                            FormattedRecipeValidationReply(bridgeMessage);
                            break;

                        case "RecParameterUpload.Reply":
                            FormattedRecipeUploadReply(bridgeMessage);
                            break;

                        case "RecipeBodyUpload.Reply":
                            UnformattedRecipeUploadReply(bridgeMessage);
                            break;

                        case "RecipeBodyValidation.Reply":
                            UnformattedRecipeValidationReply(bridgeMessage);
                            break;

                        case "Equipment.RemoteCommand.Reply":
                            EquipmentRemoteCommandReply(bridgeMessage);
                            break;

                        case "JobStateChange":
                            HandleJobStateChange(bridgeMessage);
                            break;

                        case "MES.GetRecipeInfo":
                            mNATS.RequestGetRecipe(bridgeMessage as GetRecipeInfo);
                            break;

                        case "UnformattedPPValidation.Request.Reply":
                            UnformattedRecipeValidationReply(bridgeMessage);
                            break;

                        case "EMAP.Update":
                            HandleEMapsUpdate(bridgeMessage);
                            break;

                        case "Equipment.Report":
                            HandleCustomReportData(bridgeMessage);
                            break;
                        case "Equipment.ConstantReport":
                            HandleConstantReportData(bridgeMessage);
                            break;
                        case "Equipment.AMR":
                            HandleAMREvent(bridgeMessage);
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

        public void SendMessage(IBridgeMessage bridgeMessage)
        {
            mParent.ReceiveMessage(bridgeMessage);
        }

        #endregion

        #region Virtual Method

        protected virtual void EquipmentEvent(IBridgeMessage bridgeMessage)
        {

        }

        protected virtual void EquipmentInitialization(IBridgeMessage bridgeMessage)
        {
            var initializationReceive = new EAPMessages.Receive.InitializationMessage();
            initializationReceive.CopyData(bridgeMessage);

            var initializationMessage = new InitializationMessage();
            initializationMessage.Command = EnumType.Command.Initialization;
            initializationMessage.FWMachineName = initializationReceive.FWEquipmentID;
            initializationMessage.MachineName = initializationReceive.EquipmentID;
            initializationMessage.Text = initializationReceive.ErrorText;
            initializationMessage.Code = initializationReceive.ErrorCode.ToString();
            initializationMessage.Vids.Add("EquipmentModel", initializationReceive.EquipmentModel);
            initializationMessage.Vids.Add("SoftwareRevision", initializationReceive.SoftwareRevision);
            initializationMessage.Vids.Add("IPAddress", initializationReceive.IPAddress);
            initializationMessage.Vids.Add("Port", initializationReceive.Port);
            initializationMessage.Vids.Add("DeviceID", initializationReceive.DeviceID);
            initializationMessage.Vids.Add("ConnectionType", initializationReceive.ConnectionType);
            initializationMessage.Vids.Add("COMPort", initializationReceive.COMPort);
            initializationMessage.Vids.Add("BaudRate", initializationReceive.BaudRate);

            mNATS.Publish(initializationMessage);
        }

        protected virtual void EquipmentInitializationReply(IBridgeMessage bridgeMessage)
        {
            var initializationReceive = new EAPMessages.Receive.InitializationMessage();
            initializationReceive.CopyData(bridgeMessage);

            var initializationMessageReply = new InitializationMessage();
            initializationMessageReply.Command = EnumType.Command.Initialization;
            initializationMessageReply.FWMachineName = initializationReceive.FWEquipmentID;
            initializationMessageReply.MachineName = initializationReceive.EquipmentID;
            initializationMessageReply.Text = initializationReceive.ErrorText;
            initializationMessageReply.Code = initializationReceive.ErrorCode.ToString();
            initializationMessageReply.Vids.Add("EquipmentModel", initializationReceive.EquipmentModel);
            initializationMessageReply.Vids.Add("SoftwareRevision", initializationReceive.SoftwareRevision);
            initializationMessageReply.Vids.Add("IPAddress", initializationReceive.IPAddress);
            initializationMessageReply.Vids.Add("Port", initializationReceive.Port);
            initializationMessageReply.Vids.Add("DeviceID", initializationReceive.DeviceID);
            initializationMessageReply.Vids.Add("ConnectionType", initializationReceive.ConnectionType);
            initializationMessageReply.Vids.Add("COMPort", initializationReceive.COMPort);
            initializationMessageReply.Vids.Add("BaudRate", initializationReceive.BaudRate);

            mNATS.ReplyRequest(initializationReceive.MessageID, initializationMessageReply);
        }

        protected virtual void EquipmentAlarm(IBridgeMessage bridgeMessage)
        {
            var equipmentAlarm = new EquipmentAlarmPacket();
            equipmentAlarm.CopyData(bridgeMessage);

            var alarmMessage = new AlarmMessage();
            alarmMessage.Command = EnumType.Command.Alarm;
            alarmMessage.FWMachineName = equipmentAlarm.FWEquipmentID;
            alarmMessage.MachineName = equipmentAlarm.EquipmentID;
            alarmMessage.AlarmCode = equipmentAlarm.AlarmID.ToString();
            alarmMessage.AlarmText = equipmentAlarm.AlarmText;
            alarmMessage.AlarmType = (equipmentAlarm.AlarmSet >> 7) == 0x01 ? EnumType.AlarmType.Set : EnumType.AlarmType.Clear;

            mNATS.Publish(alarmMessage);
        }

        protected virtual void EquipmentCommControlState(IBridgeMessage bridgeMessage)
        {
            var equipmentCommControlState = bridgeMessage as EquipmentCommControlState;

            var commControlStateMessage = new CommControlStateMessage();
            commControlStateMessage.Command = EnumType.Command.CommunicationControlState;
            commControlStateMessage.FWMachineName = equipmentCommControlState.FWEquipmentID;
            commControlStateMessage.MachineName = equipmentCommControlState.EquipmentID;
            commControlStateMessage.CommunicationState = equipmentCommControlState.CommState.ToEnum<EnumType.CommunicationState>();
            commControlStateMessage.ControlState = equipmentCommControlState.ControlState.ToEnum<EnumType.ControlState>();

            mNATS.Publish(commControlStateMessage);
        }

        protected virtual void EquipmentProcessState(IBridgeMessage bridgeMessage)
        {
            var equipmentProcessState = bridgeMessage as EquipmentProcessState;

            var processStateMessage = new ProcessStateMessage();
            processStateMessage.Command = EnumType.Command.ProcessState;
            processStateMessage.FWMachineName = equipmentProcessState.FWEquipmentID;
            processStateMessage.MachineName = equipmentProcessState.EquipmentID;
            processStateMessage.PreviousProcessState = equipmentProcessState.PreviousProcessState.ToEnum<EnumType.ProcessState>();
            processStateMessage.CurrentProcessState = equipmentProcessState.CurrentProcessState.ToEnum<EnumType.ProcessState>();

            mNATS.Publish(processStateMessage);
        }

        protected virtual void EquipmentRecipeListReply(IBridgeMessage bridgeMessage)
        {
            var equipmentRecipeList = bridgeMessage as EquipmentRecipeList;
            
            var recipeList = new RecipeMessage();
            recipeList.Command = EnumType.Command.RecList;
            recipeList.FWMachineName = equipmentRecipeList.FWEquipmentID;
            recipeList.MachineName = equipmentRecipeList.EquipmentID;
            recipeList.Code = equipmentRecipeList.ErrorCode.ToString();
            recipeList.Text = equipmentRecipeList.ErrorText;
            int i = 1;

            foreach (var ppid in equipmentRecipeList.Recipes)
            {
                recipeList.AddPPID("PPID_{0}".FillArguments(i++) , ppid);
            }

            mNATS.ReplyRequest(bridgeMessage.MessageID, recipeList);
        }

        protected virtual void EquipmentStatusRequestReply(IBridgeMessage bridgeMessage)
        {
            var equipmentStatusRequest = bridgeMessage as EquipmentStatusPacket;

            var variableMessage = new VariableMessage();
            variableMessage.Command = EnumType.Command.VIDQuery;
            variableMessage.FWMachineName = equipmentStatusRequest.FWEquipmentID;
            variableMessage.MachineName = equipmentStatusRequest.EquipmentID;

            foreach (var vid in equipmentStatusRequest.DictSV)
            {
                variableMessage.AddVid(vid.Key, vid.Value);
            }

            mNATS.ReplyRequest(bridgeMessage.MessageID, variableMessage);
        }

        protected virtual void EquipmentConstantRequestReply(IBridgeMessage bridgeMessage)
        {
            var equipmentConstantRequest = bridgeMessage as EquipmentConstantPacket;

            var variableMessage = new VariableMessage();
            variableMessage.Command = EnumType.Command.VIDQuery;
            variableMessage.FWMachineName = equipmentConstantRequest.FWEquipmentID;
            variableMessage.MachineName = equipmentConstantRequest.EquipmentID;

            foreach (var ecid in equipmentConstantRequest.DictEC)
            {
                variableMessage.AddEcid(ecid.Key, ecid.Value);
            }

            mNATS.ReplyRequest(bridgeMessage.MessageID, variableMessage);
        }

        protected virtual void FormattedRecipeValidationReply(IBridgeMessage bridgeMessage)
        {
            var formattedRecipeValidation = bridgeMessage as RecParameterValidation;

            var formattedRecipeMessage = new RecipeParameterMessage();
            formattedRecipeMessage.Command = EnumType.Command.RecParameterValidation;
            formattedRecipeMessage.FWMachineName = formattedRecipeValidation.FWEquipmentID;
            formattedRecipeMessage.MachineName = formattedRecipeValidation.EquipmentID;
            formattedRecipeMessage.PPID = formattedRecipeValidation.PPID;
            formattedRecipeMessage.Code = formattedRecipeValidation.Code.ToString();
            formattedRecipeMessage.Text = formattedRecipeValidation.Text;

            foreach (var param in formattedRecipeValidation.RecipeParam)
            {
                if (param.ParameterErrorText != "OK")
                {
                    formattedRecipeMessage.AddRecipeParameter(new RecipeParameter()
                    {
                        Name = param.Name,
                        ParameterErrorText = param.ParameterErrorText
                    });
                }
            }

            mNATS.ReplyRequest(bridgeMessage.MessageID, formattedRecipeMessage);
        }

        protected virtual void FormattedRecipeUploadReply(IBridgeMessage bridgeMessage)
        {
            var formattedRecipeUpload = bridgeMessage as RecParameterUpload;

            var formattedRecipeMessage = new RecipeParameterMessage();
            formattedRecipeMessage.Command = EnumType.Command.RecParameterUpload;
            formattedRecipeMessage.PPID = formattedRecipeUpload.PPID;
            formattedRecipeMessage.FWMachineName = formattedRecipeUpload.FWEquipmentID;
            formattedRecipeMessage.MachineName = formattedRecipeUpload.EquipmentID;
            formattedRecipeMessage.Code = formattedRecipeUpload.ErrorCode.ToString();
            formattedRecipeMessage.Text = formattedRecipeUpload.ErrorCode == EAPError.OK ? "OK" : formattedRecipeUpload.ErrorText;

            foreach (var param in formattedRecipeUpload.RecipeParamUpload)
            {
                formattedRecipeMessage.AddRecipeParameter(new RecipeParameter()
                {
                    Name = param.Name,
                    Value = param.Value,
                    DataType = param.DataType
                });
            }

            mNATS.ReplyRequest(bridgeMessage.MessageID, formattedRecipeMessage);
        }

        protected virtual void UnformattedRecipeUploadReply(IBridgeMessage bridgeMessage)
        {
            var unformattedRecipeUpload = bridgeMessage as UnformattedRecipeUpload;

            var unformattedRecipeMessage = new RecipeBodyMessage();
            unformattedRecipeMessage.Command = EnumType.Command.RecBodyUpload;
            unformattedRecipeMessage.PPID = unformattedRecipeUpload.PPID;
            unformattedRecipeMessage.FilePath = unformattedRecipeUpload.FilePath;
            unformattedRecipeMessage.FWMachineName = unformattedRecipeUpload.FWEquipmentID;
            unformattedRecipeMessage.MachineName = unformattedRecipeUpload.EquipmentID;
            unformattedRecipeMessage.Code = unformattedRecipeUpload.ErrorCode.ToString();
            unformattedRecipeMessage.Text = unformattedRecipeUpload.ErrorCode == EAPError.OK  ? "OK" : unformattedRecipeUpload.ErrorText;

            mNATS.ReplyRequest(bridgeMessage.MessageID, unformattedRecipeMessage);
        }

        protected virtual void UnformattedRecipeValidationReply(IBridgeMessage bridgeMessage)
        {
            var unformattedRecipeUpload = bridgeMessage as UnformattedRecipeUpload;

            var unformattedRecipeMessage = new RecipeBodyMessage();
            unformattedRecipeMessage.Command = EnumType.Command.RecBodyUpload;
            unformattedRecipeMessage.FWMachineName = unformattedRecipeUpload.FWEquipmentID;
            unformattedRecipeMessage.MachineName = unformattedRecipeUpload.EquipmentID;
            unformattedRecipeMessage.Code = unformattedRecipeUpload.ErrorCode.ToString();
            unformattedRecipeMessage.Text = unformattedRecipeUpload.ErrorCode == EAPError.OK ? "OK" : unformattedRecipeUpload.ErrorText;

            mNATS.ReplyRequest(bridgeMessage.MessageID, unformattedRecipeMessage);
        }

        protected virtual void EquipmentRemoteCommandReply(IBridgeMessage bridgeMessage)
        {
            var remoteCommand = bridgeMessage as RemoteCommand;

            var remoteCommandMessage = new RemoteCommandMessage();
            remoteCommandMessage.Command = EnumType.Command.RemoteCommand;
            remoteCommandMessage.CommandType = remoteCommand.Command.ToEnum<EnumType.RemoteCommand>();
            remoteCommandMessage.FWMachineName = remoteCommand.FWEquipmentID;
            remoteCommandMessage.MachineName = remoteCommand.EquipmentID;
            remoteCommandMessage.JobId = remoteCommand.JobID;
            remoteCommandMessage.LoadPort = remoteCommand.LoadPort;
            remoteCommandMessage.LotSize = remoteCommand.LotSize;
            remoteCommandMessage.MagazineCount = remoteCommand.MagazineCount;
            remoteCommandMessage.PPID = remoteCommand.PPID;
            remoteCommandMessage.Code = remoteCommand.ErrorCode.ToString();
            remoteCommandMessage.Text = remoteCommand.ErrorText;

            Logger.LogHelper.LogDebug("Text={0}".FillArguments(remoteCommandMessage.Text));

            mNATS.ReplyRequest(bridgeMessage.MessageID, remoteCommandMessage);
        }

        protected virtual void HandleJobStateChange(IBridgeMessage bridgeMessage)
        {
            var jobStateChange = bridgeMessage as JobStateChange;

            var jobStateChangeMessage = new JobMessage();
            jobStateChangeMessage.Command = EnumType.Command.JobStateChanged;

            mLogger.LogHelper.LogDebug(jobStateChange == null ? "Null" : "Not null");
            mLogger.LogHelper.LogDebug("CurrentJobState={0}".FillArguments(jobStateChange.CurrentJobState));
            mLogger.LogHelper.LogDebug("PreviousJobState={0}".FillArguments(jobStateChange.PreviousJobState));

            jobStateChangeMessage.FWMachineName = jobStateChange.FWEquipmentID;
            jobStateChangeMessage.MachineName = jobStateChange.EquipmentID;
            jobStateChangeMessage.Device = jobStateChange.Device;
            jobStateChangeMessage.Package = jobStateChange.Package;
            jobStateChangeMessage.Lot = jobStateChange.Lot;
            jobStateChangeMessage.PPID = jobStateChange.PPID;
            jobStateChangeMessage.JobId = jobStateChange.JobID;
            jobStateChangeMessage.CurrentProcessState = jobStateChange.CurrentJobState.ToEnum<EnumType.JobState>();
            jobStateChangeMessage.PreviousProcessState = jobStateChange.PreviousJobState.ToEnum<EnumType.JobState>();
            jobStateChangeMessage.Code = EAPError.OK.ToString();
            jobStateChangeMessage.Text = "OK";

            mNATS.Publish(jobStateChangeMessage);
        }

        protected virtual void HandleEMapsUpdate(IBridgeMessage bridgeMessage)
        {
            var emapsUpdate = bridgeMessage as EMapsUpdate;

            var emapsUpdateMessage = new EmapsUpdateStripMessage();
            emapsUpdateMessage.Command = EnumType.Command.EmapsUpdateStrip;

            emapsUpdateMessage.FWMachineName = emapsUpdate.FWEquipmentID;
            emapsUpdateMessage.MachineName = emapsUpdate.EquipmentID;
            emapsUpdateMessage.StripId = emapsUpdate.StripID;
            emapsUpdateMessage.Path = emapsUpdate.Path;
            emapsUpdateMessage.TimeOut = emapsUpdate.Timeout;
            emapsUpdateMessage.Code = EAPError.OK.ToString();
            emapsUpdateMessage.Text = "OK";

            mNATS.PublishEMapsUpdate(emapsUpdateMessage);
        }

        protected virtual void HandleCustomReportData(IBridgeMessage bridgeMessage)
        {
            var sv = bridgeMessage as EquipmentStatusPacket;
           
            var reportMessage = new ReportMessage();
            reportMessage.FWMachineName = sv.FWEquipmentID;
            reportMessage.MachineName = sv.EquipmentID;
            reportMessage.Command = EnumType.Command.Report;

            mNATS.Publish(reportMessage);
        }

        protected virtual void HandleConstantReportData(IBridgeMessage bridgeMessage)
        {
            var ec = bridgeMessage as EquipmentConstantPacket;

            var reportMessage = new ReportMessage();
            reportMessage.FWMachineName = ec.FWEquipmentID;
            reportMessage.MachineName = ec.EquipmentID;
            reportMessage.Command = EnumType.Command.Report;

            mNATS.Publish(reportMessage);
        }

        protected virtual void HandleAMREvent(IBridgeMessage bridgeMessage)
        {
            var amrEv = bridgeMessage as AMREvent;

            var amrMessage = new AMRMessage();
            amrMessage.FWMachineName = amrEv.FWEquipmentID;
            amrMessage.MachineName = amrEv.EquipmentID;
            amrMessage.Action = amrEv.Action;
            amrMessage.StripCount = amrEv.StripCount;
            amrMessage.Lot = amrEv.LotID;

            amrMessage.Command = EnumType.Command.AMR;

            mNATS.Publish(amrMessage);
        }

        protected void ReplyRequest(string replySubject, BaseMessage message)
        {
            mNATS.ReplyRequest(replySubject, message);
        }

        #endregion
    }
}
