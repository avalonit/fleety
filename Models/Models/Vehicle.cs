using System;

namespace Listener.Models;

public class Vehicle
{
    public Guid vehicle_id { get; set; }
    public Guid? driver_id { get; set; }
    public string? vehicle_name { get; set; }
    public string? vehicle_numberplate { get; set; }
    public string? gps_phone { get; set; }
    public string? gps_equipmentid { get; set; }
    public string? gps_batterylevel { get; set; }
    public string? gps_gsmlevel { get; set; }
    public string? gps_satlevel { get; set; }
    public string? gps_direction { get; set; }
    public decimal? gps_speeddec { get; set; }
    public string? gps_lnglogo { get; set; }
    public double? gps_lng { get; set; }
    public double? gps_lat { get; set; }
    public string? gps_gpstime { get; set; }
    public string? gps_gpsdate { get; set; }
    public DateTime createdat { get; set; }
    public DateTime? lastupdatedat { get; set; }
    

}