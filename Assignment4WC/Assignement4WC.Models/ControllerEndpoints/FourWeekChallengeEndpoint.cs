namespace Assignment4WC.Models.ControllerEndpoints
{
    public static class FourWeekChallengeEndpoint
    {
        public const string GetCategories = "categories";
        public const string StartRoute = "start/{appId}";
        public const string GetQuestionRoute = "question/{username}";
        public const string SubmitAnswerRoute = "question/{username}/submit";
        public const string SubmitPictureAnswerRoute = "question/{username}/submit/picture";
        public const string SetUserLocationRoute = "location/{username}";
        public const string GetHintRoute = "question/{username}/hint";
        public const string GetLocationHintRoute = "question/location/{username}/hint";
        public const string EndGameRoute = "end/{username}";
        public const string GetUserScoreRoute = "score/{username}";
        public const string GetHighScoresRoute = "score/high";


        public static string StartRouteWith(string appId) => GetInterpolatedRoute(StartRoute, "appId", appId);
        public static string GetQuestionRouteWith(string username) => GetInterpolatedRoute(GetQuestionRoute, "username", username);
        public static string SubmitAnswerRouteWith(string username) => GetInterpolatedRoute(SubmitAnswerRoute, "username", username);
        public static string SubmitPictureAnswerRouteWith(string username) => GetInterpolatedRoute(SubmitPictureAnswerRoute, "username", username);
        public static string SetUserLocationRouteWith(string username) => GetInterpolatedRoute(SetUserLocationRoute, "username", username);
        public static string GetHintRouteWith(string username) => GetInterpolatedRoute(GetHintRoute, "username", username);
        public static string GetLocationHintRouteWith(string username) => GetInterpolatedRoute(GetLocationHintRoute, "username", username);
        public static string EndGameRouteWith(string username) => GetInterpolatedRoute(EndGameRoute, "username", username);
        public static string GetUserScoreRouteWith(string username) => GetInterpolatedRoute(GetUserScoreRoute, "username", username);


        private static string GetInterpolatedRoute(string source, string key, string value) =>
            source.Replace("{" + key + "}", value);


      
    }
}
