namespace OpenRasta.Client
{
    public interface IResponse : IMessage
    {
        HttpStatus Status { get; }
        IResponseHeaders Headers { get; }
    }
}