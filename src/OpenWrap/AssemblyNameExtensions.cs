using System.Reflection;

namespace OpenWrap
{
    public static class AssemblyNameExtensions
    {
        public static bool IsStronglyNamed(this AssemblyName assemblyName)
        {
            return assemblyName.GetPublicKeyToken().Length > 0;
        }
    }
}