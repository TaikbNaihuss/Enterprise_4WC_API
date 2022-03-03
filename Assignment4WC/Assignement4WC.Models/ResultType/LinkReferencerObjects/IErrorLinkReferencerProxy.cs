namespace Assignment4WC.Models.ResultType.LinkReferencerObjects
{
    public interface IErrorLinkReferencerProxy<out TParent> : ILinkReferencerProxy<TParent>
    {
        ValueLink<string> GetErrorAndLinks();
    }
}