namespace Assignment4WC.Context.Models
{
    public class Answers
    {
        public int AnswerId { get; set; }
        public int QuestionId { get; set; }
        public char Order { get; set; }
        public string Answer { get; set; }

        public virtual Questions Question { get; set; }
    }
}