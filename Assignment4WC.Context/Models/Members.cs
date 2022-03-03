namespace Assignment4WC.Context.Models
{
    public class Members
    {
        public int MemberId { get; set; }
        public int AppId { get; set; }
        public string Username { get; set; }
        public int UserScore { get; set; }
        public string QuestionIds { get; set; }
        public int CurrentQuestionNumber { get; set; }
        public int LocationId { get; set; }
        public bool HintAsked { get; set; }
        public bool LocationHintAsked { get; set; }
    }
}