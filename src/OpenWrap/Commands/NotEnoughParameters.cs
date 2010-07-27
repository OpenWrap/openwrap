using System;
using OpenWrap.Commands;
using OpenWrap;

namespace OpenWrap.Commands
{
    public class NotEnoughParameters : Error
    {
        public bool Success
        {
            get { return false; }
        }
        public override string ToString()
        {
            return Strings.CMD_NOT_ENOUGH_ARGS;
        }
    }
}