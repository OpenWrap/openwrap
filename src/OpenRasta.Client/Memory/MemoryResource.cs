using System;
using System.Collections.Generic;
using System.IO;
using OpenRasta.Client;
using OpenRasta.Client.Memory;

namespace Tests.Repositories.factories
{
    public class MemoryResource
    {
        public MemoryResource()
        {
            Operations = new Dictionary<string, Func<IClientRequest, IClientResponse>>(StringComparer.OrdinalIgnoreCase);
        }
        public MemoryResource(MediaType responseType, Stream content)
            : this()
        {
            Operations["HEAD"] = Operations["GET"] = request =>
                new MemoryResponse
                {
                    RequestUri = request.RequestUri,
                    ResponseUri = request.RequestUri,
                    Status = new HttpStatus(200, "OK"),
                    Headers =
                    {
                            { "Content-Type", responseType.ToString() },
                            { "Content-Length", content.Length.ToString() }
                    },
                    Entity = new MemoryEntity(responseType, request.Method == "HEAD" ? null : content)
                };
        }
        public IDictionary<string, Func<IClientRequest, IClientResponse>> Operations { get; private set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}