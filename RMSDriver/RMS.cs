using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Qynix.EAP.Drivers.RMSDriver
{
    using Base.BaseInterfaces;
    using Base.BridgeMessage;
    using Utilities.ExtensionPlug;
    using Utilities.FileUtilities;
    using EAPMessages.Receive;
    using Base.BridgeMessage.Common;
    using RecValidationHandling;

    public class RMS : IEAPDriver
    {
        #region Private Field

        private IEAPDriver mParent;
        private string mEAPConfigFolder;
        private string mFWEquipmentName;
        private string mEquipmentName;
        private string mRecipeUploadPath;
        private bool mPass = true;
        private string mErrorText;
        private List<BridgeMessagePacket> mMessageList;
        private Dictionary<RecParameterValidation.EnumValidationRule, Delegate> mValidationRules;

        private Helper mHelper;
        private Logger mLogger;

        #endregion

        #region Properties

        protected string FWEquipmentName { get { return mFWEquipmentName; } }

        protected string EquipmentName { get { return mEquipmentName; } }

        protected string RecipeUploadPath { get { return mRecipeUploadPath; } }

        protected Logger Logger { get { return mLogger; } }

        protected Helper Helper { get { return mHelper; } }

        protected List<BridgeMessagePacket> MessageList
        {
            get { return mMessageList; }
        }

        protected IEAPDriver Parent { get { return mParent; } }

        #endregion

        #region Public Field

        #endregion

        #region Constructor

        public RMS()
        {
            mHelper = new Helper();
            mLogger = new Logger();
            ValidationHandling initRules = new ValidationHandling();
            mValidationRules = new Dictionary<RecParameterValidation.EnumValidationRule, Delegate>();
            mValidationRules = initRules.ValidationRules;
        }

        #endregion

        #region IEAPDriver Method

        public int AssignParent(IBridgeMessage bridgeMessage, object parent)
        {
            mParent = parent as IEAPDriver;
            return 0;
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
                var initializationMessage = new InitializationMessage();
                initializationMessage.CopyData(bridgeMessage);

                mEAPConfigFolder = initializationMessage.EAPConfigFolder;
                mFWEquipmentName = initializationMessage.FWEquipmentName;
                mEquipmentName = initializationMessage.EquipmentName;
                var logDir = initializationMessage.LogDir;
                var logFileName = "RMS_{0}.log".FillArguments(mEquipmentName);

                mLogger.Initialize(mEquipmentName, Path.Combine(logDir, logFileName));
                mLogger.LogHelper.LogInfo("Initializing RMS Driver for {0}.{1}...".FillArguments(mFWEquipmentName, mEquipmentName));

                var configFile = mHelper.GetConfigFilePath(mEAPConfigFolder, mEquipmentName);

                if (!mHelper.LoadConfiguration(configFile))
                {
                    return EAPError.UNHANDLED_ERR;
                }
                else
                {
                    mMessageList = new List<BridgeMessagePacket>();
                }

                //var paramsFile = mHelper.GetParamsFilePath(mEAPConfigFolder, mEquipmentName);
                //mHelper.LoadParameters(paramsFile);

                mRecipeUploadPath = mHelper.GetRecipeUploadPath(mEquipmentName);

                mLogger.LogHelper.LogDebug("RecipeUploadPath={0}".FillArguments(mRecipeUploadPath));

                if (!string.IsNullOrEmpty(mRecipeUploadPath))
                {
                    try
                    {
                        var result1 = mHelper.MapAllPath(mFWEquipmentName, mEquipmentName);
                        if (result1 != EAPError.OK)
                        {
                            mLogger.LogHelper.LogInfo("RMSDriver map path failed.");
                            return result1;
                        }
                    }
                    catch (Exception ex)
                    {
                        mLogger.LogHelper.LogException(ex);
                    }

                    var result = CheckMapPathExist();

                    if (result != EAPError.OK)
                    {
                        mLogger.LogHelper.LogInfo("RMS Driver initialization failed.");
                        return result;
                    }

                    var text = string.Empty;
                    FileHelper.CreateDirIfNotExist(mRecipeUploadPath, out text);

                    mLogger.LogHelper.LogInfo(text);
                }
                
                mLogger.LogHelper.LogInfo("RMS Driver initialization done for {0}.{1}".FillArguments(mFWEquipmentName, mEquipmentName));

                return EAPError.OK;
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
                mLogger.LogHelper.LogInfo(
                    "Received Sender:{0}, Destination:{1}, Subject:{2}".FillArguments(
                        bridgeMessage.Sender,
                        bridgeMessage.Destination,
                        bridgeMessage.Subject));

                var fwEquipmentName = bridgeMessage.GetBasicData("FWEQUIPMENTID")?.Value?.ToString();
                var equipmentName = bridgeMessage.GetBasicData("EQUIPMENTID")?.Value?.ToString();

                if (!string.IsNullOrEmpty(fwEquipmentName) && !string.IsNullOrEmpty(equipmentName))
                {
                    if (!(fwEquipmentName == mFWEquipmentName && mEquipmentName == equipmentName))
                    {
                        return EAPError.OK;
                    }
                }

                switch (bridgeMessage.Subject)
                {
                    case "Equipment.PPBodyUpload":
                        HandleEquipmentPPUpload(bridgeMessage);
                        break;

                    case "Equipment.PPBody.Request":
                        return HandleEquipmentPPRequest(bridgeMessage);

                    case "RecParameterValidation.Request":
                        return HandleFormattedPPValidationRequest(bridgeMessage);

                    case "FormattedPPUpload.Request":
                        HandleFormattedPPUploadRequest(bridgeMessage);
                        break;

                    case "RecipeBodyUpload":
                        HandleUnformattedPPUploadRequest(bridgeMessage);
                        break;

                    case "UnformattedPPValidation.Request":
                        HandleEquipmentPPValidate(bridgeMessage);
                        break;

                    case "Equipment.FormattedPPUpload.Reply":
                        //var recParamUploadReply = bridgeMessage as RecParameterUpload;

                        //recParamUploadReply.Sender = "RMSDriver";
                        //recParamUploadReply.Destination = "NATSCommunicationDriver";
                        //recParamUploadReply.Subject = "RecParameterUpload.Reply";

                        //mParent.ReceiveMessage(recParamUploadReply);

                        HandleFormattedPPUploadReply(bridgeMessage);
                        break;

                    case "Equipment.PPBody.Send":
                        HandleEquipmentPPSend(bridgeMessage);
                        break;

                    default:
                        break;
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

        protected virtual int HandleEquipmentPPUpload(IBridgeMessage bridgeMessage)
        {
            try
            {
                var ppBodyUpload = bridgeMessage as UnformattedRecipeUpload;
                Logger.LogHelper.LogDebug("Uploading recipe to {0}.".FillArguments(Path.Combine(mRecipeUploadPath, ppBodyUpload.PPID)));
                return UploadRecipeToShareDrive(ppBodyUpload.PPID, ppBodyUpload.PPBody);
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
                return EAPError.UNKNOWN_ERR;
            }
        }

        protected virtual int HandleEquipmentPPValidate(IBridgeMessage bridgeMessage)
        {
            try
            {
                var ppBodyUpload = bridgeMessage as UnformattedRecipeUpload;
                mMessageList.Add(ppBodyUpload);

                var newMessage = new RecParameterUpload(mFWEquipmentName, mEquipmentName, ppBodyUpload.PPID, null);
                newMessage.MessageID = ppBodyUpload.MessageID;
                newMessage.Sender = "RMSDriver";
                newMessage.Destination = "SECSDriver";
                newMessage.Subject = "Equipment.PPBody.Request";
                newMessage.FilePath = mHelper.GetRecipeUploadPath(mEquipmentName);//Kelvin
                newMessage.CompileData();
                mParent.ReceiveMessage(newMessage);

                return EAPError.OK;
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
                return EAPError.UNKNOWN_ERR;
            }
        }

        protected virtual int HandleEquipmentPPRequest(IBridgeMessage bridgeMessage)
        {
            try
            {
                var ppBodyRequest = bridgeMessage as UnformattedRecipeUpload;

                mMessageList.Add(ppBodyRequest);

                mLogger.LogHelper.LogDebug("MessageId={0}".FillArguments(bridgeMessage.MessageID));

                return ReadRecipeFromShareDrive(ppBodyRequest.PPID, ppBodyRequest.MessageID);
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
                return EAPError.UNKNOWN_ERR;
            }
        }

        protected virtual int HandleUnformattedPPUploadRequest(IBridgeMessage bridgeMessage)
        {
            try
            {
                var recBodyUploadRequest = bridgeMessage as UnformattedRecipeUpload;
                mMessageList.Add(recBodyUploadRequest);

                var newMessage = new RecParameterUpload(mFWEquipmentName, mEquipmentName, recBodyUploadRequest.PPID, null);
                newMessage.MessageID = recBodyUploadRequest.MessageID;
                newMessage.Sender = "RMSDriver";
                newMessage.Destination = "SECSDriver";
                newMessage.Subject = "Equipment.PPBody.Request";
                newMessage.FilePath = mHelper.GetRecipeUploadPath(mEquipmentName);
                newMessage.CompileData();
                mParent.ReceiveMessage(newMessage);

                return EAPError.OK;
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
                return EAPError.UNKNOWN_ERR;
            }
        }

        protected virtual int HandleFormattedPPUploadRequest(IBridgeMessage bridgeMessage)
        {
            try
            {
                var recParamUploadRequest = bridgeMessage as RecParameterUpload;
                mMessageList.Add(recParamUploadRequest);

                var newMessage = new RecParameterUpload(mFWEquipmentName, mEquipmentName, recParamUploadRequest.PPID, null);
                newMessage.MessageID = recParamUploadRequest.MessageID;
                newMessage.Sender = "RMSDriver";
                newMessage.Destination = "SECSDriver";
                newMessage.Subject = "Equipment.FormattedPPUpload.Request";
                newMessage.CompileData();
                mParent.ReceiveMessage(newMessage);

                return EAPError.OK;
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
                return EAPError.UNKNOWN_ERR;
            }
        }

        protected virtual int HandleFormattedPPValidationRequest(IBridgeMessage bridgeMessage)
        {
            try
            {
                var recParamValidationRequest = bridgeMessage as RecParameterValidation;
                mMessageList.Add(recParamValidationRequest);

                var newMessage = new RecParameterUpload(mFWEquipmentName, mEquipmentName, recParamValidationRequest.PPID, null);
                newMessage.MessageID = recParamValidationRequest.MessageID;
                newMessage.Sender = "RMSDriver";
                newMessage.Destination = "SECSDriver";
                newMessage.Subject = "Equipment.FormattedPPUpload.Request";
                newMessage.CompileData();
                mParent.ReceiveMessage(newMessage);

                return EAPError.OK;
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
                return EAPError.UNKNOWN_ERR;
            }
        }

        protected virtual int HandleFormattedPPUploadReply(IBridgeMessage bridgeMessage)
        {
            try
            {
                var message = MessageList.FirstOrDefault(x => x.MessageID == bridgeMessage.MessageID);

                var recipeParameterUpload = bridgeMessage as RecParameterUpload;

                if (message != null)
                {
                    Logger.LogHelper.LogDebug("Found message id={0}".FillArguments(message.MessageID));
                    Logger.LogHelper.LogDebug("Message.Subject={0}".FillArguments(message.Subject));

                    var recipeParamList = recipeParameterUpload.RecipeParamUpload;
                    FilterRecipeParameter(ref recipeParamList);

                    switch (message.Subject)
                    {
                        case "FormattedPPUpload.Request":
                            var recipeParameters = new RecParameterUpload(FWEquipmentName, EquipmentName, recipeParameterUpload.PPID, recipeParamList);
                            recipeParameters.Sender = "RMSDriver";
                            recipeParameters.Destination = "NATSCommunicator";
                            recipeParameters.Subject = "RecParameterUpload.Reply";
                            recipeParameters.MessageID = message.MessageID;
                            if (recipeParameterUpload.ErrorCode != EAPError.OK)
                            {
                                recipeParameters.ErrorCode = recipeParameterUpload.ErrorCode;
                                recipeParameters.ErrorText = recipeParameterUpload.ErrorText;
                            }

                            Parent.ReceiveMessage(recipeParameters);
                            break;

                        case "RecParameterValidation.Request":
                            var recParamValidate = message as RecParameterValidation;

                            RecipeParameterChecking(ref recParamValidate, recipeParamList);
                            recParamValidate.Sender = "RMSDriver";
                            recParamValidate.Destination = "NATSCommunicator";
                            recParamValidate.Subject = "RecParameterValidation.Reply";
                            recParamValidate.MessageID = message.MessageID;

                            Parent.ReceiveMessage(recParamValidate);
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

        protected virtual int HandleEquipmentPPSend(IBridgeMessage bridgeMessage)
        {
            try
            {
                var ppBodySend = bridgeMessage as UnformattedRecipeUpload;
                mMessageList.Add(ppBodySend);

                mLogger.LogHelper.LogDebug("MessageId={0}".FillArguments(bridgeMessage.MessageID));

                return ReadRecipeFromShareDrive(ppBodySend.PPID, ppBodySend.MessageID);
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
                return EAPError.UNKNOWN_ERR;
            }
        }

        protected int ReadFileFromShareDrive(string path, string replySubject, Action<object, bool, string[]> waitOrTimeoutCallback)
        {
            try
            {
                FileHelper.AsyncReadFile(
                    path,
                    replySubject,
                    3,
                    3000,
                    waitOrTimeoutCallback);

                return EAPError.OK;
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
                return EAPError.UNKNOWN_ERR;
            }
        }

        protected virtual void RecipeBodyChecksumCompareCallBack(object state, bool result, bool checksumEqual)
        {
            try
            {
                var info = state as string[];
                var path = info[0];
                var replySubject = info[1];
                var msg = info[2];

                mLogger.LogHelper.LogDebug("ReplySubject={0}".FillArguments(replySubject));

                if (result)
                {
                    mLogger.LogHelper.LogInfo(msg);

                    var replyMessage = (from a in mMessageList
                                        where a.MessageID == replySubject
                                        select a).FirstOrDefault() as UnformattedRecipeUpload;

                    if (replyMessage == null)
                        mLogger.LogHelper.LogError("Message not found for MessageID:{0}".FillArguments(replySubject));

                    var sender = replyMessage.Sender;
                    mLogger.LogHelper.LogDebug("Sender={0}".FillArguments(sender));

                    replyMessage.Subject = replyMessage.Subject + ".Reply";
                    replyMessage.Destination = sender;
                    replyMessage.Sender = "RMSDriver";
                    replyMessage.Result = true;

                    if (checksumEqual)
                    {
                        replyMessage.ErrorCode = EAPError.OK;
                        replyMessage.ErrorText = "OK";
                    }
                    else
                    {
                        replyMessage.ErrorCode = EAPError.RECIPE_BODY_VALIDATE_FAILED;
                        replyMessage.ErrorText = "Recipe body validation failed.";
                    }

                    replyMessage.CompileData();
                    mParent.ReceiveMessage(replyMessage);
                }
                else
                {
                    var replyMessage = (from a in mMessageList
                                        where a.MessageID == replySubject
                                        select a).FirstOrDefault() as UnformattedRecipeUpload;

                    var sender = replyMessage.Sender;

                    replyMessage.ErrorCode = EAPError.RECIPE_BODY_VALIDATE_FAILED;
                    replyMessage.ErrorText = msg;
                    replyMessage.Subject = replyMessage.Subject + ".Reply";
                    replyMessage.Destination = sender;
                    replyMessage.Sender = "RMSDriver";
                    replyMessage.Result = false;
                    replyMessage.CompileData();

                    mParent.ReceiveMessage(replyMessage);
                }
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }

        #endregion

        #region Private Method

        protected void PPBodyValidate(string recipeFilePath, byte[] ppBody, string replySubject)
        {
            try
            {
                FileHelper.AsyncMD5ChecksumCompare(recipeFilePath, ppBody, replySubject, 3, 1000, RecipeBodyChecksumCompareCallBack);
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }

        protected void RecipeParameterChecking(ref RecParameterValidation message, List<RecParameterUpload.RecipeParameterUploadData> recParamList)
        {
            try
            {
                string equipment = message.EquipmentID;

                mPass = true;
                foreach (var receiveParam in recParamList)
                {
                    string name = receiveParam.Name;
                    string value = receiveParam.Value;

                    var trackParamListTemp = message.RecipeParam;
                    foreach (var trackParam in trackParamListTemp)
                    {
                        if (trackParam.Name == name)
                        {
                            string returnText = GetValidationResult(trackParam, value);
                            if (!string.IsNullOrEmpty(returnText))
                            {
                                if (returnText != "OK")
                                {
                                    trackParam.ParameterErrorText = returnText;
                                    mPass = false;
                                    addErrorText("Parameter {" + name + "} - " + returnText);
                                }
                                else
                                {
                                    trackParam.ParameterErrorText = returnText;
                                }
                            }
                        }
                    }
                }

                if (!mPass)
                {
                    message.Code = EAPError.PARAM_VALIDATE_FAILED.ToString();
                    message.Text = "Recipe Parameter Validation Failed. {0}".FillArguments(mErrorText);
                    removeErrorText();
                }
                else
                {
                    message.Code = "0";
                    message.Text = "OK";
                    message.RecipeParam.Clear();
                }
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
            finally
            {
                mPass = true;
            }
        }

        private string GetValidationResult(RecParameterValidation.RecipeParameter param, string value)
        {
            try
            {
                switch (param.ValidationRule)
                {
                    case RecParameterValidation.EnumValidationRule.Range:
                        return mValidationRules[param.ValidationRule].DynamicInvoke(new object[] { Convert.ToSingle(param.MinValue), Convert.ToSingle(param.MaxValue), Convert.ToSingle(value) }).ToString();
                    case RecParameterValidation.EnumValidationRule.Equal:
                        //add by Fahmi 24/8/2020 - fix bug equal validation rule
                        return mValidationRules[param.ValidationRule].DynamicInvoke(new object[] { param.Value, value }).ToString();
                    case RecParameterValidation.EnumValidationRule.LessThan:
                        return mValidationRules[param.ValidationRule].DynamicInvoke(new object[] { Convert.ToSingle(param.Value), Convert.ToSingle(value) }).ToString();
                    case RecParameterValidation.EnumValidationRule.MoreThan:
                        return mValidationRules[param.ValidationRule].DynamicInvoke(new object[] { Convert.ToSingle(param.Value), Convert.ToSingle(value) }).ToString();
                    case RecParameterValidation.EnumValidationRule.LessThanEqual:
                        return mValidationRules[param.ValidationRule].DynamicInvoke(new object[] { Convert.ToSingle(param.Value), Convert.ToSingle(value) }).ToString();
                    case RecParameterValidation.EnumValidationRule.MoreThanEqual:
                        return mValidationRules[param.ValidationRule].DynamicInvoke(new object[] { Convert.ToSingle(param.Value), Convert.ToSingle(value) }).ToString();
                    case RecParameterValidation.EnumValidationRule.Contains:
                        return mValidationRules[param.ValidationRule].DynamicInvoke(new object[] { param.Value, value }).ToString();
                    case RecParameterValidation.EnumValidationRule.NotContains:
                        return mValidationRules[param.ValidationRule].DynamicInvoke(new object[] { param.Value, value }).ToString();
                    default:
                        return "";
                }
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
                return "";
            }
        }

        private int UploadRecipeToShareDrive(string ppid, byte[] ppBody)
        {
            try
            {
                FileHelper.AsyncWriteBinaryFile(Path.Combine(mRecipeUploadPath, ppid), ppBody, 3, 3000, UploadRecipeToShareDriveCallBack);

                return EAPError.OK;
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
                return EAPError.UNKNOWN_ERR;
            }
        }

        private int ReadRecipeFromShareDrive(string ppid, string replySubject)
        {
            try
            {

                FileHelper.AsyncReadBinaryFile(
                    Path.Combine(mRecipeUploadPath, ppid),
                    replySubject,
                    3,
                    3000,
                    ReadRecipeFromShareDriveCallBack);

                return EAPError.OK;
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
                return EAPError.UNKNOWN_ERR;
            }
        }

        private void UploadRecipeToShareDriveCallBack(object state, bool result)
        {
            try
            {
                var msg = (state as string[])[1];

                if (result)
                    mLogger.LogHelper.LogInfo(msg);
                else
                    mLogger.LogHelper.LogError(msg);
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        } 

        private void ReadRecipeFromShareDriveCallBack(object state, bool result, byte[] data)
        {
            try
            {
                var info = state as string[];
                var path = info[0];
                var replySubject = info[1];
                var msg = info[2];

                mLogger.LogHelper.LogDebug("ReplySubject={0}".FillArguments(replySubject));

                if (result)
                {
                    mLogger.LogHelper.LogInfo(msg);

                    var ppid = Path.GetFileName(path);
                    var replyMessage = (from a in mMessageList
                                        where a.MessageID == replySubject
                                        select a).FirstOrDefault() as UnformattedRecipeUpload;

                    if (replyMessage == null)
                        mLogger.LogHelper.LogError("Message not found for MessageID:{0}".FillArguments(replySubject));

                    var sender = replyMessage.Sender;
                    mLogger.LogHelper.LogDebug("Sender={0}".FillArguments(sender));

                    replyMessage.Subject = replyMessage.Subject + ".Reply";
                    replyMessage.Destination = sender;
                    replyMessage.Sender = "RMSDriver";
                    replyMessage.PPBody = data;
                    replyMessage.Result = true;
                    replyMessage.CompileData();

                    mParent.ReceiveMessage(replyMessage);
                }
                else
                {
                    var replyMessage = (from a in mMessageList
                                        where a.MessageID == replySubject
                                        select a).FirstOrDefault() as UnformattedRecipeUpload;
                    var sender = replyMessage.Sender;

                    replyMessage.Subject = replyMessage.Subject + ".Reply";
                    replyMessage.Destination = sender;
                    replyMessage.Sender = "RMSDriver";
                    replyMessage.PPBody = null;
                    replyMessage.Result = false;
                    replyMessage.CompileData();

                    mParent.ReceiveMessage(replyMessage);
                }

                mLogger.LogHelper.LogInfo(msg);
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }

        private int CheckMapPathExist()
        {
            try
            {
                var pathInfos = mHelper.Configuration.MapPath.Path;

                foreach (var pathInfo in pathInfos)
                {
                    if (!Directory.Exists(pathInfo.MapPath))
                    {
                        mLogger.LogHelper.LogError("Map path not found, PathName={0}, MapPath={1}".FillArguments(pathInfo.PathName, pathInfo.MapPath));
                        return EAPError.DIR_NOT_FOUND;
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

        private void FilterRecipeParameter(ref List<RecParameterUpload.RecipeParameterUploadData> recParamDataList)
        {
            try
            {
                var paramList = Helper.Configuration.Parameters.Param;

                var filter1 =
                    recParamDataList.Select(x => x.Name.ToUpper()).Intersect(paramList.Select(x => x.Name.ToUpper()));

                var filter2 =
                    from a in recParamDataList
                    where filter1.Contains(a.Name)
                    select new RecParameterUpload.RecipeParameterUploadData
                    {
                        Name = a.Name.ToUpper(),
                        Value = Helper.NewValue(a.Value, a.Name.ToUpper()),
                        DataType = paramList.First(x => x.Name == a.Name.ToUpper()).DataType
                    };

                recParamDataList = filter2.ToList();
            }
            catch (Exception ex)
            {
                mLogger.LogHelper.LogException(ex);
            }
        }
        private void addErrorText(string errorText)
        {
            if (string.IsNullOrEmpty(mErrorText))
            {
                mErrorText = errorText;
            }
        }
        private void removeErrorText()
        {
            mErrorText = string.Empty;
        }
        #endregion
    }
}
