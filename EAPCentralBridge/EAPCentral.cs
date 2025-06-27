using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.EAPCentral
{
    using Base.BridgeMessage;
    using Base.BaseInterfaces;
    using EAPMessages.Send;
    using Utilities.ExtensionPlug;

    public class EAPCentral : IEAPCentral
    {
        #region Private Field

        private string mEAPConfigFolder;

        private List<EAPDriver> mEAPCentralList;
        private List<EAPDriver> mEAPDriverList;

        #endregion

        private class EAPDriver
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public IEAPDriver Driver { get; set; }
        }

        public EAPCentral()
        {
        }

        #region IEAPCentral Method

        public int AssignParent(IBridgeMessage bridgeMessage, object parent)
        {
            throw new NotImplementedException();
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

            mEAPCentralList = new List<EAPDriver>();
            mEAPDriverList = new List<EAPDriver>();

            mEAPConfigFolder = bridgeMessage.GetBasicData("EAPConfigFolder").Value.ToString();
            var configFile = Helper.GetConfigurationFile(mEAPConfigFolder);

            if (!Helper.LoadConfiguration(configFile))
            {
                Logger.LogHelper.LogError("Failed to load EAPCentral configuration file, Path:{0}".FillArguments(configFile));
                return -1;
            }

            Logger.Initialize(Helper.EAPCentralConfigurations.Logger);
            Logger.LogHelper.LogInfo("Initializing EAP Central...");


            foreach (var driver in Helper.EAPCentralConfigurations.EAPDriverList.EAPDriver)
            {
                Logger.LogHelper.LogInfo("Loading EAP Central Bridge -> {0}".FillArguments(driver.DLL));

                var eapDriver = LoadDLL(driver.DLL) as IEAPDriver;
                var initializationMessage = new InitializationMessage(mEAPConfigFolder, driver.Name);

                if (eapDriver.GetType().GetInterfaces().Contains(typeof(IEAPCentral)))
                {
                    mEAPCentralList.Add(
                        new EAPDriver()
                        {
                            Name = driver.Name,
                            Description = driver.Description,
                            Driver = eapDriver as IEAPCentral
                        });
                }
                else
                {
                    mEAPDriverList.Add(
                        new EAPDriver()
                        {
                            Name = driver.Name,
                            Description = driver.Description,
                            Driver = eapDriver as IEAPDriver
                        });
                }

                eapDriver.AssignParent(null, this);
                eapDriver.Initialize(initializationMessage);
            }

            Logger.LogHelper.LogInfo("EAP Central initialization complete.");
            return 0;
        }

        public int ReceiveMessage(IBridgeMessage bridgeMessage)
        {
            try
            {
                Logger.LogHelper.LogInfo("Received Sender:{0}, Destination:{1}, Subject:{2}".FillArguments(bridgeMessage.Sender, bridgeMessage.Destination, bridgeMessage.Subject));
                IEnumerable<EAPDriver> targetDrivers;

                if (bridgeMessage.Destination == "All")
                {
                    targetDrivers = mEAPDriverList.Union(mEAPCentralList).ToArray();
                    //targetDrivers = from a in targetDrivers
                    //                where a.Name != bridgeMessage.PassBySender && a.Name != bridgeMessage.Sender
                    //                select a;
                }
                else
                {
                    var parentDestination = bridgeMessage.Destination;
                    var childDestination = string.Empty;

                    if (bridgeMessage.Destination.Contains("."))
                    {
                        parentDestination = bridgeMessage.Destination.Split('.')[0];
                        childDestination = string.Join(".", bridgeMessage.Destination.Split('.').Skip(1));
                        bridgeMessage.Destination = childDestination;
                    }

                    targetDrivers = from a in (mEAPDriverList.Union(mEAPCentralList))
                                    where a.Name == parentDestination
                                    select a;

                    if (targetDrivers == null || targetDrivers.Count() <= 0)
                    {
                        targetDrivers = mEAPDriverList.Union(mEAPCentralList);
                        targetDrivers = from a in targetDrivers
                                        where a.Name != bridgeMessage.PassBySender && a.Name != bridgeMessage.Sender
                                        select a;
                    }
                }

                var passBySender = bridgeMessage.PassBySender;
                foreach (var driver in targetDrivers)
                {
                    if (driver.Name != passBySender && driver.Name != passBySender)
                    {
                        bridgeMessage.PassBySender = "EAPCentral";
                        driver.Driver.ReceiveMessage(bridgeMessage);

                    }
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

        #region Private Method

        private object LoadDLL(string dllName)
        {
            try
            {
                var dir = Path.GetDirectoryName(dllName);

                if (string.IsNullOrEmpty(dir))
                {
                    dllName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dllName);
                }

                var assembly = Assembly.LoadFile(dllName);

                var type = (from a in assembly.GetTypes()
                           where a.GetInterfaces().Contains(typeof(IEAPDriver))
                           select a).First();

                return Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
                return null;
            }
        }

        #endregion
    }
}
