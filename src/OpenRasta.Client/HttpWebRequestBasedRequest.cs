using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using OpenRasta.Client.IO;

namespace OpenRasta.Client
{
    public class HttpWebRequestBasedRequest : IClientRequest
    {
        readonly MemoryStream _emptyStream;
        readonly HttpEntity _entity;
        readonly List<KeyValuePair<Func<IClientResponse, bool>, Action<IClientResponse>>> _handlers = new List<KeyValuePair<Func<IClientResponse, bool>, Action<IClientResponse>>>();
        readonly HttpWebRequest _request;
        readonly Uri _requestUri;
        NetworkCredential _explicitCredentials;


        public HttpWebRequestBasedRequest(Uri requestUri, WebProxy proxy)
        {
            _requestUri = requestUri;
            _emptyStream = new MemoryStream();
            _entity = new HttpEntity(_emptyStream);
            _request = (HttpWebRequest)WebRequest.Create(requestUri);
            if (proxy != null) _request.Proxy = proxy;
        }

        public event EventHandler<ProgressEventArgs> Progress;
        public event EventHandler<StatusChangedEventArgs> StatusChanged;

        public NetworkCredential Credentials
        {
            get { return _explicitCredentials; }
            set
            {
                _explicitCredentials = value;
                _request.Credentials = new CredentialCache { { _requestUri, "Basic", value }, { _requestUri, "Digest", value } };
            }
        }


        public IEntity Entity
        {
            get { return _entity; }
        }

        public string Method
        {
            get { return _request.Method; }
            set { _request.Method = value; }
        }

        public Uri RequestUri
        {
            get { return _request.RequestUri; }
        }

        public void RegisterHandler(Func<IClientResponse, bool> predicate, Action<IClientResponse> handler)
        {
            _handlers.Add(new KeyValuePair<Func<IClientResponse, bool>, Action<IClientResponse>>(predicate, handler));
        }

        public IClientResponse Send()
        {
            //_request.ContentType = _entity.ContentType.ToString();
                                                                
            RaiseStatusChanged("Connecting to {0}", RequestUri.ToString());
            if (_entity.Stream != null &&
                ((_entity.Stream.CanSeek && _entity.Stream.Length > 0) ||
                 _entity.Stream != _emptyStream))
                SendRequestStream();

            var response = new HttpWebResponseBasedResponse(this, _request);
            var exceptions = new List<Exception>();

            foreach (var handler in _handlers.Where(x => x.Key(response)).Select(x => x.Value))
            {
                try
                {
                    handler(response);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }
            response.HandlerErrors = exceptions;
            return response;
        }

        protected void RaiseProgress(int progress)
        {
            Progress.Raise(this, new ProgressEventArgs(progress));
        }

        protected void RaiseStatusChanged(string status, params object[] args)
        {
            StatusChanged.Raise(this, new StatusChangedEventArgs(string.Format(status, args)));
        }

        void SendRequestStream()
        {
            if (_entity.Stream.CanSeek)
                _request.ContentLength = _entity.Stream.Length;

            var streamToWriteTo = _request.GetRequestStream();

            if (_entity.Stream.CanSeek)
                streamToWriteTo = new ProgressStream(_entity.Stream.Length, RaiseProgress, streamToWriteTo);

            _entity.Stream.CopyTo(streamToWriteTo);
        }
    }
}