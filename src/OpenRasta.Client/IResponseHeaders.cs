using System;
using System.Collections.Generic;

namespace OpenRasta.Client
{
    public interface IResponseHeaders : IDictionary<string, string>
    {
        Uri Location { get; }
    }
}