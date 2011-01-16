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
        readonly WebClient _webClient = new WebClient();
        readonly IWebProxy _webProxy = HttpWebRequest.GetSystemWebProxy();
        byte[] _dataReadResult;
        Exception _error;
        string _stringReadResult;

        public NotifyProgressWebClient(INotifyDownload notifier, string proxyAddress, string proxyUsername, string proxyPassword)
        {

            _notifier = notifier;
            _webClient.DownloadFileCompleted += DownloadFileCompleted;
            _webClient.DownloadStringCompleted += DownloadStringCompleted;
            _webClient.DownloadDataCompleted += DownloadDataCompleted;
            _webClient.DownloadProgressChanged += DownloadProgressChanged;
            
            if (proxyAddress != null)
            {
                var proxy = new WebProxy(proxyAddress, false);
                if (proxyUsername != null)
                    proxy.Credentials = new NetworkCredential(proxyUsername, proxyPassword);
                else
                    proxy.UseDefaultCredentials = true;
                _webClient.Proxy = proxy;
            }
            else
                _webClient.Proxy = _webProxy;
            
        }

        public byte[] DownloadData(Uri uri)
        {
            StartAsync(uri);
            _notifier.DownloadStart(uri);
            _webClient.DownloadDataAsync(uri, uri);
            Wait(uri);
            return _dataReadResult;
        }

        public void DownloadFile(Uri uri, string destinationFile)
        {
            StartAsync(uri);
            _notifier.DownloadStart(uri);

            _webClient.DownloadFileAsync(uri, destinationFile, uri);
            Wait(uri);
        }

        public string DownloadString(Uri uri)
        {
            StartAsync(uri);
            _notifier.DownloadStart(uri);

            _webClient.DownloadStringAsync(uri, uri);
            Wait(uri);
            return _stringReadResult;
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