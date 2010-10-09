using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;

namespace OpenWrap.Repositories.NuPack
{
    public static class NuPackConverter
    {
        public static bool Convert(IFile nuPackPacakge, IFile openWrapPackage)
        {
            if (nuPackPacakge == null) throw new ArgumentNullException("nuPackPacakge");
            if (openWrapPackage == null) throw new ArgumentNullException("openWrapPackage");

            return true;
        }
    }
}
