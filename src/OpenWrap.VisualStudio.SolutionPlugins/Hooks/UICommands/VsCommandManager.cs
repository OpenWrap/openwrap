using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using OpenWrap.Commands;
using OpenWrap.VisualStudio.SolutionAddIn;
using OLECMDF = OpenWrap.VisualStudio.Interop.OLECMDF;

namespace OpenWrap.VisualStudio.Hooks
{
    public static class VsConstants
    {
        public const string COMMANDS_PACKAGE_GUID = "{3A9801E8-4BAF-415B-A115-1F6D62967D24}";
        public const string COMMANDS_GROUP_GLOBAL = "{C11E43BD-0D8B-4816-AD3E-C626812FFAFE}";
        public const string COMMANDS_GROUP_SOLUTION = "{403A2BCE-C712-4488-AC6F-7009C2952A7A}";
        public const string COMMANDS_GROUP_PROJECT = "{42E3D6FF-722B-4845-A2F8-F6BABA12B338}";
        public static readonly Guid COMMANDS_GROUP_GLOBAL_GUID = new Guid(COMMANDS_GROUP_GLOBAL);
        public static readonly Guid COMMANDS_GROUP_SOLUTION_GUID = new Guid(COMMANDS_GROUP_SOLUTION);
        public static readonly Guid COMMANDS_GROUP_PROJECT_GUID = new Guid(COMMANDS_GROUP_PROJECT);
    }
    public class VsCommandManager : IOleCommandTarget, IDisposable
    {
        Dictionary<VsCommandIdentifier, VsCommand> _commands;
        const int OLECMDERR_E_UNKNOWNGROUP = -2147221244;
        const int OLECMDERR_E_NOTSUPPORTED = -2147221248;
        const int S_OK = 0;

        public VsCommandManager()
        {
            // VS2008 needs that to initialize commands
            if (UICommands.DTE.Version == "9.0")
            {
                var useless = UICommands.DTE.CommandBars;
            }
            MainMenu = GetCommandBar(VsMenus.guidSHLMainMenu, 0);
            ToolsMenu = MainMenu.Popups().Named("Tools");


            _commands = new Dictionary<VsCommandIdentifier, VsCommand>();
        }

        protected CommandBar ToolsMenu { get; set; }

        protected CommandBar MainMenu { get; set; }

        CommandBar GetCommandBar(Guid commandGroup, uint commandId)
        {
            object mainMenuAsObj;
            var result = UICommands.VsProfferCommands.FindCommandBar(IntPtr.Zero, ref commandGroup, commandId, out mainMenuAsObj);
            return result >= 0 ? mainMenuAsObj as CommandBar : null;
        }
        public bool TryAdd(IUICommandDescriptor command)
        {
            var vsCommand = new VsCommand(command).Register();
            if (vsCommand == null) return false;
            this[vsCommand.CommandGroupGuid, vsCommand.CommandId] = vsCommand;

            if (command.Context == UICommandContext.Global)
            {
                vsCommand.RegisterUI(ToolsMenu);
            }

            return true;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {

            if (pguidCmdGroup != VsConstants.COMMANDS_GROUP_GLOBAL_GUID &&
                pguidCmdGroup != VsConstants.COMMANDS_GROUP_SOLUTION_GUID &&
                pguidCmdGroup != VsConstants.COMMANDS_GROUP_PROJECT_GUID)
                return OLECMDERR_E_UNKNOWNGROUP;
            if (cCmds < 1) return S_OK;


            var command = this[pguidCmdGroup, prgCmds[0].cmdID];

            if (command == null) return OLECMDERR_E_NOTSUPPORTED;

            var status = command.Status;
            if (status == UICommandState.Disabled)
                prgCmds[0].cmdf = 0u;
            else
                prgCmds[0].cmdf = (uint)OLECMDF.SUPPORTED;

            if (status == UICommandState.Enabled)
                prgCmds[0].cmdf |= (uint)OLECMDF.ENABLED;
            else if (status == UICommandState.Hidden)
                prgCmds[0].cmdf |= (uint)OLECMDF.INVISIBLE;


            return S_OK;
        }
        public VsCommand this[Guid groupId, uint cmdId]
        {
            get
            {
                var key = new VsCommandIdentifier(groupId, cmdId);
                return _commands.ContainsKey(key)
                               ? _commands[key]
                               : null;
            }
            set { _commands[new VsCommandIdentifier(groupId, cmdId)] = value; }
        }
        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            var command = this[pguidCmdGroup, nCmdID];
            if (command == null)
                return OLECMDERR_E_NOTSUPPORTED;

            try
            {
                var results = command.Execute();
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format("An error occured executing command '{0}'.\r\n{1}",
                                              command.CanonicalName,
                                              e));
            }

            return S_OK;
        }

        public void Dispose()
        {
            foreach (var cmd in _commands.Values)
                cmd.Unregister();
        }
    }
}