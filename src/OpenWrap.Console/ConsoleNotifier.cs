using System;

namespace OpenWrap.Console
{
    public class ConsoleNotifier : INotifier
    {
        public int BootstrappingFailed(Exception exception)
        {
            System.Console.WriteLine("OpenWrap bootstrapping failed.");
            System.Console.WriteLine(exception.ToString());
            return -100;
        }

        public int RunFailed(Exception e)
        {
            System.Console.WriteLine("OpenWrap could not be started.");
            System.Console.WriteLine(e.Message);
            var oldColor = System.Console.ForegroundColor;
            try
            {
                System.Console.ForegroundColor = ConsoleColor.Gray;
                System.Console.WriteLine(e.ToString());
            }
            finally
            {
                System.Console.ForegroundColor = oldColor;
            }
            return -1;
        }

        public void BootstraperIs(string entrypointFile, Version entrypointVersion)
        {
            System.Console.WriteLine("# OpenWrap v{0} ['{1}']", entrypointVersion, entrypointFile);
        }

        public void Message(string message, params object[] messageParameters)
        {
            System.Console.WriteLine(message, messageParameters);
        }

        int _downloadProgress;
        public void DownloadStart(Uri downloadAddress)
        {
            _downloadProgress = 0;
            System.Console.Write("Downloading {0} [", downloadAddress);           
        }

        public void DownloadEnd()
        {
            _downloadProgress = 0;
            System.Console.WriteLine("]");
        }

        public void DownloadProgress(int progressPercentage)
        {

            var progress = progressPercentage / 10;

            if (_downloadProgress < progress && progress <= 10)
            {
                System.Console.Write(new string('.', progress - _downloadProgress));
                _downloadProgress = progress;
            }
        }

        public InstallAction InstallOptions()
        {
            System.Console.WriteLine("The OpenWrap shell is not installed on this machine. Do you want to:");
            System.Console.WriteLine("(i) install the shell and make it available on the path?");
            System.Console.WriteLine("(c) use the current executable location and make it available on the path?");
            System.Console.WriteLine("(n) do nothing?");
            var key = System.Console.ReadKey();
            System.Console.WriteLine();
            switch (key.KeyChar)
            {
                case 'i':
                case 'I':
                    return InstallAction.InstallToDefaultLocation;
                case 'c':
                case 'C':
                    return InstallAction.UseCurrentExecutableLocation;
            }
            return InstallAction.None;
        }
    }
}