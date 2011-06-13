using System;
using System.Collections.Generic;
using OpenFileSystem.IO;
using OpenWrap.ProjectModel.Drivers.File;

namespace OpenWrap.ProjectModel
{
    public interface ISolution
    {
        IEnumerable<IProject> AllProjects { get; }
        Version Version { get; }
        void Save();
        IProject AddProject(IFile projectFile);
    }
}