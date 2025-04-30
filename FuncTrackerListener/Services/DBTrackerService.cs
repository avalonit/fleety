using AITrackerAgent.Interfaces;
using Listener.Models;
using Microsoft.EntityFrameworkCore;
using trackerlistener.DataAccessLayer;

namespace trackerlistener.Services;

public class DBTrackerService(ApplicationDbContext dbContext, IAddressService addressService)
{
    public async Task<IEnumerable<Driver>> GetDriversAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Drivers
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
    public async Task<IEnumerable<TrackerEvent>> GetTrackerEventsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.TrackerEvents
            .OrderBy(d => d.id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TrackerEvent>> GetTrackerEventsAsyncByAddressEmpty(CancellationToken cancellationToken = default)
    {
        return await dbContext.TrackerEvents
            .Where(d => !d.address_resolved && d.lng > 0 && d.lat > 0)
            .OrderBy(d => d.id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public TrackerEvent? GetTrackerEventsById(long id, CancellationToken cancellationToken = default)
    {
        return dbContext.TrackerEvents.SingleOrDefault(d => d.id == id);
    }
    public bool Update(TrackerEvent trackerEvent, CancellationToken cancellationToken = default)
    {
        var isEventAdded = false;
        dbContext.Database.BeginTransaction();
        var vehicle = dbContext.Vehicles.SingleOrDefault(a => a.gps_equipmentid == trackerEvent.equipmentid);
        if (vehicle != null
            && vehicle.gps_lat != null
            && vehicle.gps_lng != null)
        {
            vehicle.gps_lat = trackerEvent.lat;
            vehicle.gps_lng = trackerEvent.lng;
            vehicle.gps_speeddec = trackerEvent.speeddec;
            vehicle.gps_batterylevel = trackerEvent.batterylevel;
            vehicle.lastupdatedat = DateTime.Now;
            dbContext.Entry(vehicle).State = EntityState.Modified;
            dbContext.Update(vehicle);
            trackerEvent.vehicle_id = vehicle.vehicle_id;
        }

        var trackerEventEnt = GetTrackerEventsById(trackerEvent.id, cancellationToken);
        if (trackerEventEnt == null)
        {
            dbContext.TrackerEvents.Add(trackerEvent);
            isEventAdded = true;
        }

        dbContext.SaveChanges();
        dbContext.Database.CommitTransaction();
        return isEventAdded;
    }
    public async Task BatchUpdateAddress(CancellationToken cancellationToken = default)
    {
        dbContext.Database.BeginTransaction();
        var trackerEvents = await GetTrackerEventsAsyncByAddressEmpty();
        foreach (var trackerEvent in trackerEvents)
        {
            var address = await addressService.GetAddressFromCordinates(trackerEvent.lat, trackerEvent.lng);
            if (address != null)
            {
                if (!string.IsNullOrEmpty(address.FormattedAddress))
                    trackerEvent.address = address.FormattedAddress;
                if (address.CountryRegion != null && !string.IsNullOrEmpty(address.CountryRegion.Name))
                    trackerEvent.address_country = address.CountryRegion.Name;
                if (!string.IsNullOrEmpty(address.Locality))
                    trackerEvent.address_province = address.Locality;
                if (!string.IsNullOrEmpty(address.PostalCode))
                    trackerEvent.address_postalcode = address.PostalCode;
                if (!string.IsNullOrEmpty(address.AddressLine))
                    trackerEvent.address_street = address.AddressLine;
            }
            trackerEvent.address_resolved = true;
            dbContext.Entry(trackerEvent).State = EntityState.Modified;
            dbContext.Update(trackerEvent);
        }
        dbContext.SaveChanges();
        dbContext.Database.CommitTransaction();
    }

}
