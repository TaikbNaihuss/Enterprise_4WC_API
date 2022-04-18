using Assignment4WC.Models.ResultType.LinkReferencerObjects;
using System;
using System.Linq.Expressions;
using Assignment4WC.Models.ControllerEndpoints;

namespace Assignment4WC.Models.ResultType
{
    public class Result<T> : Result, IUnwrap<T>, IValueLinkReferencer<T, Result<T>>
    {
        //Encapsulates the value of a successful operation
        private ResultValue<T> ResultValue { get; }

        public Result(T value) 
        {
            ResultValue = (ResultValue<T>) value;
            IsSuccess = true;
        }

        public Result(ErrorMessage error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            Error = new ResultErrorMessage(error);
            IsSuccess = false;
        }

        public Result(ResultErrorMessage error)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
            IsSuccess = false;
        }

        //Takes the value out of a ResultValue type
        public T Unwrap()
        {
            return ResultValue != null ? ResultValue.Value : default;
        }

        public bool HasValue()
        {
            return ResultValue != null;
        }

        //Adds links to values with the key "href"
        public new Result<T> AddLink(HateoasString content)
        {
            if (IsSuccess) ResultValue.AddLink(content);
            else base.AddLink(content);

            return this;
        }

        //Adds links to values with the key and value provided by the developer
        public new Result<T> AddLink(string key, HateoasString content)
        {
            if (IsSuccess) ResultValue.AddLink(key, content);
            else base.AddLink(key, content);
            
            return this;
        }

        //Takes existing value links in one result and appends them to this one.
        public Result<T> WithLinks<S>(ValueLink<S> links)
        {
            foreach (var (key, value) in links.Links)
            {
                ResultValue.AddLink(key, value);
            }

            return this;
        }

        //Returns the value of the result, its links as well as additional parameters for mapping to an ActionResult. 
        //Provides a visually and structurally nice way to serialise the data into JSON.
        public ValueLink<T> GetValueAndLinks() => new(ResultValue.Value, ResultValue.GetLinks(), IsSuccess, ResultValue.StatusCode);
    }

    public class Result : IErrorLinkReferencerProxy<Result>
    {
        //Encapsulates the value of a failed operation
        protected ResultErrorMessage Error { get; set; }
        public bool IsSuccess { get; set; }

        public Result() { }

        public Result(ErrorMessage error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            Error = new ResultErrorMessage(error);
            IsSuccess = false;
        }

        //A method to show a successful operation without a value.
        public Result Ok()
        {
            IsSuccess = true;
            return this;
        }

        //Adds links to errors with the key "href"
        public Result AddLink(HateoasString content)
        {
            Error.AddLink(content);
            return this;
        }

        //Adds links to errors with the key and value provided by the developer
        public Result AddLink(string key, HateoasString content)
        {
            Error.AddLink(key, content);
            return this;
        }

        public ErrorMessage GetError()
        {
            return Error;
        }

        //Converts a Result of one type to another of a different type.
        //Only used to carry back error messages from failed operations.
        public Result<S> ToResult<S>() => new(Error);

        //Returns the error of the result, its links as well as additional parameters for mapping to an ActionResult. 
        //Provides a visually and structurally nice way to serialise the data into JSON.
        public ValueLink<string> GetErrorAndLinks() => new(Error.Message, Error.GetLinks(), IsSuccess, Error.StatusCode);

    }
}