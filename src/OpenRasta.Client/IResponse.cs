namespace OpenRasta.Client
{
    public interface IResponse : IMessage
    {
        int StatusCode { get; }
        IResponseHeaders Headers { get; }
    }
}