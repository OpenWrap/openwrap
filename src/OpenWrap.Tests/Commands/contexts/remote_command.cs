using System;
using OpenWrap.Commands;

namespace Tests.Commands.contexts
{
    public class remote_command<T> : command<T> where T : ICommand
    {
    }
}