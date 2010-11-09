using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace OpenWrap
{
    //[Guid(ComRegistrations.ADD_IN_GUID)]
    //[ProgId(ComRegistrations.ADD_IN_PROGID)]
    //[ComVisible(true)]
    //public class ComShim : IDTExtensibility2, IDTCommandTarget
    //{
    //    CommandEvents _commandEvents;
    //    Command _command;
    //    CommandBarControl control;
    //    CommandBarEvents commandBarEvents;

    //    public ComShim()
    //    {
    //        Debug.WriteLine("Shim initialized.");
    //        System.Diagnostics.Debugger.Launch();
    //    }

    //    protected AddIn AddIn { get; set; }
    //    protected DTE2 Application { get; set; }

    //    public void Exec(string cmdName, vsCommandExecOption executeOption, ref object variantIn, ref object variantOut, ref bool handled)
    //    {
    //        System.Diagnostics.Debugger.Launch();
            
    //        var eventArgs = new ExecEventArgs(Application, AddIn, cmdName, executeOption, variantIn, variantOut, handled);
    //        AddInRedirector.Instance.RaiseExec(eventArgs);
    //        variantIn = eventArgs.VariantIn;
    //        variantOut = eventArgs.VariantOut;
    //        handled = eventArgs.Handled;
    //    }

    //    public void QueryStatus(string cmdName, vsCommandStatusTextWanted neededText, ref vsCommandStatus statusOption, ref object commandText)
    //    {
    //        MessageBox.Show("QueryStatus");
    //        System.Diagnostics.Debugger.Launch();
    //        var eventArgs = new QueryStatusEventArgs(Application, AddIn, cmdName, neededText, statusOption, commandText);
    //        AddInRedirector.Instance.RaiseQueryStatus(eventArgs);
    //        statusOption = eventArgs.CommandStatus;
    //        commandText = eventArgs.CommandText;
    //    }

    //    public void OnAddInsUpdate(ref Array custom)
    //    {
    //    }

    //    public void OnBeginShutdown(ref Array custom)
    //    {
    //        System.Diagnostics.Debugger.Launch();
    //        AddInRedirector.Instance.RaiseBeginShutdown(new DteEventArgs(Application, AddIn));
    //    }

    //    public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
    //    {
            
    //        System.Diagnostics.Debugger.Launch();
    //        this.Application = (DTE2)application;
    //        this.AddIn = (AddIn)addInInst;
    //        TryAddCommand(Application, new AttributeBasedCommandDescriptor(typeof(ManageWrapCommand)));
    //        AddInRedirector.Instance.RaiseConnection(new ConnectionEventArgs(Application, AddIn, connectMode));

    //    }

    //    public void OnDisconnection(ext_DisconnectMode removeMode, ref Array custom)
    //    {
    //        System.Diagnostics.Debugger.Launch();
    //        AddInRedirector.Instance.RaiseDisconnection(new DisconnectionEventArgs(Application, AddIn, removeMode));
    //    }

    //    public void OnStartupComplete(ref Array custom)
    //    {
    //        System.Diagnostics.Debugger.Launch();
    //        AddInRedirector.Instance.RaiseStartupComplete(new DteEventArgs(Application, AddIn));
    //    }

    //    void TryAddCommand(DTE2 dte, ICommandDescriptor commandDescriptor)
    //    {
    //        _commandEvents = dte.Events.get_CommandEvents("{00000000-0000-0000-0000-000000000000}", 0);
    //        _commandEvents.BeforeExecute += HandleBeforeExecute;

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
    //            _command = commands.AddNamedCommand2(AddIn,
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
    //            if ((_command != null) && (toolsPopup != null))
    //            {
    //                control = (CommandBarControl)_command.AddControl(toolsPopup.CommandBar, 1);
                    
    //                commandBarEvents = (CommandBarEvents)dte.Events.get_CommandBarEvents(control);
    //                commandBarEvents.Click += HandleClick;
    //            }
    //        }
    //        catch (ArgumentException)
    //        {
    //            //If we are here, then the exception is probably because a command with that name
    //            //  already exists. If so there is no need to recreate the command and we can 
    //            //  safely ignore the exception.
    //        }
    //    }

    //    void HandleClick(object commandbarcontrol, ref bool handled, ref bool canceldefault)
    //    {
            
    //    }

    //    void HandleBeforeExecute(string guid, int id, object customin, object customout, ref bool canceldefault)
    //    {
            
    //    }
    //}

    //public class QueryStatusEventArgs : DteEventArgs
    //{
    //    public QueryStatusEventArgs(DTE2 application, AddIn @in, string commandName, vsCommandStatusTextWanted textWanted, vsCommandStatus commandStatus, object commandText) : base(application, @in)
    //    {
    //        CommandName = commandName;
    //        TextWanted = textWanted;
    //        CommandStatus = commandStatus;
    //        CommandText = commandText;
    //    }

    //    public string CommandName { get; private set; }
    //    public vsCommandStatus CommandStatus { get; set; }
    //    public object CommandText { get; set; }
    //    public vsCommandStatusTextWanted TextWanted { get; private set; }
    //}

    //public class AddInRedirector
    //{
    //    static AddInRedirector _instance;
    //    public event EventHandler<DteEventArgs> BeginShutdown = (s, e) => { };
    //    public event EventHandler<ConnectionEventArgs> Connection = (s, e) => { };
    //    public event EventHandler<DisconnectionEventArgs> Disconnection = (s, e) => { };
    //    public event EventHandler<ExecEventArgs> Exec = (s, e) => { };
    //    public event EventHandler<QueryStatusEventArgs> QueryStatus = (s, e) => { };
    //    public event EventHandler<DteEventArgs> StartupComplete = (s, e) => { };

    //    public static AddInRedirector Instance
    //    {
    //        get
    //        {
    //            if (_instance == null)
    //                lock (typeof(AddInRedirector))
    //                    if (_instance == null)
    //                    {
    //                        _instance = new AddInRedirector();
    //                        _instance.Initialize();
    //                        return _instance;
    //                    }
    //            return _instance;
    //        }
    //    }

    //    public void Initialize()
    //    {
            
    //        Debug.WriteLine("Initializing redirector");
    //    }

    //    internal void RaiseBeginShutdown(DteEventArgs args)
    //    {
    //        BeginShutdown(this, args);
    //    }

    //    internal void RaiseQueryStatus(QueryStatusEventArgs args)
    //    {
    //        QueryStatus(this, args);
    //    }

    //    internal void RaiseStartupComplete(DteEventArgs application)
    //    {
    //        StartupComplete(this, application);
    //    }

    //    internal void RaiseConnection(ConnectionEventArgs e)
    //    {
    //        Connection(this, e);
    //    }

    //    internal void RaiseDisconnection(DisconnectionEventArgs application)
    //    {
    //        Disconnection(this, application);
    //    }

    //    internal void RaiseExec(ExecEventArgs args)
    //    {
    //        Exec(this, args);
    //    }
    //}

    //public class ExecEventArgs : DteEventArgs
    //{

    //    public ExecEventArgs(DTE2 application, AddIn addIn, string cmdName, vsCommandExecOption executeOption, object variantIn, object variantOut, bool handled) : base(application,addIn)
    //    {
    //        CommandName = cmdName;
    //        ExecuteOption = executeOption;
    //        VariantIn = variantIn;
    //        VariantOut = variantOut;
    //        Handled = handled;
    //    }

    //    public bool Handled { get; set; }

    //    public string CommandName { get; private set; }
    //    public vsCommandExecOption ExecuteOption { get; private set; }
    //    public object VariantIn { get; set; }
    //    public object VariantOut { get; set; }
    //}

    //public class DisconnectionEventArgs : DteEventArgs
    //{
    //    public DisconnectionEventArgs(DTE2 application, AddIn addIn, ext_DisconnectMode mode) : base(application, addIn)
    //    {
    //        DisconnectMode = mode;
    //    }

    //    public ext_DisconnectMode DisconnectMode { get; private set; }
    //}

    //public class DteEventArgs : EventArgs
    //{
    //    public DteEventArgs(DTE2 application, AddIn addIn)
    //    {
    //        AddIn = addIn;
    //        Application = application;
    //    }

    //    public AddIn AddIn { get; private set; }
    //    public DTE2 Application { get; private set; }
    //}

    //public class ConnectionEventArgs : DteEventArgs
    //{
    //    public ConnectionEventArgs(DTE2 application, AddIn @in, ext_ConnectMode mode) : base(application, @in)
    //    {
    //        ConnectMode = mode;
    //    }

    //    public ext_ConnectMode ConnectMode { get; set; }
    //}
}