namespace OpenRasta.Client
{
    public interface IClientRequest : IRequest, IProgressNotification
    {
        IClientResponse Send();
    }
}