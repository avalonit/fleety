namespace Listener.Models
{
    public class GPSTrackerMessageData
    {
        public GPSMessageStatus MessageStatus { get; set; }
        public GPSMessageType MessageType { get; set; }
        public string[]? data { get; set; }

        public string? data0 { get; set; }

        public string? GpsData { get; set; }
        public string? EquipmentID { get; set; }
        public string? Date { get; set; }
        public string? Time { get; set; }
        public string? GpsDataEffective { get; set; }
        public string? Lat { get; set; }
        public string? LatLogo { get; set; }
        public string? Lng { get; set; }
        public string? LngLogo { get; set; }
        public decimal?  SpeedDec { get; set; }
        public string? Elevation { get; set; }
        public string? Direction { get; set; }
        public string? SatLevel { get; set; }
        public string? GsmLevel { get; set; }
        public string? BatteryLevel { get; set; }
        public string? PlanTheStep { get; set; }
        public string? RolllsOnFoot { get; set; }
        public string? TrackerState { get; set; }
        public string? GsmMCC { get; set; }
        public string? GsmMNC { get; set; }
        public required string Name { get; set; }

       
    }
}
