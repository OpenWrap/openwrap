using System;

namespace OpenWrap.Preloading
{
    public interface INotifyDownload
    {
        void DownloadStart(Uri downloadAddress);
        void DownloadEnd();
        void DownloadProgress(int progressPercentage);
    }
}