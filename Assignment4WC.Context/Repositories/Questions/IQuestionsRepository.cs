#nullable enable
using System.Collections.Generic;
using Assignment4WC.Models;

namespace Assignment4WC.Context.Repositories.Questions
{
    public interface IQuestionsRepository : ISaveChanges
    {
        Models.Questions? GetQuestionOrNull(int currentQuestionId);
        int CountQuestionsFromCategory(CategoryType category);
        bool DoesQuestionExist(int currentQuestionId);
        List<int> GetAllQuestionsByCategoryId(int categoryId);
    }
}