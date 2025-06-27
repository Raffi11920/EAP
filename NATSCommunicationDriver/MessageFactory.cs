using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.NATSCommunicationDriver
{
    using Generic;
    using RMS.Common;

    internal class MessageFactory
    {
         public static InitializationMessage GetHSMSIniatializationMessage(
             string equipmentModel,
             string softwareRevision,
             string ipAddress,
             string port,
             string deviceId)
        {
            var msg = new InitializationMessage();
            msg.Command = EnumType.Command.Initialization;
            msg.Subject = "EAPEvents";
            msg.Vids.Add("EquipmentModel", equipmentModel);
            msg.Vids.Add("SoftwareRevision", softwareRevision);
            msg.Vids.Add("IPAddress", ipAddress);
            msg.Vids.Add("Port", port);
            msg.Vids.Add("DeviceID", deviceId);
            msg.Vids.Add("ConnectionType", "HSMS");

            return msg;
        }
    }
}
