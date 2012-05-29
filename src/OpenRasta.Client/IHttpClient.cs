using System;
using System.Net;

namespace OpenRasta.Client
{
    /// <summary>
    /// Just random code not part of anything yet.
    /// </summary>
    public interface IHttpClient
    {
        HttpNotifier Notifier { get; set; }
        IClientRequest CreateRequest(Uri uri);
        Func<IWebProxy> Proxy { get; set; }
    }
    public class HttpNotifier
    {
        public static readonly HttpNotifier Default = new HttpNotifier();
        public event Action<IRequestProgress> NewRequest;
        public void RaiseNewRequest(IRequestProgress newRequest)
        {
            var handler = NewRequest;
            if (handler != null)
                handler(newRequest);
        }
    }

    public interface IRequestProgress
    {
        Uri RequestUri { get; }
        event Action<TransferProgress> Upload;
        event Action<TransferProgress> Download;
    }

    public class TransferProgress
    {
        public TransferProgress(long current, long total, bool finished = false)
        {
            Current = current;
            Total = total;
            Finished = finished;
        }

        public long Current { get; private set; }
        public long Total { get; private set; }
        public bool Finished { get; private set; }
    }
}