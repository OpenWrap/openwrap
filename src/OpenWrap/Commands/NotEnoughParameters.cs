using System;

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