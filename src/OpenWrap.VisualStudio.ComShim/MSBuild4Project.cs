using System;
using System.Collections.Generic;

namespace OpenWrap.VisualStudio.ComShim
{
    public class MSBuild4Project : IMSBuildProject
    {
        readonly object _instance;

        public MSBuild4Project(object instance)
        {
            _instance = instance;
        }
        public void RunTarget(string target)
        {
            Type projectType = _instance.GetType(); // IDictionary<string,ProjectTargetInstance>
            var dictionaryInterface = projectType.GetInterface("System.Collections.Generic.IDictionary`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[Microsoft.Build.Execution.ProjectTargetInstance, Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            var targets = projectType.GetProperty("Targets").GetValue(_instance, null);

            var targetNames = (ICollection<string>)dictionaryInterface.GetProperty("Values").GetValue(targets, null);
            if (targetNames.Contains(target))
                projectType.GetMethod("Build", new[] { typeof(string) }).Invoke(_instance, new object[] { target });
        }
    }
}