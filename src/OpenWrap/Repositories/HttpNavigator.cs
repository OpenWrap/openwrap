using System;
using System.IO;
using System.Net;
using System.Xml.Linq;

namespace OpenWrap.Repositories
{
    public class HttpNavigator : IHttpNavigator
    {
        readonly Uri _serverUri;
        WebRequest _request;
        Uri _requestUri;
        XDocument _fileList;

        public HttpNavigator(Uri serverUri)
        {
            _serverUri = serverUri;

            _requestUri = new Uri(serverUri, new Uri("index.wraplist", UriKind.Relative));

        }

        public XDocument LoadFileList()
        {
            if (_fileList == null)
                _fileList = XDocument.Load(_requestUri.ToString(), LoadOptions.SetBaseUri);
            return _fileList;
        }

        public Stream LoadFile(Uri href)
        {
            return WebRequest.Create(href).GetResponse().GetResponseStream();
        }
    }
}