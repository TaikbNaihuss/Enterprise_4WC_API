namespace Assignment4WC.Context.Repositories.ComplexQuestions
{
    public interface IComplexQuestionsRepository : ISaveChanges
    {
        Models.ComplexQuestions? GetComplexQuestion(int currentQuestionId);
    }
}