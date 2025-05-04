
namespace AITrackerAgent.Classes
{
    public class LocationHistory
    {
        public required Guid vehicle_id { get; set; }
        public string? address { get; set; }
        public string? address_street { get; set; }
        public string? address_province { get; set; }
        public string? address_country { get; set; }
        public string? address_postalcode { get; set; }
        public decimal speeddec { get; set; }
        public DateTime createdat { get; set; }

    }

}
