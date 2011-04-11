using EnvDTE80;

namespace OpenWrap.VisualStudio.ComShim
{
    public interface ITargetExecutor
    {
        void Execute(DTE2 dte);
    }
}