using Assignment4WC.Models;

namespace Assignment4WC.Context.Repositories.Categories
{
    public interface ICategoriesRepository
    {
        int? GetCategoryIdFromCategory(CategoryType category);
    }
}