using Assignment4WC.Models.ResultType.LinkReferencerObjects;
using System;
using System.Net;

namespace Assignment4WC.Models.ResultType
{
    public class Result<T> : Result, IUnwrap<T>, IValueLinkReferencer<T, Result<T>>
    {
        public ResultValue<T> ResultValue { get; }

        public Result(T value) 
        {
            ResultValue = (ResultValue<T>) value ?? throw new ArgumentNullException(nameof(value));
            IsSuccess = true;
        }

        public Result(T value, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            ResultValue = new ResultValue<T>(statusCode, value);
            IsSuccess = true;
        }

        public Result(ErrorMessage error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            Error = new ResultErrorMessage(error);
            IsSuccess = false;
        }

        private Result(ResultErrorMessage error)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
            IsSuccess = false;
        }

        public T Unwrap()
        {
            return ResultValue.Value;
        }

        public new Result<T> AddLink(string content)
        {
            if (IsSuccess) ResultValue.AddLink(content);
            else base.AddLink(content);

            return this;
        }

        public new Result<T> AddLink(string key, string content)
        {
            if (IsSuccess) ResultValue.AddLink(key, content);
            else base.AddLink(key, content);
            
            return this;
        }


        public ValueLink<T> GetValueAndLinks() => new(ResultValue.Value, ResultValue.GetLinks(), IsSuccess, ResultValue.StatusCode);

        public Result<S> ToResult<S>() => new(Error);
    }

    public class Result : IErrorLinkReferencerProxy<Result>
    {
        public ResultErrorMessage Error { get; protected set; }
        public bool IsSuccess { get; protected set; }

        public Result() { }

        public Result(ErrorMessage error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            Error = new ResultErrorMessage(error);
            IsSuccess = false;
        }

        public Result Ok()
        {
            IsSuccess = true;
            return this;
        }

        public Result AddLink(string content)
        {
            Error.AddLink(content);
            return this;
        }

        public Result AddLink(string key, string content)
        {
            Error.AddLink(key, content);
            return this;
        }

        //Primarily using Error.ToActionResult() for this, might want to remove later.
        public ValueLink<string> GetErrorAndLinks() => new(Error.Message, Error.GetLinks(), IsSuccess, Error.StatusCode);

    }
}