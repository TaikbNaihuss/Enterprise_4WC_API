using Assignment4WC.Models.ResultType.LinkReferencerObjects;
using System;
using System.Linq.Expressions;

namespace Assignment4WC.Models.ResultType
{
    public class Result<T> : Result, IUnwrap<T>, IValueLinkReferencer<T, Result<T>>
    {
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

        public T Unwrap()
        {
            return ResultValue != null ? ResultValue.Value : default;
        }

        public bool HasValue()
        {
            return ResultValue != null;
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

        public Result<T> WithLinks<S>(ValueLink<S> links)
        {
            foreach (var (key, value) in links.Links)
            {
                ResultValue.AddLink(key, value);
            }

            return this;
        }

        public ValueLink<T> GetValueAndLinks() => new(ResultValue.Value, ResultValue.GetLinks(), IsSuccess, ResultValue.StatusCode);
    }

    public class Result : IErrorLinkReferencerProxy<Result>
    {
        protected ResultErrorMessage Error { get; set; }
        public bool IsSuccess { get; set; }

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

        public ErrorMessage GetError()
        {
            return Error;
        }

        public Result<S> ToResult<S>() => new(Error);

        //Primarily using Error.ToActionResult() for this, might want to remove later.
        public ValueLink<string> GetErrorAndLinks() => new(Error.Message, Error.GetLinks(), IsSuccess, Error.StatusCode);

    }
}