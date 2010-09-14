using System;
using System.IO;
using System.Net;
using System.Xml;
using OpenRasta.Client;
using OpenWrap.Tasks;

namespace OpenWrap.Repositories.Http
{
    public class HttpRepositoryNavigator : IHttpRepositoryNavigator
    {
        readonly Uri _baseUri;
        readonly Uri _requestUri;
        PackageDocument _fileList;
        readonly IHttpClient _httpClient = new HttpWebRequestBasedClient();

        public HttpRepositoryNavigator(Uri baseUri)
        {
            _baseUri = baseUri;
            _requestUri = new Uri(baseUri, new Uri("index.wraplist", UriKind.Relative));
        }


        public PackageDocument Index()
        {
            EnsureFileListLoaded();
            return _fileList;
        }

        public Stream LoadPackage(PackageItem packageItem)
        {
            var response = _httpClient.CreateRequest(packageItem.PackageHref.BaseUri(_baseUri))
                    .Get()
                    .Send();

            return response.Entity != null ? response.Entity.Stream : null;
        }

        public bool CanPublish
        {
            get
            {
                if (_fileList == null)
                {
                    return false;
                }
                return _fileList.CanPublish;
            }
        }

        public void PushPackage(string packageFileName, Stream packageStream)
        {
            if (!CanPublish)
                throw new InvalidOperationException("The repository is read-only.");
            TaskManager.Instance.Run(string.Format("Publishing package '{0}'...", packageFileName), request =>
            {
                var response = _httpClient.CreateRequest(_fileList.PublishHref)
                        .Content(packageStream)
                        .Post()
                        .Notify(request)
                        .Send();
                request.Status(response.StatusCode == 201
                                       ? string.Format("Package created at '{0}'.", response.Headers.Location)
                                       : string.Format("Unexpected response ({0}) from server.", response.StatusCode));
            });
        }


        void EnsureFileListLoaded()
        {
            if (_fileList == null)
            {
                TaskManager.Instance.Run("Loading wrap index file.",
                                         x =>
                                         {
                                             _fileList = _httpClient.CreateRequest(_requestUri)
                                                     .Get()
                                                     .Notify(x)
                                                     .Send()
                                                     .Notify(x)
                                                     .AsPackageDocument();
                                         });
            }
        }
    }
}