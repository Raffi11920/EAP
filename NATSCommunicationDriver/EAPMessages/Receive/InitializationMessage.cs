using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.NATSCommunicationDriver.EAPMessages.Receive
{
    using Base.BridgeMessage;

    public class InitializationMessage : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentId;
        private string mEquipmentId;
        private string mEquipmentModel;
        private string mSoftwareRevision;
        private string mIPAddress;
        private string mPort;
        private string mDeviceID;
        private string mConnectionType;
        private string mCOMPort;
        private string mBaudRate;

        #endregion

        #region Properties

        public string FWEquipmentID
        {
            get { return mFWEquipmentId; }
        }

        public string EquipmentID
        {
            get { return mEquipmentId; }
        }

        public string EquipmentModel
        {
            get { return mEquipmentModel; }
        }

        public string SoftwareRevision
        {
            get { return mSoftwareRevision; }
        }

        public string IPAddress
        {
            get { return mIPAddress; }
        }

        public string Port
        {
            get { return mPort; }
        }

        public string DeviceID
        {
            get { return mDeviceID; }
        }

        public string ConnectionType
        {
            get { return mConnectionType; }
        }

        public string COMPort
        {
            get { return mCOMPort; }
        }

        public string BaudRate
        {
            get { return mBaudRate; }
        }
        #endregion

        protected override void AssignData()
        {
            mFWEquipmentId = GetBasicData("FWEQUIPMENTID").Value.ToString();
            mEquipmentId = GetBasicData("EQUIPMENTID").Value.ToString();
            mEquipmentModel = GetBasicData("EQUIPMENTMODEL").Value.ToString();
            mSoftwareRevision = GetBasicData("SOFTWAREREVISION").Value.ToString();
            mIPAddress = GetBasicData("IPADDRESS").Value.ToString();
            mPort = GetBasicData("PORT").Value.ToString();
            mDeviceID = GetBasicData("DEVICEID").Value.ToString();
            mConnectionType = GetBasicData("CONNECTIONTYPE").Value.ToString();
            mCOMPort = GetBasicData("COMPORT").Value.ToString();
            mBaudRate = GetBasicData("BAUDRATE").Value.ToString();
        }
    }
}
