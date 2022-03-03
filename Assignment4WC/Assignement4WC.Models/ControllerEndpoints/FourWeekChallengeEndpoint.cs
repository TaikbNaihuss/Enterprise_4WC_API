using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Assignment4WC.Models.ControllerEndpoints
{
    public static class FourWeekChallengeEndpoint
    {
        public const string StartRoute = "start/{appId}";
        public const string GetQuestionRoute = "question/{username}";
        public const string SubmitAnswerRoute = "/question/{username}/submit";
        public const string GetNextQuestionRoute = "question/{username}/next";
        public const string GetUserLocationRoute = "location/{username}";
        public const string GetHintRoute = "question/{username}/hint";
        public const string GetLocationHintRoute = "question/location/{username}/hint";
        public const string EndGameRoute = "end/{username}";
        public const string GetUserScoreRoute = "score/{username}";
        public const string GetHighScoresRoute = "score/high";

        public static string InterpolatedStartRoute(string appId) => GetInterpolatedRoute(StartRoute, "appId", appId);
        public static string InterpolatedGetQuestionRoute(string username) => GetInterpolatedRoute(GetQuestionRoute, "username", username);
        public static string InterpolatedSubmitAnswerRoute(string username) => GetInterpolatedRoute(SubmitAnswerRoute, "username", username);
        public static string InterpolatedGetNextQuestionRoute(string username) => GetInterpolatedRoute(GetNextQuestionRoute, "username", username);
        public static string InterpolatedGetUserLocationRoute(string username) => GetInterpolatedRoute(GetUserLocationRoute, "username", username);
        public static string InterpolatedGetHintRoute(string username) => GetInterpolatedRoute(GetHintRoute, "username", username);
        public static string InterpolatedGetLocationHintRoute(string username) => GetInterpolatedRoute(GetLocationHintRoute, "username", username);
        public static string InterpolatedEndGameRoute(string username) => GetInterpolatedRoute(EndGameRoute, "username", username);
        public static string InterpolatedGetUserScoreRoute(string username) => GetInterpolatedRoute(GetUserScoreRoute, "username", username);
        public static string InterpolatedGetHighScoresRoute(string username) => GetInterpolatedRoute(GetHighScoresRoute, "username", username);

        private static string GetInterpolatedRoute(string source, string key, string value) =>
            source.Replace("{" + key + "}", value);
    }
}
