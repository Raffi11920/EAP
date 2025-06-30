using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Runtime.CompilerServices;

using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;

namespace Qynix.EAP.Utilities.LogUtilities
{
    using FileUtilities;
    using log4net.Core;
    public sealed class LogHelper : LogBase
    {
        #region Private Field

        private ILog mLogger = null;
        private string mDefaultLoggerConfigFile = 
            Path.Combine(
                System.Environment.CurrentDirectory, 
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
                    .FilePath);

        #endregion

        #region Properties

        public string LoggerName
        {
            get { return mLogger.Logger.Name; }
        }

        #endregion

        #region Constructor

        public LogHelper() : base()
        {

        }

        #endregion

        #region Public Method

        public void InitializeLogger()
        {
            LoadLoggerConfigFile(mDefaultLoggerConfigFile);
        }

        public void InitializeLogger(string loggerName)
        {
            //LoadLoggerConfigFile(mDefaultLoggerConfigFile);
            var allLoggers = LogManager.GetCurrentLoggers();
            mLogger = LogManager.GetLogger(loggerName);
        }

        public void InitializeLogger(string appenderName, string loggerName, string fileName)
        {
            var appender = CreateFileAppender(appenderName, fileName);
            var repository = LogManager.GetRepository();
            var hierarchy = (Hierarchy)repository;
            var logger = hierarchy.LoggerFactory.CreateLogger(repository, loggerName);
            logger.Hierarchy = hierarchy;

            logger.AddAppender(appender);
            logger.Repository.Configured = true;
            logger.Level = Level.All;
            
            hierarchy.Threshold = Level.All;

            mLogger = new LogImpl(logger);
        }

        public void InitializeLogger(string loggerConfigFile, string loggerName)
        {
            LoadLoggerConfigFile(loggerConfigFile);
            mLogger = LogManager.GetLogger(loggerName);          
        }

        public void LogException(Exception ex, [CallerMemberName]string methodName = null, [CallerFilePath]string errSourceFile = null)
        {
            if (mLogger == null)
                throw new ArgumentNullException("mLogger", "The variable <mLogger> is null.");

            string className = FileHelper.GetFileNameFromFilePath(errSourceFile);
            string exceptionMsg = FormatStandardLogMessage(className, methodName, ex.ToString());

#if DEBUG
            WriteConsoleLogType("EXCEPTION", ConsoleColor.DarkRed);
            Console.WriteLine("{0}", exceptionMsg);
#endif
            mLogger.Fatal(exceptionMsg);
        }

        public void LogError(string errMsg, [CallerMemberName]string methodName = null, [CallerFilePath]string callerFilePath = null)
        {
            if (mLogger == null)
                throw new ArgumentNullException("mLogger", "The variable <mLogger> is null.");

            string className = FileHelper.GetFileNameFromFilePath(callerFilePath);
            string message = FormatStandardLogMessage(className, methodName, errMsg);


#if DEBUG
            WriteConsoleLogType("ERROR", ConsoleColor.Red);
            Console.WriteLine("{0}", message);
#endif
            mLogger.Error(message);
        }

        public void LogInfo(string infoMsg, [CallerMemberName]string methodName = null, [CallerFilePath]string callerFilePath = null)
        {
            if (mLogger == null)
                throw new ArgumentNullException("mLogger", "The variable <mLogger> is null.");

            string className = FileHelper.GetFileNameFromFilePath(callerFilePath);
            string message = FormatStandardLogMessage(className, methodName, infoMsg);

#if DEBUG
            WriteConsoleLogType("INFO", ConsoleColor.Green);
            Console.WriteLine("{0}", message);
#endif

            mLogger.Info(message);
        }

        public void LogDebug(string debugMsg, [CallerMemberName]string methodName = null, [CallerFilePath]string callerFilePath = null)
        {
            if (mLogger == null)
                throw new ArgumentNullException("mLogger", "The variable <mLogger> is null.");

            string className = FileHelper.GetFileNameFromFilePath(callerFilePath);
            string message = FormatStandardLogMessage(className, methodName, debugMsg);

#if DEBUG
            WriteConsoleLogType("DEBUG", ConsoleColor.Cyan);
            Console.WriteLine("{0}", message);
#endif
            mLogger.Debug(message);
        }

        #endregion

        #region Private Method

        private void LoadLoggerConfigFile(string configFile)
        {
            var file = new FileInfo(configFile);

            if (!file.Exists)
            {
                throw new FileLoadException(string.Format("The configuration file {0} cannot be found."), configFile);
            }

            log4net.Config.XmlConfigurator.Configure(file);
        }

        private string FormatStandardLogMessage(string className, string methodName, string message)
        {
            return string.Format("{0} - {1}: {2}", className, methodName, message);
        }

        private void WriteConsoleLogType(string logType, ConsoleColor consoleColor)
        {
            Console.Write("[");
            Console.ForegroundColor = consoleColor;
            Console.Write(logType);
            Console.ResetColor();
            Console.Write("]");
        }

        public void LogError(object p)
        {
            throw new NotImplementedException();
        }

        public void LogInfo(object p)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
