using System;
using System.IO;
using System.Net;
using OpenRasta.IO;

namespace OpenRasta.Client
{
    public class HttpWebResponseBasedResponse : IClientResponse
    {
        readonly HttpWebRequestBasedRequest _request;
        readonly HttpWebResponse _response;
        HttpEntity _entity;
        IResponseHeaders _headers = new ResponseHeaders();

        public HttpWebResponseBasedResponse(HttpWebRequestBasedRequest request, HttpWebRequest nativeRequest)
        {
            _request = request;
            try
            {
                _response = (HttpWebResponse)nativeRequest.GetResponse();
            }
            catch (WebException e)
            {
                _response = (HttpWebResponse)e.Response;
            }
            RaiseStatusChanged("Connected.");
            if (_response.ContentLength > 0)
            {
                _entity = new HttpEntity(new ProgressStream(_response.ContentLength, RaiseProgress, _response.GetResponseStream()));
            }
        }
        
        public int StatusCode
        {
            get { return (int)_response.StatusCode; }
        }

        public IResponseHeaders Headers
        {
            get { return _headers; }
        }

        public IEntity Entity
        {
            get { return _entity; }
        }

        protected void RaiseStatusChanged(string status, params object[] args)
        {
            StatusChanged.Raise(this, new StatusChangedEventArgs(string.Format(status, args)));

        }
        protected void RaiseProgress(int progress)
        {
            Progress.Raise(this, new ProgressEventArgs(progress));
        }
        public event EventHandler<StatusChangedEventArgs> StatusChanged;
        public event EventHandler<ProgressEventArgs> Progress;
    }

    public class ProgressStream : WrapperStream
    {
        readonly long _size;
        long _total = 0;
        Action<int> _progressNotifier;

        public ProgressStream(long size,Action<int> progressNotifier, Stream underlyingStream)
            : base(underlyingStream)
        {
            _size = size;
            _progressNotifier = progressNotifier;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = base.Read(buffer, offset, count);
            NotifyProgress(read);
            return read;
        }

        void NotifyProgress(int amount)
        {
            _total += amount;

            _progressNotifier((int)(((double)_total / _size) * 100));
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);
            NotifyProgress(count);
        }
    }
}