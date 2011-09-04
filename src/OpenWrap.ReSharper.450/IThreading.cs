extern alias resharper;
namespace OpenWrap.Resharper
{
    public interface IThreading
    {
        resharper::JetBrains.Threading.JetDispatcher Dispatcher { get; }
        resharper::JetBrains.Threading.ReentrancyGuard ReentrancyGuard { get; }
    }
}