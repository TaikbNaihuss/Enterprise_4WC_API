#nullable enable
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Assignment4WC.Context.Repositories.ComplexQuestions
{
    public class ComplexQuestionsRepository : IComplexQuestionsRepository
    {
        private readonly AssignmentContext _context;

        public ComplexQuestionsRepository(AssignmentContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Models.ComplexQuestions? GetComplexQuestion(int currentQuestionId) =>
            _context.ComplexQuestions
                .Include(questions => questions.Category)
                .Include(questions => questions.Answers)
                .Include(questions => questions.Location)
                .AsSplitQuery()
                .FirstOrDefault(questions => questions.QuestionId == currentQuestionId);

        public void SaveChanges() =>
            _context.SaveChanges();
    }
}