#nullable enable
using Assignment4WC.Context.Models;
using Assignment4WC.Context.Repositories;
using Assignment4WC.Models;
using Assignment4WC.Models.ControllerEndpoints;
using Assignment4WC.Models.ResultType;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Assignment4WC.Logic
{
    public class FourWeekChallengeManager : IFourWeekChallengeManager
    {
        private const int QuestionCountIncrements = 5;

        private readonly IGlobalRepository _repository;
        private readonly IQuestionRandomiser _questionRandomiser;

        public FourWeekChallengeManager(IGlobalRepository repository, IQuestionRandomiser questionRandomiser)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _questionRandomiser = questionRandomiser ?? throw new ArgumentNullException(nameof(questionRandomiser));
        }

        //Get all the names of each category, and parse them into a string. From there create a new 
        //CategoryWithQuestionCount for each category with the category name and the question count in increments of 
        //QuestionCountIncrements then compile them all into a list. Add a link to help the user proceed
        //to the next endpoint appropriate to this circumstance.
        public Result<List<CategoryWithQuestionCount>> GetCategoriesAndQuestionCount() =>
            new Result<List<CategoryWithQuestionCount>>(
                    Enum.GetNames(typeof(CategoryType))
                        .Select(categoryString => (CategoryType)Enum.Parse(typeof(CategoryType), categoryString))
                        .Select(category => new CategoryWithQuestionCount(
                            category.ToString(),
                            GetQuestionCountInIncrements(category, QuestionCountIncrements)))
                        .ToList())
                .AddLink(FourWeekChallengeEndpoint.StartRoute);


        public Result UpdateUserLocation(string username, decimal latitude, decimal longitude)
        {
            //Retrieve the member associated with the username. Otherwise if there is none, return an error with appropriate link(s).
            var member = _repository.Members.GetMemberOrNull(username);
            if (member == null)
                return GetMemberDoesNotExistError(username);

            //Create a new location object with the current latitude and longitude of the member.
            var newLocation = new Locations
            {
                Latitude = latitude,
                Longitude = longitude
            };

            //Member does not have a location set if their LocationId == 0.
            if (member.LocationId == 0)
            {
                //Add the new location to the Locations table, save the changed.
                //Then retrieve the ID of the newly created object in the database and 
                //assign it to the member.
                _repository.Locations.Add(newLocation);
                _repository.SaveChanges();
                var location = _repository.Locations.GetLocationByLocation(newLocation);
                member.LocationId = location.LocationId;
            }
            else
            {
                //Get the current location in the Locations table from the member's locationId
                //then set the latitude and longitude of the member's location to the current 
                //latitude and longitude.
                var memberLocation = _repository.Locations.GetLocationByLocationId(member.LocationId);
                memberLocation.Latitude = latitude;
                memberLocation.Longitude = longitude;
            }

            //Save the changes and return an OK result.
            _repository.SaveChanges();

            return new Result().Ok();
        }

        public Result<string> GetHintFromQuestion(string username)
        {
            //Get the current complex question data from the member.
            //If an error occurs in that process, return the error with an appropriate link.
            var questionDataResult = GetCurrentComplexQuestionData(username);
            if (!questionDataResult.IsSuccess)
                return questionDataResult.ToResult<string>();

            //Retrieve the member and changed their HintAsked property to true,
            //then save changes.
            _repository.Members.GetMemberOrNull(username)!.HintAsked = true;
            _repository.SaveChanges();

            //Return the hint to the member or if there is one, otherwise return an empty string.
            return questionDataResult.HasValue()
                ? new Result<string>(questionDataResult.Unwrap().Hint)
                : new Result<string>(string.Empty);
        }

        public Result AddNewPlayer(int appId, string username, CategoryType category, int numOfQuestions)
        {
            //Checks if the username exists in the database and returns an error if it does with an appropriate link.
            if (_repository.Members.DoesUsernameExist(username))
                return new Result(new ErrorMessage(HttpStatusCode.BadRequest,
                    $"The username '{username}' already exists, try another."))
                    .AddLink(FourWeekChallengeEndpoint.StartRouteWith(appId.ToString()));

            //Gets a set of questions from a category with a randomised order.
            //If an error occurs in that process, return the error with an appropriate link.
            var result = _questionRandomiser.GetQuestionsWithOrder(numOfQuestions, category);
            if (!result.IsSuccess) return new Result(result.GetError())
                .AddLink(FourWeekChallengeEndpoint.StartRouteWith(appId.ToString()));

            //Add the user as a new member to the database.
            _repository.Members.Add(new Members
            {
                AppId = appId,
                Username = username,
                QuestionIds = result.Unwrap(),
            });

            _repository.SaveChanges();

            return new Result().Ok();
        }

        public Result<Questions> GetCurrentQuestionData(string username)
        {
            //Get the current question ID of the member
            //If an error occurs in that process, return the error with an appropriate link.
            var currentQuestionIdResult = GetMembersCurrentQuestionId(username);
            if (!currentQuestionIdResult.IsSuccess)
                return currentQuestionIdResult.ToResult<Questions>();

            //Get the value from the result.
            var currentQuestionId = currentQuestionIdResult.Unwrap();

            //Get the question the current question ID is linked to.
            //If an error occurs in that process, return an error with an appropriate link.
            var question = _repository.Questions.GetQuestionOrNull(currentQuestionId);
            if (question == null)
                return new Result<Questions>(
                        new ErrorMessage(
                            HttpStatusCode.NotFound,
                            $"Question with ID '{currentQuestionId}' does not exist in database."))
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(username));
            
            //If the question is a simple question, return just the question. Otherwise if it is
            //not a simple question. Add links appropriate for a complex question.
            var result = question.Discriminator == QuestionComplexity.Simple.ToString()
                    ? new Result<Questions>(question)
                    : new Result<Questions>(question)
                        .AddLink("hint", FourWeekChallengeEndpoint.GetHintRouteWith(username))
                        .AddLink("setLocation", FourWeekChallengeEndpoint.SetUserLocationRouteWith(username))
                        .AddLink("locationHint", FourWeekChallengeEndpoint.GetLocationHintRouteWith(username));

            return result;
        }

        public Result<string> GetLocationHintFromQuestion(string username)
        {
            //Get the current complex question data from the member.
            //If an error occurs in that process, return the error with an appropriate link.
            var questionDataResult = GetCurrentComplexQuestionData(username);
            if (!questionDataResult.IsSuccess)
                return questionDataResult.ToResult<string>();

            //Retrieve the member and changed their LocationHintAsked property to true,
            //then save changes.
            _repository.Members.GetMemberOrNull(username)!.LocationHintAsked = true;
            _repository.SaveChanges();

            //Given it has a value, return the hint or return an empty string if not.
            return questionDataResult.HasValue()
                ? new Result<string>(questionDataResult.Unwrap().LocationHint)
                : new Result<string>(string.Empty);
        }

        public Result EndGame(string username)
        {
            //Retrieve the member associated with the username. Otherwise if there is none, return an error with appropriate link(s).
            var member = _repository.Members.GetMemberOrNull(username);
            if (member == null)
                return GetMemberDoesNotExistError(username);

            //IF the game has ended for the member, then return an OK result, otherwise return an error with appropriate link(s).
            return HasGameEnded(member) ? 
                new Result().Ok() : 
                new Result(new ErrorMessage(HttpStatusCode.BadRequest,
                    $"Game has not ended for member with name '{username}.'"))
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(username));

        }

        public Result<int> GetUserScore(string username)    
        {
            //Retrieve the member associated with the username. Otherwise if there is none, return an error with appropriate link(s).
            var member = _repository.Members.GetMemberOrNull(username);

            //If a member exists, return the member value with appropriate link(s).
            //If they don't then return an error with appropriate link(s).
            return member != null ? 
                new Result<int>(member.UserScore) 
                    .AddLink("category", FourWeekChallengeEndpoint.GetCategories)
                    .AddLink("highScore", FourWeekChallengeEndpoint.GetHighScoresRoute)
                : GetMemberDoesNotExistError(username)
                    .ToResult<int>();
        }

        public Result<List<UserScore>> GetHighScores()
        {
            //If there are no members, return an error with appropriate link(s).
            if (!_repository.Members.Any())
                return new Result<List<UserScore>>(
                        new ErrorMessage(HttpStatusCode.BadRequest, "No members currently exist."))
                    .AddLink("categories", FourWeekChallengeEndpoint.GetCategories);
            
            //Return all the high scores in descending order (starting from the highest scores to the lowest)
            //and along with that value, send back appropriate link(s).
            return new Result<List<UserScore>>(_repository.Members.GetUserScoreInDescendingOrder())
                .AddLink("categories", FourWeekChallengeEndpoint.GetCategories);
        }

        public Result<bool> SubmitPictureAnswer(string username, IFormFile picture)
        {
            //Get the pictures filename, split it at the period. Convert it to a list so the "Last()"
            //function can be used to get the last occurrence of that period (which implies its an extension).
            //Given the extension isn't an accepted extenstion, return an error with appropriate link(s).
            var extension = picture.FileName.Split(".").ToList().Last();
            if (extension is not ("jpg" or "png" or "jpeg"))
                return new Result<bool>(new ErrorMessage(HttpStatusCode.UnprocessableEntity,
                            $"File types are limited to '.jpg','.jpeg' and '.png'. The uploaded file type was .{extension}"))
                        .AddLink(FourWeekChallengeEndpoint.SubmitPictureAnswerRouteWith(username));


            //If the picture's length is not greater than zero, the code proceeds as normal.
            //This is because checks in the function that returns the value for this method
            //will return an error appropriate for the circumstance.
            var pictureBase64 = "";
            if (picture.Length > 0)
            {
                //Converts the picture to a byte array, in which it can be converted to a Base64 string.
                using var ms = new MemoryStream();
                picture.CopyTo(ms);
                var fileBytes = ms.ToArray();
                pictureBase64 = Convert.ToBase64String(fileBytes);
            }

            //Submit the Base64 string as the answer for comparison.
            //All values or errors from this would be returned from the below function.
            return SubmitAnswer(username, pictureBase64);
        }

        public Result<bool> SubmitAnswer(string username, string answer)
        {
            //Get the question data that the member is currently has.
            //If an error occurs in that process, return an error with an appropriate link.
            var questionDataResult = GetCurrentQuestionData(username);
            if (!questionDataResult.IsSuccess)
                return questionDataResult.ToResult<bool>();

            //Retrieve the member associated with the username. Otherwise if there is none, return an error with appropriate link(s).
            var member = _repository.Members.GetMemberOrNull(username)!;

            //Get the value from the result.
            var questionData = questionDataResult.Unwrap();
            
            //If the question is simple, assign the variable to null, otherwise return the complex question data from this question.
            var complexQuestionData = questionData.Discriminator == QuestionComplexity.Simple.ToString() ?
                    null :
                    _repository.ComplexQuestions.GetComplexQuestion(questionData.QuestionId);

            //If the answer is any form of the word "PASS" then assign this variable to true, otherwise false.
            var skippedQuestion = string.Equals(answer, "PASS", StringComparison.CurrentCultureIgnoreCase);

            //By default this variable is true, in cases where the question is simple, the location can be considered the same,
            //otherwise this value is evaluated in the case that its a complex question.
            var isLocationCorrect = true;

            //Executes the following code in the case that the question is skipped or is correct.
            if (questionData.CorrectAnswer == answer || skippedQuestion)
            {
                //NARRATIVE:
                //Given the question is not skipped
                if (!skippedQuestion)
                {
                    //And the question is complex.
                    if (complexQuestionData != null)
                    {
                        //Then check if the locations are the close enough to each other.
                        //If an error occurs in that process, return an error with an appropriate link.
                        var isLocationCorrectResult = AreLocationsCloseEnough(complexQuestionData, member);
                        if (!isLocationCorrectResult.IsSuccess)
                            return isLocationCorrectResult;

                        //Get the value from the result.
                        isLocationCorrect = isLocationCorrectResult.Unwrap();
                    }

                    //If the location is correct, then add to their user score and 
                    //increment the question number they are currently on.
                    if (isLocationCorrect)
                    {
                        AddToUserScore(member);
                        member.CurrentQuestionNumber++;
                    }
                    //END NARRATIVE
                }
                else
                //If the question was skipped, only increment the question number.
                    member.CurrentQuestionNumber++;

                //Save changes to the database.
                _repository.SaveChanges();
            }

            //Return a boolean result value based off if the answer and location was correct or if the user skipped the question.
            var result = new Result<bool>((questionData.CorrectAnswer == answer && isLocationCorrect) || skippedQuestion);

            //Depending of if the game ended at this point, then return the appropriate value link(s).
            return !HasGameEnded(member) ?
                result.AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(username)) :
                result.AddLink(FourWeekChallengeEndpoint.EndGameRouteWith(username));
        }

        private Result<bool> AreLocationsCloseEnough(ComplexQuestions complexQuestionData, Members member)
        {
            const int correctLocationRadiusInMeters = 20;

            //Get the current location in the Locations table from the member's locationId
            //If an error occurs in that process, return an error with an appropriate link.
            var memberLocation = _repository.Locations.GetLocationByLocationIdOrNull(member.LocationId);
            if (memberLocation == null)
                return new Result<bool>(
                        new ErrorMessage(HttpStatusCode.NotFound,
                            $"Member with username '{member.Username}' does not have a location set."))
                    .AddLink(FourWeekChallengeEndpoint.SetUserLocationRouteWith(member.Username));

            //If the member's location is within the "correctLocationRadiusInMeters"'s value of the complex questions location
            //Then assign the value to true, otherwise false.
            var withinLocationRadius = memberLocation.GetDistanceInMeters(complexQuestionData.Location) <= correctLocationRadiusInMeters;

            return new Result<bool>(withinLocationRadius);
        }

        private IEnumerable<int> GetQuestionCountInIncrements(CategoryType category, int increments)
        {
            //Get the count of the questions within the category.
            var questionCount = _repository.Questions.CountQuestionsFromCategory(category);

            //Iterate over the question count in increments and return a collection of all
            //the valid question counts to pick from.
            for (var i = 0; i < questionCount / increments; i++)
            {
                yield return increments * (i + 1);
            }
        }

        private Result<ComplexQuestions> GetCurrentComplexQuestionData(string username)
        {
            //Get the current question ID of the member
            //If an error occurs in that process, return the error with an appropriate link.
            var currentQuestionIdResult = GetMembersCurrentQuestionId(username);
            if (!currentQuestionIdResult.IsSuccess)
                return currentQuestionIdResult.ToResult<ComplexQuestions>();

            //Get the value from the result.
            var currentQuestionId = currentQuestionIdResult.Unwrap();

            //Get the current complex question from the current question ID.
            var complexQuestion = _repository.ComplexQuestions.GetComplexQuestion(currentQuestionId);

            //Check if the question exists in the first place.
            var questionExists = _repository.Questions.DoesQuestionExist(currentQuestionId);

            //If the complex question exists, return it.
            //If the complex question doesn't exist, but the question does, the its not a complex question and return an error with appropriate link(s).
            //If the question itself with the question ID provided doesn't exist, then return an error with appropriate link(s)
            return complexQuestion != null ?
                new Result<ComplexQuestions>(complexQuestion) :
                questionExists ?
                    new Result<ComplexQuestions>(new ErrorMessage(HttpStatusCode.BadRequest,
                            $"This question is not a complex question. Cannot provide additional details for this question."))
                        .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(username)) :
                    new Result<ComplexQuestions>(new ErrorMessage(HttpStatusCode.NotFound,
                            $"Question with ID '{currentQuestionId}' does not exist in database."))
                        .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(username));
        }

        private Result<int> GetMembersCurrentQuestionId(string username)
        {
            //Retrieve the member associated with the username. Otherwise if there is none, return an error with appropriate link(s).
            var member = _repository.Members.GetMemberOrNull(username);
            if (member == null)
                return GetMemberDoesNotExistError(username)
                    .ToResult<int>();

            //Get the current question ID from the member.
            var currentQuestionIdResult = GetCurrentQuestionId(member);

            //If the result was success, return the result
            //otherwise if an error occurred in the process, return an error with appropriate link(s).
            return currentQuestionIdResult.IsSuccess ?
                currentQuestionIdResult :
                currentQuestionIdResult.ToResult<int>();
        }

        private static void AddToUserScore(Members member)
        {
            const int basePointsConstant = 2;
            var basePoints = basePointsConstant;

            //Given the member has asked a hint, deduct points and reset the HintAsked property to false.
            if (member.HintAsked)
            {
                basePoints--;
                member.HintAsked = false;
            }
            //Given the member has asked a location hint, deduct points and reset the LocationHintAsked property to false.
            if (member.LocationHintAsked)
            {
                basePoints--;
                member.LocationHintAsked = false;
            }

            //Add to the User-score property of the member with the acquired cumulative base points.
            member.UserScore += basePoints;
        }

        private static Result<int> GetCurrentQuestionId(Members member)
        {
            //FOR REFERENCE: The question ID's are stored as a comma separated string the database
            //E.g. "23,2,14,45,6"

            //Split the member's question ID's by the comma in the string.
            var currentQuestionIndex = member.CurrentQuestionNumber;
            var questionIds = member.QuestionIds.Split(",");

            //If the length of the collection of quetion ID's is less than the current index, the return an error with appropriate link(s).
            if (questionIds.Length < currentQuestionIndex)
                return new Result<int>(new ErrorMessage(HttpStatusCode.InternalServerError,
                    $"Index '{nameof(member.CurrentQuestionNumber)}' was outside the range for the number of questionIds the member has."));

            //If the length of the collection of question ID's not equal to the current index, 
            //then return the question id at the current index.
            //Otherwise return an error with appropriate link(s).
            return questionIds.Length != currentQuestionIndex ? 
                new Result<int>(int.Parse(questionIds[currentQuestionIndex])) :
                new Result<int>(new ErrorMessage(HttpStatusCode.NotFound, $"Game has ended for member with name '{member.Username}'."))
                    .AddLink(FourWeekChallengeEndpoint.EndGameRouteWith(member.Username));
        }


        //Returns error associated with a member not existing with a certain username.
        private static Result GetMemberDoesNotExistError(string username) =>
            new Result(new ErrorMessage(HttpStatusCode.NotFound,
                    $"Member with username '{username}' does not exist."))
                .AddLink(FourWeekChallengeEndpoint.GetCategories);

        //Used to determine whether a game has ended if the length of the collection of question Id's 
        //is the same as their current question number, A.K.A the index of their current question.
        private static bool HasGameEnded(Members member) =>
            member.QuestionIds.Split(",").Length == member.CurrentQuestionNumber;
    }
}
