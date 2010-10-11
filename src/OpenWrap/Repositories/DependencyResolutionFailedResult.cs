using System.Linq;
using OpenWrap.Commands;

namespace OpenWrap.Repositories
{
    public class DependencyResolutionFailedResult : Error
    {
        public DependencyResolutionResult Result { get; set; }
        readonly string _message;
        DependencyResolutionResult _result;

        public DependencyResolutionFailedResult(DependencyResolutionResult result)
                : this("The following dependencies were not found:", result)
        {
            Result = result;
        }

        public DependencyResolutionFailedResult(string message, DependencyResolutionResult result)
        {
            _message = message;
            _result = result;
        }

        public override string ToString()
        {
            return _message + "\r\n\t"
                   + string.Join("\r\n\t",
                                 _result.Dependencies.Where(x => x.Package == null)
                                         .Select(x => "- '" + x.Dependency.Name + "'")
                                         .ToArray());
        }
    }
}