using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Assignment4WC.Models.ResultType;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Numeric;
using FluentAssertions.Primitives;
using Newtonsoft.Json;

namespace Assignment4WC.Tests
{
    public static class AndConstraintExtensions
    {
        [CustomAssertion]
        public static AndConstraint<ObjectAssertions> BeResultEquivalentTo(this ObjectAssertions parent, Result expectedResult)
        {
            var parentResult = (Result)parent.Subject;

            Execute.Assertion
                .Given(() => parentResult)
                .ForCondition(result => result.IsSuccess == expectedResult.IsSuccess)
                .FailWith("Actual result and expected result success states differ ." +
                          $"\nActual IsSuccess: '{parentResult.IsSuccess}'" +
                          $"\nExpected IsSuccess: '{expectedResult.IsSuccess}'");

            Execute.Assertion
                .Given(() => parentResult)
                .ForCondition(_ => !AreEitherErrorsNull(parentResult, expectedResult))
                .FailWith("Actual result and expected result errors differ." +
                          $"\nActual Error Is Null: '{parentResult.GetError() == null}'" +
                          $"\nExpected Error Is Null: '{expectedResult.GetError() == null}'");

            if (parentResult.GetError() != null && expectedResult.GetError() != null)
            {

                Execute.Assertion
                    .Given(() => parentResult)
                    .ForCondition(result => result.GetError()?.StatusCode == expectedResult.GetError().StatusCode)
                    .FailWith("Actual result and expected result error status codes differ." +
                              $"\nActual Error StatusCode: '{parentResult.GetError().StatusCode}'" +
                              $"\nExpected Error StatusCode: '{expectedResult.GetError().StatusCode}'");

                Execute.Assertion
                    .Given(() => (Result)parent.Subject)
                    .ForCondition(result => result.GetError().Message == expectedResult.GetError().Message)
                    .FailWith("Actual result and expected result error messages differ." +
                              $"\nActual Error Message: '{parentResult.GetError().Message}'" +
                              $"\nExpected Error Message: '{expectedResult.GetError().Message}'");


                Execute.Assertion
                    .Given(() => parentResult)
                    .ForCondition(_ => CompareErrorLinkDictionaries(parentResult, expectedResult))
                    .FailWith("Actual result and expected result error links differ." +
                              $"\nActual Error Links:\n{GetKeyAndValues(parentResult.GetErrorAndLinks().Links)}\n" +
                              $"\nExpected Error Links:\n{GetKeyAndValues(expectedResult.GetErrorAndLinks().Links)}");
            }

            return new AndConstraint<ObjectAssertions>(parent);
        }

        [CustomAssertion]
        public static AndConstraint<ObjectAssertions> BeResultEquivalentTo<TType>(this ObjectAssertions parent, Result<TType> expectedResult)
        {
            var parentResult = (Result<TType>)parent.Subject;

            Execute.Assertion
                .Given(() => parentResult)
                .ForCondition(result => result.IsSuccess == expectedResult.IsSuccess)
                .FailWith("Actual result and expected result success states differ ." +
                          $"\nActual IsSuccess: '{parentResult.IsSuccess}'" +
                          $"\nExpected IsSuccess: '{expectedResult.IsSuccess}'");


            Execute.Assertion
                .Given(() => parentResult)
                .ForCondition(_ => !AreEitherErrorsNull(parentResult, expectedResult))
                .FailWith("Actual result and expected result errors differ." +
                          $"\nActual Error Is Null: '{parentResult.GetError() == null}'" +
                          $"\nExpected Error Is Null: '{expectedResult.GetError() == null}'");

            if (parentResult.GetError() != null && expectedResult.GetError() != null)
            {
                Execute.Assertion
                    .Given(() => parentResult)
                    .ForCondition(result => result.GetError().StatusCode == expectedResult.GetError().StatusCode)
                    .FailWith("Actual result and expected result error status codes differ." +
                              $"\nActual Error StatusCode: '{parentResult.GetError().StatusCode}'" +
                              $"\nExpected Error StatusCode: '{expectedResult.GetError().StatusCode}'");

                Execute.Assertion
                    .Given(() => parentResult)
                    .ForCondition(result => result.GetError().Message == expectedResult.GetError().Message)
                    .FailWith("Actual result and expected result error messages differ." +
                              $"\nActual Error Message: '{parentResult.GetError().Message}'" +
                              $"\nExpected Error Message: '{expectedResult.GetError().Message}'");

                Execute.Assertion
                    .Given(() => parentResult)
                    .ForCondition(_ => CompareErrorLinkDictionaries(parentResult, expectedResult))
                    .FailWith("Actual result and expected result error links differ." +
                              $"\nActual Error Links:\n{GetKeyAndValues(parentResult.GetErrorAndLinks().Links)}\n" +
                              $"\nExpected Error Links:\n{GetKeyAndValues(expectedResult.GetErrorAndLinks().Links)}");
            }

            Execute.Assertion
                .Given(() => parentResult)
                .ForCondition(_ => parentResult.HasValue() == expectedResult.HasValue())
                .FailWith("Actual result and expected result value links differ." +
                          $"\nActual Has Value:\n{parentResult.HasValue()}\n" +
                          $"\nExpected Has Value:\n{expectedResult.HasValue()}");

            if (parentResult.HasValue() && expectedResult.HasValue())
            {
                Execute.Assertion
                    .Given(() => parentResult)
                    .ForCondition(_ => JsonCompare(parentResult, expectedResult))
                    .FailWith("Actual result and expected result contain different values." +
                              $"\nActual Has Value:\n{JsonConvert.SerializeObject(parentResult.Unwrap(), Formatting.Indented)}\n" +
                              $"\nExpected Has Value:\n{JsonConvert.SerializeObject(expectedResult.Unwrap(), Formatting.Indented)}");

                Execute.Assertion
                    .Given(() => parentResult)
                    .ForCondition(_ => CompareValueLinkDictionaries(parentResult, expectedResult))
                    .FailWith("Actual result and expected result value links differ." +
                              $"\nActual Value Links:\n{GetKeyAndValues(parentResult.GetValueAndLinks().Links)}\n" +
                              $"\nExpected Value Links:\n{GetKeyAndValues(expectedResult.GetValueAndLinks().Links)}");
            }

           


            return new AndConstraint<ObjectAssertions>(parent);
        }



        private static string GetKeyAndValues(Dictionary<string, string> links)
        {
            return "{" + string.Join(Environment.NewLine, links.Select(kv => $"Key: {kv.Key},     Value: {kv.Value}").ToArray()) + "}";
        }

        private static bool AreEitherErrorsNull(Result parentResult, Result expectedResult)
        {
            var x = parentResult.GetError();
            var y = expectedResult.GetError();

            return (x == null && y != null) || (x != null && y == null);
        }

        private static bool AreEitherErrorsNull<TType>(Result<TType> parentResult, Result<TType> expectedResult)
        {
            var x = parentResult.GetError();
            var y = expectedResult.GetError();

            return (x == null && y != null) || (x != null && y == null);
        }

        //private static bool AreEitherValueLinks<TType>(Result<TType> parentResult, Result<TType> expectedResult)
        //{
        //    var x = parentResult.GetValueAndLinks().Links;
        //    var y = expectedResult.GetValueAndLinks().Links;

        //    return (x == null && y != null) || (x != null && y == null);
        //}

        //private static bool AreEitherErrorLinksNull(Result parentResult, Result expectedResult)
        //{
        //    var x = parentResult.GetErrorAndLinks().Links;
        //    var y = expectedResult.GetErrorAndLinks().Links;

        //    return (x == null && y != null) || (x != null && y == null);
        //}

        //private static bool AreEitherErrorLinkssNull(Result parentResult, Result expectedResult)
        //{
        //    var x = parentResult.GetErrorAndLinks().Links;
        //    var y = expectedResult.GetErrorAndLinks().Links;

        //    return (x == null && y != null) || (x != null && y == null);
        //}
        private static bool CompareValueLinkDictionaries<TType>(Result<TType> parentResult, Result<TType> expectedResult)
        {
            var x = parentResult.GetValueAndLinks().Links;
            var y = expectedResult.GetValueAndLinks().Links;

            return x.Keys.Count == y.Keys.Count && x.Keys.All(k => y.ContainsKey(k) && x[k] == y[k]);
        }

        private static bool CompareErrorLinkDictionaries<TType>(Result<TType> parentResult, Result<TType> expectedResult)
        {
            var x = parentResult.GetErrorAndLinks().Links;
            var y = expectedResult.GetErrorAndLinks().Links;

            return x.Keys.Count == y.Keys.Count && x.Keys.All(k => y.ContainsKey(k) && x[k] == y[k]);
        }

        private static bool CompareErrorLinkDictionaries(Result parentResult, Result expectedResult)
        {
            var x = parentResult.GetErrorAndLinks().Links;
            var y = expectedResult.GetErrorAndLinks().Links;

            return x.Keys.Count == y.Keys.Count && x.Keys.All(k => y.ContainsKey(k) && x[k] == y[k]);
        }

        public static bool JsonCompare<TType>(this Result<TType> obj, Result<TType> another)
        {
            if (ReferenceEquals(obj, another)) return true;
            if ((obj == null) || (another == null)) return false;
            if (obj.GetType() != another.GetType()) return false;

            var objJson = JsonConvert.SerializeObject(obj.Unwrap());
            var anotherJson = JsonConvert.SerializeObject(another.Unwrap());

            return objJson == anotherJson;
        }
    }
}