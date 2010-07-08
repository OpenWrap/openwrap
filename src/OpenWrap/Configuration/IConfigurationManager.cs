using System;

namespace OpenWrap.Configuration
{
    public interface IConfigurationManager
    {
        T Load<T>(Uri uri) where T:new();
        void Save<T>(Uri uri, T configEntry);
    }
}
