namespace Listener.Models;

public class TrackerEvent
{
    public required long id { get; set; }
    public string? title { get; set; }
    public DateTime createdat { get; set; }
    public string? user { get; set; }
    public string? state { get; set; }
    public string? equipmentid { get; set; }
    public string? gpsdate { get; set; }
    public string? gpstime { get; set; }
    public string? gpsdataeffective { get; set; }
    public double lat { get; set; }
    public string? latlogo { get; set; }
    public double lng { get; set; }
    public string? lnglogo { get; set; }
    public decimal? speeddec { get; set; }
    public string? elevation { get; set; }
    public string? direction { get; set; }
    public string? satlevel { get; set; }
    public string? gsmlevel { get; set; }
    public string? batterylevel { get; set; }
    public string? planthestep { get; set; }
    public string? rolllsonfoot { get; set; }
    public string? trackerstate { get; set; }
    public string? gsmmcc { get; set; }
    public string? gsmmnc { get; set; }
    public required string name { get; set; }
    public string? gps_data { get; set; }
    public string? address { get; set; }
    public string? address_street { get; set; }
    public string? address_province { get; set; }
    public string? address_country { get; set; }
    public string? address_postalcode { get; set; }
    public bool address_resolved { get; set; }
    public Guid vehicle_id { get; set; }



}
