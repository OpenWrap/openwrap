using System;

namespace OpenWrap.VisualStudio.ComShim
{
    public class MSBuild4Engine : IMSBuildEngine
    {
        public IMSBuildProject Load(string fullPath)
        {
            var type = Type.GetType("Microsoft.Build.Evaluation.ProjectCollection, Microsoft.Build, Version=4.0.0.0");
            var collection = type.GetProperty("GlobalProjectCollection").GetValue(null, null);
            return new MSBuild4Project(type.GetMethod("LoadProject", new[] { typeof(string) }).Invoke(collection, new object[] { fullPath }));
        }
    }
}