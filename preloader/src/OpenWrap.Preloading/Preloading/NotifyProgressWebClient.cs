using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Reflection;
using System.Threading;

namespace OpenWrap.Preloading
{
    public class NotifyProgressWebClient
    {
        readonly Dictionary<Uri, ManualResetEvent> _locks = new Dictionary<Uri, ManualResetEvent>();
        readonly INotifyDownload _notifier;
        readonly string _proxyAddress;
        readonly string _proxyUsername;
        readonly string _proxyPassword;
        readonly IWebProxy _systemProxy = WebRequest.GetSystemWebProxy();
        byte[] _dataReadResult;
        Exception _error;
        string _stringReadResult;

        public NotifyProgressWebClient(INotifyDownload notifier, string proxyAddress, string proxyUsername, string proxyPassword)
        {

            _notifier = notifier;
            _proxyAddress = proxyAddress;
            _proxyUsername = proxyUsername;
            _proxyPassword = proxyPassword;
        }

        WebClient CreateWebClient(Uri uri)
        {
            WebClient newWebClient = new WebClient();
            newWebClient.DownloadFileCompleted += DownloadFileCompleted;
            newWebClient.DownloadStringCompleted += DownloadStringCompleted;
            newWebClient.DownloadDataCompleted += DownloadDataCompleted;
            newWebClient.DownloadProgressChanged += DownloadProgressChanged;

            WebProxy finalProxySettings = null;
            if (_proxyAddress != null)
            {
                finalProxySettings = new WebProxy(_proxyAddress, false);
            }
            else
            {
                var proxyUriForUri = _systemProxy.GetProxy(uri);
                if (!_systemProxy.IsBypassed(uri))
                    finalProxySettings = new WebProxy(proxyUriForUri, false);

            }
            if (finalProxySettings != null)
            {
                Console.Write("Using proxy at " + finalProxySettings.Address.ToString());
                if (_proxyUsername != null)
                {
                    finalProxySettings.Credentials = new NetworkCredential(_proxyUsername, _proxyPassword);
                    Console.WriteLine(" using provided credentials.");
                }
                else
                {
                    finalProxySettings.UseDefaultCredentials = true;
                    Console.WriteLine(" using default credentials.");
                }
                newWebClient.Proxy = finalProxySettings;
            }

            return newWebClient;
        }

        public byte[] DownloadData(Uri uri)
        {
            using (var client = CreateWebClient(uri))
            {
                StartAsync(uri);
                _notifier.DownloadStart(uri);
                client.DownloadDataAsync(uri, uri);
                Wait(uri);
                return _dataReadResult;
            }
        }

        public void DownloadFile(Uri uri, string destinationFile)
        {
            using (var client = CreateWebClient(uri))
            {
                StartAsync(uri);
                _notifier.DownloadStart(uri);

                client.DownloadFileAsync(uri, destinationFile, uri);
                Wait(uri);
            }
        }

        public string DownloadString(Uri uri)
        {

            using (var client = CreateWebClient(uri))
            {
                StartAsync(uri);
                _notifier.DownloadStart(uri);

                client.DownloadStringAsync(uri, uri);
                Wait(uri);
                return _stringReadResult;
            }
        }

        void Completed(AsyncCompletedEventArgs e)
        {
            _notifier.DownloadEnd();
            if (e.Error != null)
                _error = e.Error;
            _locks[(Uri)e.UserState].Set();
        }

        void StartAsync(Uri uri)
        {
            _locks[uri] = new ManualResetEvent(false);
        }

        void Wait(Uri uri)
        {
            _locks[uri].WaitOne();
            if (_error != null)
                throw new WebException("An error occured.", _error);
        }

        void DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                _dataReadResult = e.Result;
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                throw;
            }
            finally
            {
                Completed(e);
            }
        }

        void DownloadFileCompleted(object src, AsyncCompletedEventArgs e)
        {
            Completed(e);
        }

        void DownloadProgressChanged(object src, DownloadProgressChangedEventArgs e)
        {
            _notifier.DownloadProgress(e.ProgressPercentage);
        }

        void DownloadStringCompleted(object src, DownloadStringCompletedEventArgs e)
        {
            try
            {
                _stringReadResult = e.Result;
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                throw;
            }
            finally
            {
                Completed(e);
            }
        }
    }
}