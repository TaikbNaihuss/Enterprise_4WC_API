using System;
using System.Linq;
using Assignment4WC.Models;

namespace Assignment4WC.Context.Repositories.Categories
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly AssignmentContext _context;

        public CategoriesRepository(AssignmentContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public int? GetCategoryIdFromCategory(CategoryType category) =>
            _context.Categories?.FirstOrDefault(categories =>
                    categories.CategoryName == category)
                ?.CategoryId;
    }
}
