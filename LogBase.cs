using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;
using log4net.Repository;
using log4net.Appender;
using log4net.Layout;
using log4net.Core;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace Qynix.EAP.Utilities.LogUtilities
{
    public class LogBase
    {
        #region Public Field

        public const string DefaultLogInstanceName = "TraceLog";

        #endregion

        #region Private Field

        private ILog _logger = null;
        private static LogBase _instance;

        #endregion

        #region Public Properties

        protected ILog Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        protected static LogBase Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LogBase(DefaultLogInstanceName);

                return _instance;
            }
        }

        #endregion

        #region Constructor

        public LogBase()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public LogBase(string instanceName)
        {
            _logger = LogManager.GetLogger(instanceName);
        }

        #endregion

        #region Protected Method

        protected static LogBase GetInstance(string name)
        {
            return new LogBase(name);
        }

        protected void Info(object obj)
        {
            Info(obj, null);
        }

        protected void Info(object obj, Exception ex)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info(obj, ex);
            }
        }

        protected void Debug(object obj)
        {
            Debug(obj, null);
        }

        protected void Debug(object obj, Exception ex)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(obj, ex);
            }
        }

        protected void Warn(object obj)
        {
            Warn(obj, null);
        }

        protected void Warn(object obj, Exception ex)
        {
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn(obj, ex);
            }
        }

        protected void Error(object obj)
        {
            Error(obj, null);
        }

        protected void Error(object obj, Exception ex)
        {
            if (Logger.IsErrorEnabled)
            {
                Logger.Error(obj, ex);
            }
        }

        protected void Fatal(object obj)
        {
            Fatal(obj, null);
        }

        protected void Fatal(object obj, Exception ex)
        {
            if (Logger.IsFatalEnabled)
            {
                Logger.Fatal(obj, ex);
            }
        }

        protected IAppender CreateFileAppender(string name, string fileName)
        {
            var appender = new RollingFileAppender();
            appender.Name = name;
            appender.File = fileName;
            appender.AppendToFile = true;
            appender.RollingStyle = RollingFileAppender.RollingMode.Size;
            appender.MaxFileSize = 5242880;
            appender.MaxSizeRollBackups = 10;
            appender.StaticLogFileName = true;

            var layout = new PatternLayout();
            layout.ConversionPattern = "%-5p %d: %m%n";
            layout.Header = "[Begin Log]\r\n";
            layout.Footer = "[End Log]\r\n";
            layout.ActivateOptions();

            appender.Layout = layout;
            appender.ActivateOptions();

            return appender;
        }


        #endregion
    }
}
