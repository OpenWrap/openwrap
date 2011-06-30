using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using OpenFileSystem.IO;
using OpenWrap.IO;

namespace Tests.ProjectModel.drivers.file
{
    public class ClassLibraryProject
    {
        const string NS_MSBUILD = "http://schemas.microsoft.com/developer/msbuild/2003";
        public string Namespace { get; set; }
        public string AssemblyName { get; set; }
        string _content;
        List<KeyValuePair<string, string>> _compile = new List<KeyValuePair<string, string>>();


        public ClassLibraryProject(string projectName, string @namespace = "OpenWrap.Project", string assemblyName = "OpenWrap.Project", string outputPath = "bin")
        {
            Name = projectName;
            Namespace = @namespace;
            AssemblyName = assemblyName;
            ProjectGuid = Guid.NewGuid();
            _content = string.Format(Files.project_classlib, ProjectGuid, @namespace, assemblyName, outputPath);
        }

        public string Name { get; set; }
        public void AddCompile(string fileName, string content)
        {
            _compile.Add(new KeyValuePair<string, string>(fileName, content));

        }
        public void Write(IFile file)
        {
            if (_compile.Count > 0)
            {
                var doc = XDocument.Parse(_content);
                doc.Root.Add(new XElement(XName.Get("ItemGroup", NS_MSBUILD), 
                            from compile in _compile
                            let compileFile = CreateFile(file.Parent, compile)
                            let relativePath = compileFile.Path.MakeRelative(file.Parent.Path)
                            select new XElement(XName.Get("Compile", NS_MSBUILD), new XAttribute("Include", relativePath))
                            ));
                _content = doc.ToString();
            }
            file.WriteString(_content);
        }

        IFile CreateFile(IDirectory dir, KeyValuePair<string,string> compile)
        {
            var file = dir.GetFile(compile.Key);
            file.WriteString(compile.Value);
            return file;
        }

        protected Guid ProjectGuid { get; set; }
    }
}