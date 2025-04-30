
using System.Text.Json.Serialization;


namespace AITrackerAgent.Classes;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
public class FeatureCollection
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("features")]
    public List<Feature> Features { get; set; }
}

public class Feature
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("properties")]
    public Properties Properties { get; set; }

    [JsonPropertyName("geometry")]
    public Geometry Geometry { get; set; }

    [JsonPropertyName("bbox")]
    public List<double> Bbox { get; set; }
}

public class Properties
{
    [JsonPropertyName("address")]
    public Address Address { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("confidence")]
    public string Confidence { get; set; }

    [JsonPropertyName("matchCodes")]
    public List<string> MatchCodes { get; set; }

    [JsonPropertyName("geocodePoints")]
    public List<GeocodePoint> GeocodePoints { get; set; }
}

public class Address
{
    [JsonPropertyName("countryRegion")]
    public CountryRegion CountryRegion { get; set; }

    [JsonPropertyName("adminDistricts")]
    public List<AdminDistrict> AdminDistricts { get; set; }

    [JsonPropertyName("formattedAddress")]
    public string FormattedAddress { get; set; }

    [JsonPropertyName("streetName")]
    public string StreetName { get; set; }

    [JsonPropertyName("streetNumber")]
    public string StreetNumber { get; set; }

    [JsonPropertyName("locality")]
    public string Locality { get; set; }

    [JsonPropertyName("postalCode")]
    public string PostalCode { get; set; }

    [JsonPropertyName("addressLine")]
    public string AddressLine { get; set; }
}

public class CountryRegion
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class AdminDistrict
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("shortName")]
    public string ShortName { get; set; }
}

public class GeocodePoint
{
    [JsonPropertyName("geometry")]
    public Geometry Geometry { get; set; }

    [JsonPropertyName("calculationMethod")]
    public string CalculationMethod { get; set; }

    [JsonPropertyName("usageTypes")]
    public List<string> UsageTypes { get; set; }
}

public class Geometry
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("coordinates")]
    public List<double> Coordinates { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
