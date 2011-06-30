using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Tests.Repositories.factories;

namespace OpenRasta.Client.Memory
{
    public class MemoryHttpClient : IHttpClient
    {
        public MemoryHttpClient()
        {
            Resources = new Dictionary<Uri, MemoryResource>();

        }
        public IClientRequest CreateRequest(Uri uri)
        {
            return new MemoryRequest(this, uri);
        }

        public Func<IWebProxy> Proxy { get; set; }

        internal class MemoryRequest : IClientRequest
        {
            readonly MemoryHttpClient _client;

            public MemoryRequest(MemoryHttpClient client, Uri requestUri)
            {
                _client = client;
                RequestUri = requestUri;
                Handlers = new List<KeyValuePair<Func<IClientResponse, bool>, Action<IClientResponse>>>();
            }

            public IEntity Entity { get; set; }

            public Uri RequestUri { get; private set; }

            public string Method { get; set; }
            public event EventHandler<StatusChangedEventArgs> StatusChanged = (s, e) => { };
            public event EventHandler<ProgressEventArgs> Progress = (s, e) => { };
            public IClientResponse Send()
            {
                var response = RedirectIfNeeded(_client.Execute(this), this);

                foreach (var handler in Handlers.Where(x => x.Key(response)).Select(x => x.Value)) handler(response);
                return response;
            }

            IClientResponse RedirectIfNeeded(IClientResponse response, MemoryRequest request)
            {
                if (response.Status.Code == 301 ||
                    response.Status.Code == 302 ||
                    response.Status.Code == 303 ||
                    response.Status.Code == 307)
                {
                    return _client.CreateRequest(response.Headers["Location"].ToUri()).Method(request.Method).Send();
                }
                return response;
            }

            public NetworkCredential Credentials { get; set; }
            public void RegisterHandler(Func<IClientResponse, bool> predicate, Action<IClientResponse> handler)
            {
                Handlers.Add(new KeyValuePair<Func<IClientResponse, bool>, Action<IClientResponse>>(predicate, handler));
            }

            protected List<KeyValuePair<Func<IClientResponse, bool>, Action<IClientResponse>>> Handlers { get; set; }
        }

        public Dictionary<Uri, MemoryResource> Resources { get; private set; }

        IClientResponse Execute(IClientRequest request)
        {
            MemoryResource resource;
            if (!Resources.TryGetValue(request.RequestUri, out resource))
            {
                return new MemoryResponse
                {
                        Status = new HttpStatus(404, "Nout found"),
                        Headers = { { "Content-Length", "0" } }
                };
            }
            Func<IClientRequest, IClientResponse> methodHandler;
            if (!resource.Operations.TryGetValue(request.Method,out methodHandler))
            {
                return new MemoryResponse
                {
                        Status = new HttpStatus(405, "Method not allowed"),
                        Headers = { { "Content-Length", "0" } }
                };
            }
            return methodHandler(request);

        }
    }

    public class MemoryResponse : IClientResponse
    {
        public MemoryResponse()
        {
            Headers = new ResponseHeaders();

        }
        public MemoryResponse(int statusCode) : this()
        {
            Status = new HttpStatus(statusCode, string.Empty);
        }

        public IEntity Entity { get; set; }
        public Uri RequestUri { get; set; }
        public HttpStatus Status { get; set; }
        public IResponseHeaders Headers { get; private set; }
        public event EventHandler<StatusChangedEventArgs> StatusChanged;
        public event EventHandler<ProgressEventArgs> Progress;

        public Uri ResponseUri { get; set; }
    }

}