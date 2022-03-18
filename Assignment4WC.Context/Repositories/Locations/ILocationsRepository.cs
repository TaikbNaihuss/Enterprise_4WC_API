namespace Assignment4WC.Context.Repositories.Locations
{
    public interface ILocationsRepository : ISaveChanges
    {
        void Add(Models.Locations newLocation);
        Models.Locations GetLocationByLocation(Models.Locations newLocation);
        Models.Locations GetLocationByLocationId(int locationId);
        Models.Locations? GetLocationByLocationIdOrNull(int locationId);
    }
}