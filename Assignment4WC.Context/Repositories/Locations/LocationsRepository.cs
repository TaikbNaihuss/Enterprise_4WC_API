#nullable enable
using System;
using System.Linq;

namespace Assignment4WC.Context.Repositories.Locations
{
    public class LocationsRepository : ILocationsRepository
    {
        private readonly AssignmentContext _context;

        public LocationsRepository(AssignmentContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Add(Models.Locations newLocation) =>
            _context.Locations.Add(newLocation);

        public Models.Locations GetLocationByLocation(Models.Locations newLocation) =>
            _context.Locations.First(locations => locations == newLocation);

        public Models.Locations GetLocationByLocationId(int locationId) =>
            _context.Locations.First(locations => locations.LocationId == locationId);

        public Models.Locations? GetLocationByLocationIdOrNull(int locationId) =>
            _context.Locations.FirstOrDefault(locations => locations.LocationId == locationId);

        public void SaveChanges() => 
            _context.SaveChanges();
    }
}