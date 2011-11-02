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

        public AssemblyInfoGenerator(IPackageDescriptor descriptor)
        {
            _descriptor = descriptor;
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
            if (_descriptor.Version != null)
            {
                TryAppend<AssemblyVersionAttribute>(sb,
                                                    "assembly-version",
                                                    new Version(
                                                        _descriptor.Version.Major % ushort.MaxValue,
                                                        _descriptor.Version.Minor % ushort.MaxValue,
                                                        _descriptor.Version.Build % ushort.MaxValue,
                                                        _descriptor.Version.Revision % ushort.MaxValue
                                                        ).ToString());
                TryAppend<AssemblyFileVersionAttribute>(sb,
                                                        "file-version",
                                                        _descriptor.Version.ToString());
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