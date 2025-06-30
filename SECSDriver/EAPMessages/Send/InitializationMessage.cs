using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SECSDriver.EAPMessages.Send
{
    using Base.BridgeMessage;

    internal class InitializationMessage : BridgeMessagePacket
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



        #endregion

        public InitializationMessage
            (
                string fwEquipmentId,
                string equipmentId,
                string equipmentModel,
                string softwareRevision,
                string ipAddress,
                string port,
                string deviceId,
                string connectionType,
                string ComPort,
                string BaudRate
            )
        {
            mFWEquipmentId = fwEquipmentId;
            mEquipmentId = equipmentId;
            mEquipmentModel = equipmentModel;
            mSoftwareRevision = softwareRevision;
            mIPAddress = ipAddress;
            mPort = port;
            mDeviceID = deviceId;
            mConnectionType = connectionType;
            mCOMPort = ComPort;
            mBaudRate = BaudRate;

            CompileData();
        }

        public override void CompileData()
        {
            AddBasicData("FWEQUIPMENTID", mFWEquipmentId, mFWEquipmentId.GetType());
            AddBasicData("EQUIPMENTID", mEquipmentId, mEquipmentId.GetType());
            AddBasicData("EQUIPMENTMODEL", mEquipmentModel, mEquipmentModel.GetType());
            AddBasicData("SOFTWAREREVISION", mSoftwareRevision, mSoftwareRevision.GetType());
            AddBasicData("IPADDRESS", mIPAddress, mIPAddress.GetType());
            AddBasicData("PORT", mPort, mPort.GetType());
            AddBasicData("DEVICEID", mDeviceID, mDeviceID.GetType());
            AddBasicData("CONNECTIONTYPE", mConnectionType, mConnectionType.GetType());
            AddBasicData("COMPORT", mCOMPort, mCOMPort.GetType());
            AddBasicData("BAUDRATE", mBaudRate, mBaudRate.GetType());
        }
    }
}
