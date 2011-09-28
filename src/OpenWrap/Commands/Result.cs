using System;

namespace OpenWrap.Commands
{
    public class Data : AbstractOutput
    {
        public Data(string str, params object[] parameters) : base(str, parameters)
        {
            this.Type = CommandResultType.Data;
        }

    }
}