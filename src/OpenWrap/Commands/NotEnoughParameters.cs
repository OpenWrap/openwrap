using System;
using OpenWrap.Commands;
using OpenWrap;

namespace OpenWrap.Commands
{
    public class NotEnoughParameters : Error
    {
        public override string ToString()
        {
            return Strings.CMD_NOT_ENOUGH_ARGS;
        }
    }
}