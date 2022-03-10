using Assignment4WC.Context.Models;
using Assignment4WC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace Assignment4WC.API.Extensions
{
    public static class QuestionsExtensions
    {   
        public static Dictionary<string, string> GetAnswersFromQuestionData(Questions questionData)
        {
            switch (questionData.QuestionType)
            {
                case QuestionType.MultipleChoice:
                {
                    var answerCollection = questionData.Answers
                            .OrderBy(answers => answers.Order)
                            .Select(answers => (answers.Order, answers.Answer));

                    return answerCollection.ToDictionary(
                        answer => answer.Order.ToString(),
                        answer => answer.Answer);
                }
                case QuestionType.Text or QuestionType.Picture:
                {
                    return null;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
