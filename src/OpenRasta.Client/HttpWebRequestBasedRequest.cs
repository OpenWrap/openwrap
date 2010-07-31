using System;
using System.IO;
using System.Net;
using OpenRasta.Client.IO;

namespace OpenRasta.Client
{
    public class HttpWebRequestBasedRequest : IClientRequest
    {
        HttpWebRequest _request;
        HttpEntity _entity;
        MemoryStream _emptyStream;

        public HttpWebRequestBasedRequest(Uri requestUri)
        {
            _request = (HttpWebRequest)WebRequest.Create(requestUri);
            _emptyStream = new MemoryStream();
            _entity = new HttpEntity(_emptyStream);
        }

        public event EventHandler<StatusChangedEventArgs> StatusChanged;
        public event EventHandler<ProgressEventArgs> Progress;


        public IClientResponse Send()
        {
            _request.ContentType = _entity.ContentType.ToString();
            RaiseStatusChanged("Connecting to {0}", Uri.ToString());
            if (_entity.Stream != null &&
                ((_entity.Stream.CanSeek && _entity.Stream.Length > 0) ||
                _entity.Stream != _emptyStream))
                SendRequestStream();

            return new HttpWebResponseBasedResponse(this, _request);
        }

        void SendRequestStream()
        {
            var streamToWriteTo = _request.GetRequestStream();
            if (_entity.Stream.CanSeek)
            {
                _request.ContentLength = _entity.Stream.Length;
                streamToWriteTo = new ProgressStream(_entity.Stream.Length, RaiseProgress, streamToWriteTo);
            }
            _entity.Stream.CopyTo(streamToWriteTo);
        }

        protected void RaiseStatusChanged(string status, params object[] args)
        {
            StatusChanged.Raise(this, new StatusChangedEventArgs(string.Format(status, args)));

        }
        protected void RaiseProgress(int progress)
        {
            Progress.Raise(this, new ProgressEventArgs(progress));
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

        public Uri Uri
        {
            get { return _request.RequestUri; }
        }
    }
}