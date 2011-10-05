using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenFileSystem.IO;
using OpenRasta.Client;
using OpenWrap.Configuration;
using OpenWrap.Repositories.FileSystem;
using OpenWrap.Repositories.Http;
using OpenWrap.Repositories.NuGet;

namespace OpenWrap.Repositories
{
    public interface IRemoteRepositoryFactory
    {
        IPackageRepository FromUserInput(string directoryPath);

        IPackageRepository FromToken(string token);
    }

    public static class ClientExtensions
    {
        public static IClientRequest Head(this IHttpClient client, Uri uri)
        {
            return client.CreateRequest(uri).Head();
        }

        public static IClientRequest Get(this IHttpClient client, Uri uri)
        {
            return client.CreateRequest(uri).Get();
        }
        public static IClientRequest Head(this IHttpClient client, string uri)
        {
            return client.CreateRequest(new Uri(uri, UriKind.RelativeOrAbsolute)).Head();
        }
        public static IClientRequest Handle(this IClientRequest request, Func<IClientResponse, bool> predicate, Action<IClientResponse> response)
        {
            request.RegisterHandler(predicate, response);
            return request;
        }
        public static IClientRequest Handle(this IClientRequest request, Action<IClientResponse> response)
        {
            return request.Handle(_ => true, response);
        }
        public static IClientRequest Handle(this IClientRequest request, int statusCode, Action<IClientResponse> response)
        {
            return request.Handle(_ => _.Status.Code == statusCode, response);
        }
    }
}