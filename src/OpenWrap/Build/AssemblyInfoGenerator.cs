using System;
using System.Linq;
using System.Reflection;
using System.Text;
using OpenFileSystem.IO;
using OpenWrap.IO;
using OpenWrap.PackageModel;
using OpenWrap.ProjectModel;

namespace OpenWrap.Build
{
    public class AssemblyInfoGenerator
    {
        const string ATTRIBUTE_TEXT = "[assembly: {0}(\"{1}\")]";
        
        readonly IPackageDescriptor _descriptor;
        public Version Version { get; set; }

        public AssemblyInfoGenerator(IPackageDescriptor descriptor)
        {
            _descriptor = descriptor;
            Version = descriptor.Version;
        }
        public void Write(IFile destination)
        {
            if (_descriptor.AssemblyInfo.Any())
                destination.WriteString(ToString());
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            TryAppend<AssemblyCompanyAttribute>(sb, "author", _descriptor.Authors.JoinString(", "));
            TryAppend<AssemblyProductAttribute>(sb, "title", _descriptor.Title);
            TryAppend<AssemblyCopyrightAttribute>(sb, "copyright", _descriptor.Copyright);
            if (Version != null)
            {
                TryAppend<AssemblyVersionAttribute>(sb,
                                                    "assembly-version",
                                                    new Version(
                                                        Version.Major % ushort.MaxValue,
                                                        Version.Minor % ushort.MaxValue,
                                                        Version.Build == -1 ? 0 : Version.Build % ushort.MaxValue,
                                                        Version.Revision == -1 ? 0 : Version.Revision % ushort.MaxValue
                                                        ).ToString());
                TryAppend<AssemblyFileVersionAttribute>(sb,
                                                        "file-version",
                                                        Version.ToString());
            }
            return sb.ToString();
        }

        void TryAppend<T>(StringBuilder sb, string flagName, string attribValue)
        {
            if (_descriptor.AssemblyInfo.ContainsNoCase(flagName))
                sb.AppendLine(string.Format(ATTRIBUTE_TEXT, typeof(T).FullName, attribValue));
        }
    }
}