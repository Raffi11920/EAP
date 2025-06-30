using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SECSDriver.Tasks
{
    using EventManager;
    using EventManager.EventArguments;
    using SECS.SECSFramework;
    using System.Threading;
    using Utilities.ExtensionPlug;

    internal class SECSGEMActionTask
    {

        #region Private Field

        private SECSEngine mSECSEngine;
        private Drivers.SECSDriver.Logger mLogger;
        private Helper mHelper;
        private AutoResetEvent mWaitAction;

        #endregion

        #region Public Field

        public event EventDelegates.SECSGEMActionEventHandler OnActionCompleted;

        #endregion

        #region Constructor

        public SECSGEMActionTask(SECSEngine secsEngine, Helper helper, Drivers.SECSDriver.Logger logger)
        {
            mSECSEngine = secsEngine;
            mHelper = helper;
            mLogger = logger;
            mWaitAction = new AutoResetEvent(true);
        }

        #endregion

        #region Public Method

        public Task RunAction(string actionName, bool replyRequired, string messageId)
        {
            return 
                Task.Run(() =>
                {
                    // Store value first, prevent asynchronous value happen
                    var storeReplyRequired = replyRequired;
                    var storeMessageId = messageId;
                    var errorCode = 0;
                    var errorText = "OK";

                    mWaitAction.WaitOne();

                    var action =
                        (from a in mHelper.Configuration.SECSGEMActions.Action
                         where a.Name == actionName
                         select a).First();

                    var count = action.SECSFunction.Length;

                    for (int i = 1; i <= count; i++)
                    {
                        var secsFunction = action.SECSFunction.First(x => x.Order == i);

                        var result = 
                            mSECSEngine.SendSECSTransaction(
                                secsFunction.Name,
                                !secsFunction.UseDefaultTransaction,
                                secsFunction.ExpectedAcknowledge,
                                secsFunction.WaitForReply,
                                out errorCode,
                                out errorText,
                                secsFunction.UseDefaultTransaction ? string.Empty : mHelper.GetSECSFunctionFilePath(secsFunction.Name));

                        if (!result)
                        {
                            mLogger.LogHelper.LogError("SECSGEM Action failed at {0}.{1}".FillArguments(action.Name, secsFunction.Name));
                            break;
                        }
                    }

                    mWaitAction.Set();
                    OnActionCompleted?.Invoke(this, new SECSGEMActionEventArgs(actionName, storeReplyRequired, storeMessageId, errorCode, errorText));
                });
        }

        public Task RunActionWithParam(string actionName, bool replyRequired, string messageId, params object[] parameters)
        {
            return
                Task.Run(() =>
                {
                    // Store value first, prevent asynchronous value happen
                    var storeReplyRequired = replyRequired;
                    var storeMessageId = messageId;
                    var errorCode = 0;
                    var errorText = "OK";

                    mWaitAction.WaitOne();

                    var action =
                        (from a in mHelper.Configuration.SECSGEMActions.Action
                         where a.Name == actionName
                         select a).First();

                    var count = action.SECSFunction.Length;

                    for (int i = 1; i <= count; i++)
                    {
                        var secsFunction = action.SECSFunction.First(x => x.Order == i);

                        var result =
                            mSECSEngine.SendSECSTransaction(
                                secsFunction.Name,
                                !secsFunction.UseDefaultTransaction,
                                secsFunction.ExpectedAcknowledge,
                                secsFunction.WaitForReply,
                                out errorCode,
                                out errorText,
                                secsFunction.UseDefaultTransaction ? string.Empty : mHelper.GetSECSFunctionFilePath(secsFunction.Name),
                                parameters);

                        if (!result)
                        {
                            mLogger.LogHelper.LogError("SECSGEM Action failed at {0}.{1}".FillArguments(action.Name, secsFunction.Name));
                            break;
                        }
                    }

                    mWaitAction.Set();
                    OnActionCompleted?.Invoke(this, new SECSGEMActionEventArgs(actionName, storeReplyRequired, storeMessageId, errorCode, errorText));
                });
        }

        #endregion
    }
}
