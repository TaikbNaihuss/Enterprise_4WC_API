using System;
using Assignment4WC.Context.Repositories.Categories;
using Assignment4WC.Context.Repositories.ComplexQuestions;
using Assignment4WC.Context.Repositories.Locations;
using Assignment4WC.Context.Repositories.Member;
using Assignment4WC.Context.Repositories.Questions;

namespace Assignment4WC.Context.Repositories
{
    public class GlobalRepository : IGlobalRepository
    {
        public IQuestionsRepository Questions { get; }
        public IComplexQuestionsRepository ComplexQuestions { get; }
        public ILocationsRepository Locations { get; }
        public IMembersRepository Members { get; }
        public ICategoriesRepository Categories { get; }

        public GlobalRepository(IQuestionsRepository questions, IComplexQuestionsRepository complexQuestions,
             ILocationsRepository locations, IMembersRepository members, ICategoriesRepository categories)
        {
            Questions = questions ?? throw new ArgumentNullException(nameof(questions));
            ComplexQuestions = complexQuestions ?? throw new ArgumentNullException(nameof(complexQuestions));
            Locations = locations ?? throw new ArgumentNullException(nameof(locations));
            Members = members ?? throw new ArgumentNullException(nameof(members));
            Categories = categories ?? throw new ArgumentNullException(nameof(categories));
        }

        public void SaveChanges()
        {
            Questions.SaveChanges();
            ComplexQuestions.SaveChanges();
            Locations.SaveChanges();
            Members.SaveChanges();
        }
    }
}