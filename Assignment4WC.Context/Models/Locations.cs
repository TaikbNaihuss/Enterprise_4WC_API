using System;
using System.ComponentModel.DataAnnotations.Schema;
using Geolocation;

namespace Assignment4WC.Context.Models
{
    public partial class Locations
    {
        public int LocationId { get; set; }

        [Column(TypeName = "decimal(8,6)")]
        public decimal Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude { get; set; }

        public virtual ComplexQuestions Question { get; set; }
    }

    public partial class Locations
    {
        public int GetDistanceInMeters(Locations location)
        {
            var location1 = new Coordinate(decimal.ToDouble(Latitude), decimal.ToDouble(Longitude));
            var location2 = new Coordinate(decimal.ToDouble(location.Latitude), decimal.ToDouble(location.Longitude));

            return Convert.ToInt32(
                Math.Round(
                    ConvertMilesToMeters(
                        GeoCalculator.GetDistance(location1, location2))));
        }

        private static double ConvertMilesToMeters(double miles) =>
            miles * 1609.344;
    }
}