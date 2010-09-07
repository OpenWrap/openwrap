using System;
using System.Net;

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
            if (_response != null)
            {
                RaiseStatusChanged("Connected.");
                if (_response.ContentLength > 0)
                {
                    _entity = new HttpEntity(new ProgressStream(_response.ContentLength, RaiseProgress, _response.GetResponseStream()));
                }
            }
            else
            {
                RaiseStatusChanged("No response.");
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

        public Uri RequestUri
        {
            get { return _request.RequestUri; }
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
}