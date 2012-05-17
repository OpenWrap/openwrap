using System;
using System.IO;
using System.Net;
using OpenRasta.Client;
using OpenWrap.Tasks;

namespace OpenWrap.Repositories.Http
{
    public class HttpRepositoryNavigator : IHttpRepositoryNavigator, ISupportAuthentication
    {
        readonly IHttpClient _httpClient;
        readonly Uri _baseUri;
        readonly Func<Uri, IClientRequest> _httpClientGetter;
        readonly Uri _requestUri;
        PackageFeed _fileList;
        NetworkCredential _availableCredentials;



        public HttpRepositoryNavigator(IHttpClient httpClient, Uri baseUri)
        {
            _httpClient = httpClient;
            _baseUri = baseUri;
            _requestUri = new Uri(baseUri, new Uri("index.wraplist", UriKind.Relative));
            _httpClientGetter = (uri) => _availableCredentials != null
                                              ? _httpClient.CreateRequest(uri).Credentials(new NetworkCredential(_availableCredentials.UserName, _availableCredentials.Password))
                                              : _httpClient.CreateRequest(uri);
        }

        public bool CanPublish
        {
            get
            {
                EnsureFileListLoaded();
                return _fileList != null && _fileList.CanPublish;
            }
        }


        public PackageFeed Index()
        {
            EnsureFileListLoaded();
            return _fileList;
        }

        public Stream LoadPackage(PackageEntry packageEntry)
        {
            var response = _httpClientGetter(packageEntry.PackageHref.BaseUri(_baseUri))
                    .Get()
                    .Send();

            return response.Entity != null ? response.Entity.Stream : null;
        }

        public void PushPackage(string packageFileName, Stream packageStream)
        {
            if (!CanPublish)
                throw new InvalidOperationException("The repository is read-only.");
            TaskManager.Instance.Run(string.Format("Publishing package '{0}'...", packageFileName),
                                     request =>
                                     {
                                         var response = _httpClientGetter(_fileList.PublishHref)
                                                 .Content(packageStream)
                                                 .Post()
                                                 .Notify(request)
                                                 .Send();
                                         request.Status(response.Status.Code == 201
                                                                ? string.Format("Package created at '{0}'.", response.Headers.Location)
                                                                : string.Format("Unexpected response ({0}) from server.", response.Status.Code));
                                     });
        }


        void EnsureFileListLoaded()
        {
            if (_fileList == null)
            {
                TaskManager.Instance.Run("Loading wrap index file.",
                                         x =>
                                         {
                                             _fileList = _httpClientGetter(_requestUri)
                                                     .Get()
                                                     .Notify(x)
                                                     .Send()
                                                     .Notify(x)
                                                     .AsPackageDocument();
                                         });
            }
        }

        IDisposable ISupportAuthentication.WithCredentials(NetworkCredential credentials)
        {
            _availableCredentials = credentials;
            return new ActionOnDispose(() => _availableCredentials = null);
        }

        public NetworkCredential CurrentCredentials
        {
            get { return _availableCredentials; }
        }
    }
}