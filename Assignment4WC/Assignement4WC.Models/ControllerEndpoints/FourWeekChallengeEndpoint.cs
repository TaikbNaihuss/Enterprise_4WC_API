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

        public static HateoasString GetCategoriesHateoas = new(GetCategories, HttpAction.GET.ToString());
        public static HateoasString StartRouteHateoas = new(StartRoute, HttpAction.POST.ToString());
        public static HateoasString GetQuestionRouteHateoas = new(GetQuestionRoute, HttpAction.GET.ToString());
        public static HateoasString SubmitAnswerRouteHateoas = new(SubmitAnswerRoute, HttpAction.POST.ToString());
        public static HateoasString SubmitPictureAnswerRouteHateoas = new(SubmitPictureAnswerRoute, HttpAction.POST.ToString());
        public static HateoasString SetUserLocationRouteHateoas = new(SetUserLocationRoute, HttpAction.PUT.ToString());
        public static HateoasString GetHintRouteHateoas = new(GetHintRoute, HttpAction.GET.ToString());
        public static HateoasString GetLocationHintRouteHateoas = new(GetLocationHintRoute, HttpAction.GET.ToString());
        public static HateoasString EndGameRouteHateoas = new(EndGameRoute, HttpAction.PUT.ToString());
        public static HateoasString GetUserScoreRouteHateoas = new(GetUserScoreRoute, HttpAction.GET.ToString());
        public static HateoasString GetHighScoresRouteHateoas = new(GetHighScoresRoute, HttpAction.GET.ToString());
        
        public static HateoasString StartRouteWith(string appId) => GetInterpolatedRoute(StartRouteHateoas, "appId", appId);
        public static HateoasString GetQuestionRouteWith(string username) => GetInterpolatedRoute(GetQuestionRouteHateoas, "username", username);
        public static HateoasString SubmitAnswerRouteWith(string username) => GetInterpolatedRoute(SubmitAnswerRouteHateoas, "username", username);
        public static HateoasString SubmitPictureAnswerRouteWith(string username) => GetInterpolatedRoute(SubmitPictureAnswerRouteHateoas, "username", username);
        public static HateoasString SetUserLocationRouteWith(string username) => GetInterpolatedRoute(SetUserLocationRouteHateoas, "username", username);
        public static HateoasString GetHintRouteWith(string username) => GetInterpolatedRoute(GetHintRouteHateoas, "username", username);
        public static HateoasString GetLocationHintRouteWith(string username) => GetInterpolatedRoute(GetLocationHintRouteHateoas, "username", username);
        public static HateoasString EndGameRouteWith(string username) => GetInterpolatedRoute(EndGameRouteHateoas, "username", username);
        public static HateoasString GetUserScoreRouteWith(string username) => GetInterpolatedRoute(GetUserScoreRouteHateoas, "username", username);

        private static HateoasString GetInterpolatedRoute(HateoasString source, string key, string value)
        {
            var newRoute = source.RouteName.Replace("{" + key + "}", value);
            return new HateoasString(newRoute, source.HttpAction);
        }




    }
}
