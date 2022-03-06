using System.Collections.Generic;
using Assignment4WC.Models;

namespace Assignment4WC.Context.Models
{
    public class Questions
    {
        public int QuestionId { get; set; }
        public QuestionType QuestionType { get; set; }
        public string Question { get; set; }
        public int CategoryId { get; set; }
        public string Discriminator { get; set; }
        public string CorrectAnswer { get; set; }

        public List<Answers> Answers { get; set; }
        public Categories Category { get; set; }
    }
}