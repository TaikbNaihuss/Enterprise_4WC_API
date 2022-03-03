using Assignment4WC.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Assignment4WC.Models;
using Assignment4WC.Models.ResultType;

namespace Assignment4WC.Logic
{
    public class QuestionRandomiser : IQuestionRandomiser
    {
        private readonly AssignmentContext _context;
        private readonly Random _random;

        public QuestionRandomiser(AssignmentContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _random = new Random(Guid.NewGuid().GetHashCode());
        }

        public Result<string> GetQuestionsWithOrder(int numOfQuestions, CategoryType category)
        {
            var categoryId = GetCategoryIdFromName(category);

            if (!categoryId.HasValue)
                return new Result<string>(new ErrorMessage(HttpStatusCode.NotFound,
                    $"Category '{category}' does not exist in the database."));

            var questionIds = _context.Questions
                .Where(questions => questions.CategoryId == categoryId.Value)
                .Select(questions => questions.QuestionId)
                .ToList();

            if (questionIds.Count < numOfQuestions)
                return new Result<string>(new ErrorMessage(HttpStatusCode.BadRequest,
                    $"{numOfQuestions} questions from category '{category}' were requested but this category only contains {questionIds.Count} question(s)."));

            var orderedQuestionIds = "";
            var exclusionList = new List<int>();
            for (var i = 0; i < numOfQuestions; i++)
            {
                int currentQuestionId;
                do
                {
                    currentQuestionId = questionIds[_random.Next(questionIds.Count)];
                } while (exclusionList.Contains(currentQuestionId));
                
                exclusionList.Add(currentQuestionId);

                orderedQuestionIds += $"{currentQuestionId},";
            }

            return new Result<string>(orderedQuestionIds.TrimEnd(','));
        }

        private int? GetCategoryIdFromName(CategoryType category) =>
            _context.Categories?.FirstOrDefault(categories =>
                    categories.CategoryName == category)
                    ?.CategoryId;

    }
}
