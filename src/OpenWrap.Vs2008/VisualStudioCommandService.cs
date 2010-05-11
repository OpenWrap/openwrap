using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Resources;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;
using OpenRasta.Wrap.Build.Services;
using OpenRasta.Wrap.Commands;
using OpenRasta.Wrap.Commands.Wrap;

namespace OpenWrap.Vs2008
{
    //public class VisualStudioCommandService : IService
    //{
    //    CommandEvents _commandEvents;

    //    public void Initialize()
    //    {

    //        System.Diagnostics.Debugger.Launch();



    //        AddInRedirector.Instance.Connection += HandleConnection;
    //        AddInRedirector.Instance.Disconnection += HandleDisconnection;
    //        AddInRedirector.Instance.BeginShutdown += HandleBeginShutdown;
    //        AddInRedirector.Instance.QueryStatus += HandleQueryStatus;
    //        AddInRedirector.Instance.Exec += HandleExec;

    //        HookupSolutionAddIn();

    //    }

    //    void HookupSolutionAddIn()
    //    {
    //        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    //        var dte = (DTE2)Global<DTE>() ?? (DTE2)AppDomain.CurrentDomain.GetData("DTE");
    //        var addin = dte.Solution.AddIns.AsEnumerable().FirstOrDefault(x => x.ProgID == ComRegistrations.ADD_IN_PROGID);
    //        if (addin == null)
    //        {
    //            AddInInstaller.RegisterAddInInUserHive();
    //            addin = dte.Solution.AddIns.Add(ComRegistrations.ADD_IN_PROGID, "OpenWrap's Visual Studio Integration", "OpenWrap Visual Studio COM Shim", true);
                

    //        }
    //    }


    //    Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    //    {
    //        if (args.Name.StartsWith("OpenRasta.ComShim"))
    //            return typeof(ComShim).Assembly;
    //        return null;
    //    }
    //    void HandleConnection(object sender, ConnectionEventArgs e)
    //    {
    //        Application = e.Application;
    //        AddIn = e.AddIn;

    //        //AddCommand(new AttributeBasedCommandDescriptor(typeof(ManageWrapCommand)));
    //    }

    //    void AddCommand(ICommandDescriptor commandDescriptor)
    //    {
    //        TryAddCommand((DTE2)Application, commandDescriptor);
    //    }

    //    void TryAddCommand(DTE2 dte, ICommandDescriptor commandDescriptor)
    //    {
    //        _commandEvents = dte.Events.get_CommandEvents("{00000000-0000-0000-0000-000000000000}", 0);
            
    //        _commandEvents.BeforeExecute += new _dispCommandEvents_BeforeExecuteEventHandler(commandEvents_BeforeExecute);


    //        var contextGUIDS = new object[] { };
    //        Commands2 commands = (Commands2)dte.Commands;

    //        string toolsMenuName;

    //        try
    //        {
    //            //If you would like to move the command to a different menu, change the word "Tools" to the 
    //            //  English version of the menu. This code will take the culture, append on the name of the menu
    //            //  then add the command to that menu. You can find a list of all the top-level menus in the file
    //            //  CommandBar.resx.
    //            ResourceManager resourceManager = new ResourceManager("TestAddIn.CommandBar", Assembly.GetExecutingAssembly());
    //            CultureInfo cultureInfo = new CultureInfo(dte.LocaleID);
    //            string resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, "Tools");
    //            toolsMenuName = resourceManager.GetString(resourceName);
    //        }
    //        catch
    //        {
    //            //We tried to find a localized version of the word Tools, but one was not found.
    //            //  Default to the en-US word, which may work for the current culture.
    //            toolsMenuName = "Tools";
    //        }

    //        //Place the command on the tools menu.
    //        //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
    //        CommandBar menuBarCommandBar =
    //            ((CommandBars)dte.CommandBars)["MenuBar"];

    //        //Find the Tools command bar on the MenuBar command bar:
    //        CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
    //        var toolsPopup = (CommandBarPopup)toolsControl;

    //        //This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
    //        //  just make sure you also update the QueryStatus/Exec method to include the new command names.
    //        try
    //        {
    //            //Add a command to the Commands collection:
    //            Command command = commands.AddNamedCommand2(AddIn,
    //                                                        string.Format("{0}{1}", commandDescriptor.Namespace, commandDescriptor.Name),
    //                                                        commandDescriptor.DisplayName,
    //                                                        "No description",
    //                                                        true,
    //                                                        59,
    //                                                        ref contextGUIDS,
    //                                                        (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled,
    //                                                        (int)vsCommandStyle.vsCommandStylePictAndText,
    //                                                        vsCommandControlType.vsCommandControlTypeButton);

    //            //Add a control for the command to the tools menu:
    //            if ((command != null) && (toolsPopup != null))
    //            {
    //                var control = command.AddControl(toolsPopup.CommandBar, 1);
    //                var events = dte.Events.get_CommandBarEvents(control) as CommandBarEvents;
    //                events.Click += HandleButtonClick;
    //            }
    //        }
    //        catch (ArgumentException)
    //        {
    //            //If we are here, then the exception is probably because a command with that name
    //            //  already exists. If so there is no need to recreate the command and we can 
    //            //  safely ignore the exception.
    //        }
    //    }

    //    void commandEvents_BeforeExecute(string guid, int id, object customin, object customout, ref bool canceldefault)
    //    {
            

    //    }

    //    void HandleButtonClick(object commandbarcontrol, ref bool handled, ref bool canceldefault)
    //    {
            
    //    }

    //    void HandleExec(object sender, ExecEventArgs e)
    //    {
            
    //    }

    //    void HandleQueryStatus(object sender, QueryStatusEventArgs e)
    //    {
            
    //    }

    //    void HandleBeginShutdown(object sender, DteEventArgs e)
    //    {
            

    //    }


    //    void HandleDisconnection(object sender, DisconnectionEventArgs e)
    //    {
            
    //    }

    //    protected AddIn AddIn { get; set; }

    //    protected DTE2 Application { get; set; }

    //    TInterface Global<T, TInterface>()
    //        where T : class
    //        where TInterface : class
    //    {
    //        return (TInterface)Package.GetGlobalService(typeof(T));
    //    }

    //    T Global<T>() where T : class
    //    {
    //        return Global<T, T>();
    //    }
    //}


}
