using System;
using System.Collections.Generic;
using System.Net;

namespace OpenRasta.Client
{
    public class HttpWebResponseBasedResponse : IClientResponse
    {
        readonly HttpWebRequestBasedRequest _request;
        readonly HttpWebResponse _response;
        HttpEntity _entity;
        IResponseHeaders _headers = new ResponseHeaders();

        public HttpWebResponseBasedResponse(HttpWebRequestBasedRequest request, HttpWebRequest nativeRequest, Action<TransferProgress> notifyProgress)
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
                Status = new HttpStatus((int)_response.StatusCode, _response.StatusDescription);
                RaiseStatusChanged("Connected.");
                if (_response.ContentLength == -1 || _response.ContentLength > 0)
                {
                    _entity = new HttpEntity(new ProgressStream(_response.ContentLength, 
                        notifyProgress,
                        _response.GetResponseStream()));
                }
            }
            else
            {
                Status = new HttpStatus(-1, "No response");
                RaiseStatusChanged("No response.");
            }
        }
        
        public int StatusCode
        {
            get { return (int)_response.StatusCode; }
        }

        public HttpStatus Status { get; private set; }

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

        public IEnumerable<Exception> HandlerErrors { get; set; }

        protected void RaiseStatusChanged(string status, params object[] args)
        {
            StatusChanged.Raise(this, new StatusChangedEventArgs(string.Format(status, args)));

        }
        public event EventHandler<StatusChangedEventArgs> StatusChanged;
        public event EventHandler<ProgressEventArgs> Progress;

        public Uri ResponseUri
        {
            get { return _response.ResponseUri; }
        }
    }
}