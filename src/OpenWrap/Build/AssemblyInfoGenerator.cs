using System;
using System.Linq;
using System.Reflection;
using System.Text;
using OpenFileSystem.IO;
using OpenWrap.IO;
using OpenWrap.PackageModel;

namespace OpenWrap.Build
{
    public class AssemblyInfoGenerator
    {
        const string ATTRIBUTE_TEXT = "[assembly: {0}(\"{1}\")]";

        readonly IPackageDescriptor _descriptor;

        public AssemblyInfoGenerator(IPackageDescriptor descriptor)
        {
            _descriptor = descriptor;
            Version = descriptor.SemanticVersion ?? descriptor.Version.ToSemVer();
        }

        public SemanticVersion Version { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            TryAppend<AssemblyCompanyAttribute>(sb, "author", _descriptor.Authors.JoinString(", "));
            TryAppend<AssemblyProductAttribute>(sb, "title", _descriptor.Title);
            TryAppend<AssemblyCopyrightAttribute>(sb, "copyright", _descriptor.Copyright);
            TryAppend<AssemblyConfigurationAttribute>(sb, "build", _descriptor.BuildConfiguration);
            TryAppend<AssemblyDescriptionAttribute>(sb, "description", _descriptor.Description);
            TryAppend<AssemblyTrademarkAttribute>(sb, "trademark", _descriptor.Trademark);
            if (Version != null)
            {
                int revision;
                if (!int.TryParse(Version.Build, out revision)) revision = -1;
                var clrVersion = new Version(
                    Version.Major % ushort.MaxValue, 
                    Version.Minor % ushort.MaxValue, 
                    Version.Patch == -1 ? 0 : Version.Patch % ushort.MaxValue, 
                    revision == -1 ? 0 : revision % ushort.MaxValue);

                TryAppend<AssemblyVersionAttribute>(sb, 
                                                    "assembly-version", 
                                                    clrVersion.ToString());
                TryAppend<AssemblyInformationalVersionAttribute>(sb, "assembly-info-version", Version.ToString());

                TryAppend<AssemblyFileVersionAttribute>(sb, 
                                                        "file-version", 
                                                        Version.ToVersion().ToString());
            }
            return sb.ToString();
        }

        public void Write(IFile destination)
        {
            if (_descriptor.AssemblyInfo.Any())
                destination.WriteString(ToString());
        }

        void TryAppend<T>(StringBuilder sb, string flagName, string attribValue)
        {
            if (_descriptor.AssemblyInfo.Contains("*") || _descriptor.AssemblyInfo.ContainsNoCase(flagName))
                sb.AppendLine(string.Format(ATTRIBUTE_TEXT, typeof(T).FullName, attribValue));
        }
    }
}