using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace OpenRasta.Client
{
    public static class RequestExtensions
    {
        public static T Get<T>(this T request) where T : IClientRequest
        {
            request.Method = "GET";
            return request;
        }
        public static T Post<T>(this T request) where T : IClientRequest
        {
            request.Method = "POST";
            return request;
        }
        public static T Content<T>(this T request, Stream content) where T : IClientRequest
        {
            request.Entity.Stream = content;
            return request;
        }

        public static T ContentType<T>(this T request, string mediaType) where T : IClientRequest
        {
            request.Entity.ContentType = new MediaType(mediaType);
            return request;
        }
        public static T Progress<T>(this T request, Action<int> progress) where T : IClientRequest
        {
            request.Progress += (s, e) => progress(e.Progress);
            return request;
        }
        public static T Status<T>(this T request, Action<string> status) where T : IClientRequest
        {
            request.StatusChanged += (s, e) => status(e.Message);

            return request;
        }
        public static XmlReader AsXmlReader<T>(this T response) where T : IClientResponse
        {
            if (response == null || response.Entity == null || response.Entity.Stream == null)
                return null;
            return new XmlTextReader(response.RequestUri.ToString(), response.Entity.Stream);
        }
        public static XDocument AsXDocument<T>(this T response) where T : IClientResponse
        {
            var reader = response.AsXmlReader<T>();
            if (reader == null) return null;
            return XDocument.Load(reader, LoadOptions.SetBaseUri);
        }
    }
}