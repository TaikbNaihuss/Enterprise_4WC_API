#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Assignment4WC.Models;
using Microsoft.EntityFrameworkCore;

namespace Assignment4WC.Context.Repositories.Questions
{
    public class QuestionsRepository : IQuestionsRepository
    {
        private readonly AssignmentContext _context;

        public QuestionsRepository(AssignmentContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Models.Questions? GetQuestionOrNull(int currentQuestionId) =>
            _context.Questions
                .Include(questions => questions.Category)
                .Include(questions => questions.Answers)
                .AsSplitQuery()
                .FirstOrDefault(questions => questions.QuestionId == currentQuestionId);

        public int CountQuestionsFromCategory(CategoryType category) =>
            _context.Questions.Include(questions => questions.Category)
                .AsSplitQuery()
                .Count(questions => questions.Category.CategoryName == category);

        public bool DoesQuestionExist(int currentQuestionId) =>
            _context.Questions.Any(questions => questions.QuestionId == currentQuestionId);

        public List<int> GetAllQuestionsByCategoryId(int categoryId) =>
            _context.Questions
                .Where(questions => questions.CategoryId == categoryId)
                .Select(questions => questions.QuestionId)
                .ToList();

        public void SaveChanges() => 
            _context.SaveChanges();
    }
}