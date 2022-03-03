using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment4WC.Context.Models
{
    public class Locations
    {
        public int LocationId { get; set; }

        [Column(TypeName = "decimal(8,6)")]
        public decimal Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude { get; set; }

        public virtual ComplexQuestions Question { get; set; }
    }
}