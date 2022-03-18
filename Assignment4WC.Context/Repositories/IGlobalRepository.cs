using Assignment4WC.Context.Repositories.Categories;
using Assignment4WC.Context.Repositories.ComplexQuestions;
using Assignment4WC.Context.Repositories.Locations;
using Assignment4WC.Context.Repositories.Member;
using Assignment4WC.Context.Repositories.Questions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Assignment4WC.Context.Repositories
{
    public interface IGlobalRepository : ISaveChanges
    {
        IQuestionsRepository Questions { get; }
        IComplexQuestionsRepository ComplexQuestions { get; }
        ILocationsRepository Locations { get; }
        IMembersRepository Members { get; }
        ICategoriesRepository Categories { get; }
    }
}