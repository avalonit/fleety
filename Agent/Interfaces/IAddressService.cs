using AITrackerAgent.Classes;

namespace AITrackerAgent.Interfaces;

public interface IAddressService
{
    Task<string> GetFormattedAddressFromCordinates(double lat, double lng);
    Task<Address?> GetAddressFromCordinates(double lat, double lng);
}

