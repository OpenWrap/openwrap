using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.VisualStudio
{
    public static class Constants
    {
        public const string ADD_IN_NAME = "OpenWrap Visual Studio Solution Add-in";
        public const string ADD_IN_DESCRIPTION = "Ensures OpenWrap initialization when a solution is opened.";
        public const string ADD_IN_GUID_2008 = "DAABB8CF-D6C7-4C25-A633-E94B022A1060";
        public const string ADD_IN_GUID_2010 = "FBFF8E62-9532-419E-9F48-6A7943CDD14E";
        public const string ADD_IN_PROGID_2008 = "OpenWrap.VisualStudioIntegration2008";
        public const string ADD_IN_PROGID_2010 = "OpenWrap.VisualStudioIntegration2010";
        public const string COMMANDS_PACKAGE_GUID = "{3A9801E8-4BAF-415B-A115-1F6D62967D24}";
        public const string COMMANDS_GROUP_GLOBAL = "{C11E43BD-0D8B-4816-AD3E-C626812FFAFE}";
        public const string COMMANDS_GROUP_SOLUTION = "{403A2BCE-C712-4488-AC6F-7009C2952A7A}";
        public const string COMMANDS_GROUP_PROJECT = "{42E3D6FF-722B-4845-A2F8-F6BABA12B338}";
        public static readonly Guid COMMANDS_GROUP_GLOBAL_GUID = new Guid(COMMANDS_GROUP_GLOBAL);
        public static readonly Guid COMMANDS_GROUP_SOLUTION_GUID = new Guid(COMMANDS_GROUP_SOLUTION);
        public static readonly Guid COMMANDS_GROUP_PROJECT_GUID = new Guid(COMMANDS_GROUP_PROJECT);
    }
}
