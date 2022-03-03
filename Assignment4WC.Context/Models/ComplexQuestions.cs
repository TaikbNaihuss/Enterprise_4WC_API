using System.Collections.Generic;

namespace Assignment4WC.Context.Models
{
    public class ComplexQuestions : Questions
    {
        public string Hint { get; set; }
        public string LocationHint { get; set; }
        public int LocationId { get; set; }

        public virtual Locations Location { get; set; }
    }
}