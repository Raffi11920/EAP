using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.EquipmentCentralDriver
{
    using Base.BaseInterfaces;
    using Base.BridgeMessage;
    using EAPMessages.Send;
    using EAPMessages.Receive;
    using Utilities.ExtensionPlug;
    using EAP;

    public class EquipmentCentral : IEAPCentral
    {
        #region Private Field

        private string mMyName;
        private IEAPCentral mEAPCentral;
        private List<EAPDriver> mEAPDriverList;

        #endregion

        #region Custom Class

        internal class EAPDriver
        {
            public string FWEquipmentName { get; set; }
            public string EquipmentName { get; set; }
            public string DriverName { get; set; }
            public IEAPDriver Driver { get; set; }
        }

        #endregion

        #region Constructor

        public EquipmentCentral()
        {
            mEAPDriverList = new List<EAPDriver>();
        }

        #endregion

        #region IEAPCentral Method

        public int AssignParent(IBridgeMessage bridgeMessage, object parent)
        {
            try
            {
                mEAPCentral = parent as IEAPCentral;

                return 0;
            }
            catch (Exception ex)
            {
                return -9999;
            }
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

			mMyName = receive.DriverName;
            var eapConfigFolder = receive.EAPConfigFolder;
            var configFile = Helper.GetConfigurationFile(eapConfigFolder);

            if (!Helper.LoadConfiguration(configFile))
            {
                return EAPError.UNHANDLED_ERR;
            }

            try
            {
			
                Logger.Initialize(Helper.Configuration.Logger);
                Logger.LogHelper.LogInfo("Initializing Equipment Central...");

                var equipmentCount = Helper.Configuration.Equipments.EquipmentCount.ToInt();

                if (equipmentCount != Helper.Configuration.Equipments.EquipmentList.Equipment.Length)
                {
                    Logger.LogHelper.LogError("Error in EquipmentCentral configuration, <EquipmentCount> is not equal to number of <Equipment> in <EquipmentList>");
                    return -1;
                }
                
                foreach (var equipment in Helper.Configuration.Equipments.EquipmentList.Equipment)
                {
                    var initializeMessage = new InitializationMessage(eapConfigFolder, Helper.Configuration.FWEquipmentName, equipment);
                    initializeMessage.Subject = "Initialization";

                    var equipmentDriver = LoadDLL(Helper.Configuration.Equipments.EquipmentDriver.DLL) as IEAPDriver;
                    equipmentDriver.AssignParent(null, this);
                    equipmentDriver.Initialize(initializeMessage);
                    mEAPDriverList.Add(new EAPDriver()
                    {
                        FWEquipmentName = Helper.Configuration.FWEquipmentName,
                        EquipmentName = equipment,
                        Driver = equipmentDriver,
                        DriverName = Helper.Configuration.Equipments.EquipmentDriver.Name
                    });
                }

                Logger.LogHelper.LogInfo("Equipment Central initialization complete.");
                return 0;
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
                return EAPError.UNKNOWN_ERR;
            }
        }

        public int ReceiveMessage(IBridgeMessage bridgeMessage)
        {
            try
            {
                Logger.LogHelper.LogInfo("Received Sender:{0}, PassBySender:{1}, Destination:{2}, Subject:{3}".FillArguments(bridgeMessage.Sender, bridgeMessage.PassBySender, bridgeMessage.Destination, bridgeMessage.Subject));
                var parentDestination = bridgeMessage.Destination;
                var childDestination = string.Empty;
                //bridgeMessage.PassBySender = mMyName;

                if (bridgeMessage.Destination.Contains("."))
                {
                    parentDestination = bridgeMessage.Destination.Split('.')[0];
                    childDestination = string.Join(".", bridgeMessage.Destination.Split('.').Skip(1));
                    bridgeMessage.Destination = childDestination;
                }

                if (bridgeMessage.Destination == "EquipmentConnectionDriver")
                {
                    var fwEquipmentName = bridgeMessage.GetBasicData("FWEQUIPMENTID").Value.ToString();
                    var equipmentName = bridgeMessage.GetBasicData("EQUIPMENTID").Value.ToString();

                    var targetEqDriver = mEAPDriverList.FirstOrDefault(x => x.FWEquipmentName == fwEquipmentName && x.EquipmentName == equipmentName);

                    if (targetEqDriver == null)
                    {
                        Logger.LogHelper.LogError("Target equipment {0}.{1} does not exist or configured.".FillArguments(fwEquipmentName, equipmentName));
                        return EAPError.DRIVER_NOT_FOUND;
                    }

                    bridgeMessage.PassBySender = mMyName;
                    targetEqDriver.Driver.ReceiveMessage(bridgeMessage);
                }
                else
                {
                    var fwEquipmentName = bridgeMessage.GetBasicData("FWEQUIPMENTID").Value.ToString();
                    var equipmentName = bridgeMessage.GetBasicData("EQUIPMENTID").Value.ToString();
                    var targetEqDriver = mEAPDriverList.FirstOrDefault(x => x.FWEquipmentName == fwEquipmentName && x.EquipmentName == equipmentName);

                    if (targetEqDriver != null && (targetEqDriver.DriverName != bridgeMessage.Sender && targetEqDriver.DriverName != bridgeMessage.PassBySender))
                    {
                        bridgeMessage.PassBySender = mMyName;
                        targetEqDriver.Driver.ReceiveMessage(bridgeMessage);
                    }
                    //bridgeMessage.Sender = mMyName;
                    if (bridgeMessage.Sender != "EAPCentral" && bridgeMessage.PassBySender != "EAPCentral")
                    {
                        bridgeMessage.PassBySender = mMyName;
                        mEAPCentral.ReceiveMessage(bridgeMessage);
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

                Logger.LogHelper.LogDebug("Loading dll = {0}".FillArguments(dllName));

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
