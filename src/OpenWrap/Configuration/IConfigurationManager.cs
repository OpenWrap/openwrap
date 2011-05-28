using System;

namespace OpenWrap.Configuration
{
    public interface IConfigurationManager
    {
        T Load<T>(Uri uri = null) where T : new();
        void Save<T>(T configEntry, Uri uri = null);
    }
}