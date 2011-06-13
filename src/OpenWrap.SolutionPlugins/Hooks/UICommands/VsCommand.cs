using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.CommandBars;
using OpenWrap.VisualStudio.SolutionAddIn;
using OpenWrap.Commands;

namespace OpenWrap.VisualStudio.Hooks
{
    public class VsCommand
    {
        readonly IUICommandDescriptor _commandDescriptor;
        public string CanonicalName { get; private set; }
        string _description;
        string _text;
        ICommand _commandInstance;
        CommandBarControl _uiControl;

        public VsCommand(IUICommandDescriptor commandDescriptor)
        {
            _commandDescriptor = commandDescriptor;
            CanonicalName = "openwrap." + commandDescriptor.Noun.ToLowerInvariant() + "." + commandDescriptor.Verb.ToLowerInvariant();
            _description = commandDescriptor.Description;
            _text = commandDescriptor.Label;

        }
        public VsCommand Register()
        {
            var packageId = new Guid(VsConstants.COMMANDS_PACKAGE_GUID);
            var groupId = CommandGroupGuid = GetGroupId(_commandDescriptor);
            if (CommandGroupGuid == Guid.Empty) return null;
            
            uint pdwCmdId;
            Guid[] emptyGuids = new Guid[0];
            var registered = UICommandsPlugin.VsProfferCommands.AddNamedCommand(ref packageId, ref groupId, CanonicalName, out pdwCmdId, CanonicalName, _text, _description, null, 0u, 0u, 0u, 0, emptyGuids);
            if (Failed(registered)) return null;
            CommandId = pdwCmdId;
            Debug.WriteLine(string.Format("Registered command '{0}'", CanonicalName));
            _commandInstance = _commandDescriptor.Create();
            return this;
        }

        public void Unregister()
        {

        }

        bool Failed(int hr)
        {
            return hr < 0;
        }

        public Guid CommandGroupGuid { get; set; }

        public uint CommandId { get; set; }

        public UICommandState Status
        {
            get { return UICommandState.Enabled; }
        }

        Guid GetGroupId(IUICommandDescriptor commandDescriptor)
        {
            if (commandDescriptor.Context == UICommandContext.Global)
                return new Guid(VsConstants.COMMANDS_GROUP_GLOBAL);
            if (commandDescriptor.Context == UICommandContext.Solution)
                return new Guid(VsConstants.COMMANDS_GROUP_SOLUTION);
            if (commandDescriptor.Context == UICommandContext.OpenWrapProject
                || commandDescriptor.Context == UICommandContext.StandardProject)
                return new Guid(VsConstants.COMMANDS_GROUP_PROJECT);
            return Guid.Empty;
        }

        public IEnumerable<ICommandOutput> Execute()
        {
            return _commandInstance.Execute().ToList();
        }

        public void RegisterUI(CommandBar menu, int index = 1, bool isFirstInGroup = false)
        {
            List<CommandBarControl> toRemove = new List<CommandBarControl>();
            foreach(CommandBarControl control in menu.Controls)
            {
                int commandId;
                string commandGroup;
                UICommandsPlugin.DTE.Commands.CommandInfo(control, out commandGroup, out commandId);
                if (new Guid(commandGroup) == CommandGroupGuid && commandId == CommandId)
                    toRemove.Add(control);
            }
            foreach(var value in toRemove)
                value.Delete();
            var dteCommand = UICommandsPlugin.DTE.Commands.Named(CanonicalName);
            _uiControl = (CommandBarControl)dteCommand.AddControl(menu, index);
            _uiControl.BeginGroup = isFirstInGroup;
        }
        public void UnregisterUI()
        {
            _uiControl.Delete();
            _uiControl = null;
        }
    }
}