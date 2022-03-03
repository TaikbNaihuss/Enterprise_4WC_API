using System.Collections.Generic;
using Assignment4WC.Models;

namespace Assignment4WC.Context.Models
{
    public class Categories
    {
        public int CategoryId { get; set; }
        public CategoryType CategoryName { get; set; }

        public virtual List<Questions> Questions { get; set; }
    }
}
