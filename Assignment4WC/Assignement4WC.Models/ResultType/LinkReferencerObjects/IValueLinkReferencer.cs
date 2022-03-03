namespace Assignment4WC.Models.ResultType.LinkReferencerObjects
{
    public interface IValueLinkReferencer<TContent, out TParent> : ILinkReferencerProxy<TParent>
    {
        ValueLink<TContent> GetValueAndLinks();
    }
}