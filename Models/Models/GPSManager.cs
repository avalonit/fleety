namespace Listener.Models
{
    public class GPSManager
    {

        public GPSTrackerMessageData Validate(string message)
        {
            var msg = new GPSTrackerMessageData()
            {
                Name = DateTime.Now.ToString("yyyyMMddHHmmssfff")
            };

            msg.MessageStatus = GPSMessageStatus.MessageNotValid;
            msg.MessageType = GPSMessageType.Unknown;
            msg.GpsData = message;

            if (string.IsNullOrEmpty(message))
                return msg;

            if (message.StartsWith("PINGPONG"))
            {
                msg.MessageStatus = GPSMessageStatus.MessagePingPong;
                msg.MessageType = GPSMessageType.Unknown;
                return msg;
            }
            if (message.StartsWith("PING"))
            {
                msg.MessageStatus = GPSMessageStatus.MessagePing;
                msg.MessageType = GPSMessageType.Unknown;
                return msg;
            }

            if (message.StartsWith("[SG*") && message.EndsWith("]"))
            {
                msg.data = message.Split(",");
                msg.MessageStatus = GPSMessageStatus.MessageValid;
                msg.MessageType = GPSMessageType.Unknown;

                if (msg.data != null)
                {
                    if (msg.data.Length > 3)
                        msg.MessageType = GPSMessageType.GPS_Tracking;
                    else
                        msg.MessageType = GPSMessageType.GPS_Ping;

                    msg.EquipmentID = "unknown";
                    if (msg.data.Length > 0)
                    {
                        msg.data0 = msg.data[0];
                        msg.EquipmentID = GetEquipmentId(msg.data0);
                    }
                    int count = 1;
                    if (msg.data.Length > count)
                        msg.Date = msg.data[count];
                    count++;
                    if (msg.data.Length > count)
                        msg.Time = msg.data[count];
                    count++;
                    if (msg.data.Length > count)
                        msg.GpsDataEffective = msg.data[count];
                    count++;
                    if (msg.data.Length > count)
                        msg.Lat = msg.data[count];
                    count++;
                    if (msg.data.Length > count)
                        msg.LatLogo = msg.data[count];
                    count++;
                    if (msg.data.Length > count)
                        msg.Lng = msg.data[count];
                    count++;
                    if (msg.data.Length > count)
                        msg.LngLogo = msg.data[count];
                    count++;
                    if (msg.data.Length > count)
                    {
                        var speed = msg.data[count];
                        var speedDec = 0m;
                        decimal.TryParse(speed, out speedDec);
                        msg.SpeedDec = speedDec;
                    }
                    count++;
                    if (msg.data.Length > count)
                        msg.Direction = msg.data[count];
                    count++;
                    if (msg.data.Length > count)
                        msg.Elevation = msg.data[count];
                    count++;
                    if (msg.data.Length > count)
                        msg.SatLevel = msg.data[count];
                    count++;
                    if (msg.data.Length > count)
                        msg.GsmLevel = msg.data[count];
                    count++;
                    if (msg.data.Length > count)
                        msg.BatteryLevel = msg.data[count];
                    count++;
                    if (msg.data.Length > count)
                        msg.PlanTheStep = msg.data[count];
                    count++;
                    if (msg.data.Length > count)
                        msg.RolllsOnFoot = msg.data[count];
                    count++;
                    if (msg.data.Length > count)
                        msg.TrackerState = msg.data[count];
                    count++;
                    count++;
                    count++;
                    if (msg.data.Length > count)
                        msg.GsmMCC = msg.data[count];
                    count++;
                    if (msg.data.Length > count)
                        msg.GsmMNC = msg.data[count];


                }

                return msg;

            }
            return msg;
        }

        private string GetEquipmentId(string data)
        {
            var equipmentID = string.Empty;
            equipmentID = data.Replace("[SG*", string.Empty);
            var asteriskPos = equipmentID.IndexOf('*');
            if (asteriskPos > 0)
            {
                equipmentID = equipmentID.Substring(0, asteriskPos);
            }
            equipmentID = equipmentID.Replace("*", "_");
            return equipmentID;
        }

    }
}
