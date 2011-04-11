using System;
using System.ComponentModel.Design;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;
using Process = System.Diagnostics.Process;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace OpenWrap.ComShim
{
    /// <summary>
    /// This class encapsulates an IVsWindowFrame instance and build functionality around.
    /// </summary>
    // ================================================================================================
    public class WindowFrame :
        // --- Our class behaves like a native Visual Studio window frame.
      IVsWindowFrame,
        // --- Our window frame handles events.
      IVsWindowFrameNotify3
    {
        #region Private fields

        private readonly IVsWindowFrame _Frame;

        #endregion

        #region Lifecycle methods

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates a new instance of the class 
        /// </summary>
        /// <param name="frame"></param>
        // --------------------------------------------------------------------------------------------
        public WindowFrame(IVsWindowFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException("frame");
            }
            _Frame = frame;

            // --- Set up event handlers
            ErrorHandler.ThrowOnFailure(_Frame.SetProperty((int)__VSFPROPID.VSFPROPID_ViewHelper, this));
        }

        #endregion

        #region Public properties

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the caption of the window frame.
        /// </summary>
        /// <value>
        /// Caption of the window frame.
        /// </value>
        // --------------------------------------------------------------------------------------------
        public string Caption
        {
            get
            {
                object result;
                ErrorHandler.ThrowOnFailure(_Frame.GetProperty(
                                              (int)__VSFPROPID.VSFPROPID_Caption, out result));
                return result.ToString();
            }
            set
            {
                ErrorHandler.ThrowOnFailure(_Frame.SetProperty(
                                              (int)__VSFPROPID.VSFPROPID_Caption, value));
            }
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the bitmap resource id of the window frame.
        /// </summary>
        /// <value>
        /// Bitmap resource id of the window frame.
        /// </value>
        // --------------------------------------------------------------------------------------------
        public int BitmapResourceID
        {
            get
            {
                object result;
                ErrorHandler.ThrowOnFailure(_Frame.GetProperty(
                                              (int)__VSFPROPID.VSFPROPID_BitmapResource, out result));
                return (int)result;
            }
            set
            {
                ErrorHandler.ThrowOnFailure(_Frame.SetProperty(
                                              (int)__VSFPROPID.VSFPROPID_BitmapResource, value));
            }
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the bitmap index of the window frame.
        /// </summary>
        /// <value>
        /// Bitmap index of the window frame.
        /// </value>
        // --------------------------------------------------------------------------------------------
        public int BitmapIndex
        {
            get
            {
                object result;
                ErrorHandler.ThrowOnFailure(_Frame.GetProperty(
                                              (int)__VSFPROPID.VSFPROPID_BitmapIndex, out result));
                return (int)result;
            }
            set
            {
                ErrorHandler.ThrowOnFailure(_Frame.SetProperty(
                                              (int)__VSFPROPID.VSFPROPID_BitmapIndex, value));
            }
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the GUID of this window frame.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        public Guid Guid
        {
            get
            {
                Guid guid;
                _Frame.GetGuidProperty((int)__VSFPROPID.VSFPROPID_GuidPersistenceSlot, out guid);
                return guid;
            }
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the OleServiceProvider of this window frame.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        public Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider
        {
            get
            {
                object result;
                ErrorHandler.ThrowOnFailure(_Frame.GetProperty(
                                              (int)__VSFPROPID.VSFPROPID_SPFrame, out result));
                return result as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
            }
        }

        #endregion

        #region Public Events

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Event raised when the show state of the window frame changes.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        public event EventHandler<WindowFrameShowEventArgs> OnShow;

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Event raised when the window frame is being closed.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        public event EventHandler<WindowFrameCloseEventArgs> OnClose;

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Event raised when the window frame is being resized.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        public event EventHandler<WindowFramePositionChangedEventArgs> OnResize;

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Event raised when the window frame is being moved.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        public event EventHandler<WindowFramePositionChangedEventArgs> OnMove;

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Event raised when the window frame's dock state is being changed.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        public event EventHandler<WindowFrameDockChangedEventArgs> OnDockChange;

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Event raised when the window frame's state is being changed.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        public event EventHandler OnStatusChange;

        #endregion

        #region Public methods

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Renders this window visible, brings the window to the top, and activates the window.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        public void Show()
        {
            ErrorHandler.ThrowOnFailure(((IVsWindowFrame)this).Show());
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Hides a window.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        public void Hide()
        {
            ErrorHandler.ThrowOnFailure(((IVsWindowFrame)this).Hide());
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Determines whether or not the window is visible.
        /// </summary>
        /// <returns>
        /// Returns S_OK if the window is visible, otherwise returns S_FALSE.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        public bool IsVisible()
        {
            int hresult = ((IVsWindowFrame)this).Hide();
            if (hresult == VSConstants.S_OK) return true;
            if (hresult == VSConstants.S_FALSE) return false;
            ErrorHandler.ThrowOnFailure(hresult);
            return false;
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Shows or makes a window visible and brings it to the top, but does not make it the 
        /// active window.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        public void ShowNoActivate()
        {
            ErrorHandler.ThrowOnFailure(((IVsWindowFrame)this).ShowNoActivate());
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Closes a window.
        /// </summary>
        /// <param name="option">Save options</param>
        // --------------------------------------------------------------------------------------------
        public void CloseFrame(FrameCloseOption option)
        {
            ErrorHandler.ThrowOnFailure(((IVsWindowFrame)this).CloseFrame((uint)option));
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the size of the window frame
        /// </summary>
        /// <param name="size">Size of the frame.</param>
        // --------------------------------------------------------------------------------------------
        public void SetFrameSize(Size size)
        {
            Guid guid = Guid.Empty;
            ErrorHandler.ThrowOnFailure(((IVsWindowFrame)this).
                                          SetFramePos(VSSETFRAMEPOS.SFP_fSize, ref guid, 0, 0, size.Width, size.Height));
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the position of the window frame
        /// </summary>
        /// <param name="rec">Rectangle defining the size of the frame.</param>
        // --------------------------------------------------------------------------------------------
        public void SetWindowPosition(Rectangle rec)
        {
            Guid guid = Guid.Empty;
            ErrorHandler.ThrowOnFailure(((IVsWindowFrame)this).
                                          SetFramePos(VSSETFRAMEPOS.SFP_fMove, ref guid,
                                                      rec.Left, rec.Top, rec.Width, rec.Height));
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the position of the window frame.
        /// </summary>
        /// <param name="position">Window frame coordinates.</param>
        /// <returns>
        /// General position of the frame (docked, tabbed, floating, etc.)
        /// </returns>
        // --------------------------------------------------------------------------------------------
        public FramePosition GetWindowPosition(out Rectangle position)
        {
            int left;
            int top;
            int width;
            int height;
            var pdwSFP = new VSSETFRAMEPOS[1];
            Guid guid;
            ErrorHandler.ThrowOnFailure(((IVsWindowFrame)this).
                                          GetFramePos(pdwSFP, out guid,
                                                      out left, out top, out width, out height));
            position = new Rectangle(left, top, width, height);
            switch (pdwSFP[0])
            {
                case VSSETFRAMEPOS.SFP_fDock:
                    return FramePosition.Docked;
                case VSSETFRAMEPOS.SFP_fTab:
                    return FramePosition.Tabbed;
                case VSSETFRAMEPOS.SFP_fFloat:
                    return FramePosition.Float;
                case VSSETFRAMEPOS.SFP_fMdiChild:
                    return FramePosition.MdiChild;
            }
            return FramePosition.Unknown;
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks if the window frame is on the screen.
        /// </summary>
        /// <returns>True, if the frame is on the screen; otherwise, false.</returns>
        // --------------------------------------------------------------------------------------------
        public bool IsOnScreen()
        {
            int onScreen;
            ErrorHandler.ThrowOnFailure(((IVsWindowFrame)this).IsOnScreen(out onScreen));
            return onScreen != 0;
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Obtains all tool window frames.
        /// </summary>
        /// <value>All available tool window frames.</value>
        // --------------------------------------------------------------------------------------------
        public static IEnumerable<WindowFrame> ToolWindowFrames
        {
            get
            {
                var uiShell = SiteManager.GetGlobalService<SVsUIShell, IVsUIShell>();
                IEnumWindowFrames windowEnumerator;
                ErrorHandler.ThrowOnFailure(uiShell.GetToolWindowEnum(out windowEnumerator));
                var frame = new IVsWindowFrame[1];
                int hr = VSConstants.S_OK;
                while (hr == VSConstants.S_OK)
                {
                    uint fetched;
                    hr = windowEnumerator.Next(1, frame, out fetched);
                    ErrorHandler.ThrowOnFailure(hr);
                    if (fetched == 1)
                    {
                        yield return new WindowFrame(frame[0]);
                    }
                }
            }
        }

        #endregion

        #region Implementation of IVsWindowFrame

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Renders this window visible, brings the window to the top, and activates the window.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrame.Show()
        {
            return _Frame.Show();
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Hides a window.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrame.Hide()
        {
            return _Frame.Hide();
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Determines whether or not the window is visible.
        /// </summary>
        /// <returns>
        /// Returns S_OK if the window is visible, otherwise returns S_FALSE.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrame.IsVisible()
        {
            return _Frame.IsVisible();
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Shows or makes a window visible and brings it to the top, but does not make it the 
        /// active window.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrame.ShowNoActivate()
        {
            return _Frame.ShowNoActivate();
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Closes a window.
        /// </summary>
        /// <param name="grfSaveOptions">
        /// Save options whose values are taken from the __FRAMECLOSE.
        /// </param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrame.CloseFrame(uint grfSaveOptions)
        {
            return _Frame.CloseFrame(grfSaveOptions);
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the position of the window.
        /// </summary>
        /// <param name="dwSFP">
        /// Frame position whose values are taken from the VSSETFRAMEPOS enumeration.
        /// </param>
        /// <param name="rguidRelativeTo">Not used.</param>
        /// <param name="x">Absolute x ordinate.</param>
        /// <param name="y">Absolute y ordinate.</param>
        /// <param name="cx">x ordinate relative to x.</param>
        /// <param name="cy">y ordinate relative to y.</param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrame.SetFramePos(VSSETFRAMEPOS dwSFP, ref Guid rguidRelativeTo, int x, int y, int cx, int cy)
        {
            return _Frame.SetFramePos(dwSFP, ref rguidRelativeTo, x, y, cx, cy);
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the position of the window.
        /// </summary>
        /// <param name="pdwSFP">Pointer to the frame position.</param>
        /// <param name="pguidRelativeTo">Not used.</param>
        /// <param name="px">Pointer to the absolute x ordinate.</param>
        /// <param name="py">Pointer to the absolute y ordinate.</param>
        /// <param name="pcx">Pointer to the x ordinate relative to px.</param>
        /// <param name="pcy">Pointer to the y ordinate relative to py.</param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrame.GetFramePos(VSSETFRAMEPOS[] pdwSFP, out Guid pguidRelativeTo, out int px,
                                       out int py, out int pcx, out int pcy)
        {
            return _Frame.GetFramePos(pdwSFP, out pguidRelativeTo, out px, out py, out pcx, out pcy);
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a window property.
        /// </summary>
        /// <param name="propid">
        /// Identifier of the property whose values are taken from the __VSFPROPID enumeration.
        /// </param>
        /// <param name="pvar">Pointer to a VARIANT.</param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrame.GetProperty(int propid, out object pvar)
        {
            return _Frame.GetProperty(propid, out pvar);
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets a window frame property.
        /// </summary>
        /// <param name="propid">
        /// Identifier of the property whose values are taken from the __VSFPROPID enumeration.
        /// </param>
        /// <param name="var">The value depends on the property set (see __VSFPROPID ).</param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrame.SetProperty(int propid, object var)
        {
            return _Frame.SetProperty(propid, var);
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a window frame property based on a supplied GUID.
        /// </summary>
        /// <param name="propid">
        /// Identifier of the property whose values are taken from the __VSFPROPID enumeration.
        /// </param>
        /// <param name="pguid">Pointer to the unique identifier of the property.</param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrame.GetGuidProperty(int propid, out Guid pguid)
        {
            return _Frame.GetGuidProperty(propid, out pguid);
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets a window frame property based on a supplied GUID.
        /// </summary>
        /// <param name="propid">
        /// Identifier of the property whose values are taken from the __VSFPROPID enumeration.
        /// </param>
        /// <param name="rguid">Unique identifier of the property to set.</param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrame.SetGuidProperty(int propid, ref Guid rguid)
        {
            return _Frame.SetGuidProperty(propid, ref rguid);
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Provides IVsWindowFrame with a view helper (VSFPROPID_ViewHelper) inserted into its list 
        /// of event notifications.
        /// </summary>
        /// <param name="riid">Identifier of the window frame being requested.</param>
        /// <param name="ppv">
        /// Address of pointer variable that receives the window frame pointer requested in riid.
        /// </param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrame.QueryViewInterface(ref Guid riid, out IntPtr ppv)
        {
            return _Frame.QueryViewInterface(ref riid, out ppv);
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns true if the window frame is on the screen.
        /// </summary>
        /// <param name="pfOnScreen">true if the window frame is visible on the screen.</param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        /// <remarks>
        /// IVsWindowFrame.IsOnScreen checks to see if a window hosted by the Visual Studio IDE has 
        /// been autohidden, or if the window is part of a tabbed display and currently obscured by 
        /// another tab. IsOnScreen also checks to see whether the instance of the Visual Studio IDE 
        /// is minimized or obscured. IsOnScreen differs from the behavior of IsWindowVisible a 
        /// method that may return true even if the window is completely obscured or minimized. 
        /// IsOnScreen also differs from IsVisible which does not check to see if the Visual Studio 
        /// IDE has autohidden the window, or if the window is tabbed and currently obscured by 
        /// another window.
        /// </remarks>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrame.IsOnScreen(out int pfOnScreen)
        {
            return _Frame.IsOnScreen(out pfOnScreen);
        }

        #endregion

        #region Implementation of IVsWindowFrameNotify3

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Notifies the VSPackage of a change in the window's display state.
        /// </summary>
        /// <param name="fShow">
        /// Specifies the reason for the display state change. Value taken from the __FRAMESHOW.
        /// </param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrameNotify3.OnShow(int fShow)
        {
            if (OnShow != null)
            {
                var e = new WindowFrameShowEventArgs((FrameShow)fShow);
                OnShow(this, e);
            }
            InvokeStatusChanged();
            return VSConstants.S_OK;
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Notifies the VSPackage that a window is being moved.
        /// </summary>
        /// <param name="x">New horizontal position.</param>
        /// <param name="y">New vertical position.</param>
        /// <param name="w">New window width.</param>
        /// <param name="h">New window height.</param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrameNotify3.OnMove(int x, int y, int w, int h)
        {
            if (OnMove != null)
            {
                var e = new WindowFramePositionChangedEventArgs(new Rectangle(x, y, w, h));
                OnMove(this, e);
            }
            InvokeStatusChanged();
            return VSConstants.S_OK;
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Notifies the VSPackage that a window is being resized.
        /// </summary>
        /// <param name="x">New horizontal position.</param>
        /// <param name="y">New vertical position.</param>
        /// <param name="w">New window width.</param>
        /// <param name="h">New window height.</param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrameNotify3.OnSize(int x, int y, int w, int h)
        {
            if (OnResize != null)
            {
                var e = new WindowFramePositionChangedEventArgs(new Rectangle(x, y, w, h));
                OnResize(this, e);
            }
            InvokeStatusChanged();
            return VSConstants.S_OK;
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Notifies the VSPackage that a window's docked state is being altered.
        /// </summary>
        /// <param name="fDockable">true if the window frame is being docked.</param>
        /// <param name="x">Horizontal position of undocked window.</param>
        /// <param name="y">Vertical position of undocked window.</param>
        /// <param name="w">Width of undocked window.</param>
        /// <param name="h">Height of undocked window.</param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. If it fails, it returns an error code.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrameNotify3.OnDockableChange(int fDockable, int x, int y, int w, int h)
        {
            if (OnDockChange != null)
            {
                var e = new WindowFrameDockChangedEventArgs(new Rectangle(x, y, w, h), fDockable != 0);
                OnDockChange(this, e);
            }
            InvokeStatusChanged();
            return VSConstants.S_OK;
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Notifies the VSPackage that a window frame is closing and tells the environment what 
        /// action to take.
        /// </summary>
        /// <param name="pgrfSaveOptions">
        /// Specifies options for saving window content. Values are taken from the __FRAMECLOSE 
        /// enumeration.
        /// </param>
        // --------------------------------------------------------------------------------------------
        int IVsWindowFrameNotify3.OnClose(ref uint pgrfSaveOptions)
        {
            if (OnClose != null)
            {
                var e = new WindowFrameCloseEventArgs((FrameCloseOption)pgrfSaveOptions);
                OnClose(this, e);
            }
            InvokeStatusChanged();
            return VSConstants.S_OK;
        }

        #endregion

        #region Private methods

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Invokes the event handler for the status changed event.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        void InvokeStatusChanged()
        {
            if (OnStatusChange != null)
            {
                var e = new EventArgs();
                OnStatusChange(this, e);
            }
        }

        #endregion
    }

    /// <summary>
    /// Specifies close options when closing a window frame.
    /// </summary>
    // ================================================================================================
    public enum FrameCloseOption
    {
        /// <summary>Do not save the document.</summary>
        NoSave = __FRAMECLOSE.FRAMECLOSE_NoSave,

        /// <summary>Save the document if it is dirty.</summary>
        SaveIfDirty = __FRAMECLOSE.FRAMECLOSE_SaveIfDirty,

        /// <summary>Prompt for document save.</summary>
        PromptSave = __FRAMECLOSE.FRAMECLOSE_PromptSave
    }

    /// <summary>
    /// Specifies the window frame positions.
    /// </summary>
    // ================================================================================================
    public enum FramePosition
    {
        /// <summary>Window frame has unknown position.</summary>
        Unknown = 0,
        /// <summary>Window frame is docked.</summary>
        Docked = VSSETFRAMEPOS.SFP_fDock,
        /// <summary>Window frame is tabbed.</summary>
        Tabbed = VSSETFRAMEPOS.SFP_fTab,
        /// <summary>Window frame floats.</summary>
        Float = VSSETFRAMEPOS.SFP_fFloat,
        /// <summary>Window frame is currently within the MDI space.</summary>
        MdiChild = VSSETFRAMEPOS.SFP_fMdiChild
    }

    /// <summary>
    /// Specifies options when the show state of a window frame changes.
    /// </summary>
    // ================================================================================================
    [Flags]
    public enum FrameShow
    {
        /// <summary>Reason unknown</summary>
        Unknown = 0,
        /// <summary>Obsolete; use WinHidden.</summary>
        Hidden = __FRAMESHOW.FRAMESHOW_Hidden,
        /// <summary>Window (tabbed or otherwise) is hidden.</summary>
        WinHidden = __FRAMESHOW.FRAMESHOW_WinHidden,
        /// <summary>A nontabbed window is made visible.</summary>
        Shown = __FRAMESHOW.FRAMESHOW_WinShown,
        /// <summary>A tabbed window is activated (made visible).</summary>
        TabActivated = __FRAMESHOW.FRAMESHOW_TabActivated,
        /// <summary>A tabbed window is deactivated.</summary>
        TabDeactivated = __FRAMESHOW.FRAMESHOW_TabDeactivated,
        /// <summary>Window is restored to normal state.</summary>
        Restored = __FRAMESHOW.FRAMESHOW_WinRestored,
        /// <summary>Window is minimized.</summary>
        Minimized = __FRAMESHOW.FRAMESHOW_WinMinimized,
        /// <summary>Window is maximized.</summary>
        Maximized = __FRAMESHOW.FRAMESHOW_WinMaximized,
        /// <summary>Multi-instance tool window destroyed.</summary>
        DestroyMultipleInstance = __FRAMESHOW.FRAMESHOW_DestroyMultInst,
        /// <summary>Autohidden window is about to slide into view.</summary>
        AutoHideSlideBegin = __FRAMESHOW.FRAMESHOW_AutoHideSlideBegin
    }

    /// <summary>
    /// Event arguments for the event raised when the show state of a window frame changes.
    /// </summary>
    // ================================================================================================
    public class WindowFrameShowEventArgs : EventArgs
    {
        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Reason of the event (why the show state is changed)?
        /// </summary>
        // --------------------------------------------------------------------------------------------
        public FrameShow Reason { get; private set; }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates an event argument instance with the initial reason.
        /// </summary>
        /// <param name="reason">Event reason.</param>
        // --------------------------------------------------------------------------------------------
        public WindowFrameShowEventArgs(FrameShow reason)
        {
            Reason = reason;
        }
    }

    /// <summary>
    /// Event arguments for the event raised when the window frame is closed.
    /// </summary>
    // ================================================================================================
    public class WindowFrameCloseEventArgs : EventArgs
    {
        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Options used to close the window frame.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        public FrameCloseOption CloseOption { get; private set; }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates an event argument instance with the initial close option.
        /// </summary>
        /// <param name="closeOption">Close option.</param>
        // --------------------------------------------------------------------------------------------
        public WindowFrameCloseEventArgs(FrameCloseOption closeOption)
        {
            CloseOption = closeOption;
        }
    }

    /// <summary>
    /// Event arguments for the events raised when the window frame position is changed.
    /// </summary>
    // ================================================================================================
    public class WindowFramePositionChangedEventArgs : EventArgs
    {
        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// New window frame position.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        public Rectangle Position { get; private set; }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates an event argument instance with the new frame position.
        /// </summary>
        /// <param name="position">New frame position.</param>
        // --------------------------------------------------------------------------------------------
        public WindowFramePositionChangedEventArgs(Rectangle position)
        {
            Position = position;
        }
    }

    /// <summary>
    /// Event arguments for the event raised when the dock state of the window frame is changed.
    /// </summary>
    // ================================================================================================
    public class WindowFrameDockChangedEventArgs : WindowFramePositionChangedEventArgs
    {
        public bool Docked { get; private set; }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates an event argument instance with the new position and dock state..
        /// </summary>
        /// <param name="position">New position of the window frame.</param>
        /// <param name="docked">True, if the frame is docked; otherwise, false.</param>
        // --------------------------------------------------------------------------------------------
        public WindowFrameDockChangedEventArgs(Rectangle position, bool docked)
            : base(position)
        {
            Docked = docked;
        }
    }
    /// <summary>
    /// This class is responsible for providing siting support for Add-Ins, packages, and other 
    /// artifacts running within the Visual Studio IDE process.
    /// </summary>
    // ================================================================================================
    public static class SiteManager
    {
        #region Lifecycle members

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Monikers of possible DTE objects. Right now VS 2008 and VS 2010 is handled.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        private static readonly List<string> VSMonikers =
          new List<string>
        {
          "!VisualStudio.DTE.9.0:{0}",
          "!VisualStudio.DTE.10.0:{0}"
        };

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// The static constructor automatically tries to assign the SiteManager static class to a site
        /// that accesses VS IDE global services.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        static SiteManager()
        {
            foreach (string moniker in VSMonikers)
            {
                TryToGetServiceProviderFromCurrentProcess(moniker);
                if (HasGlobalServiceProvider) return;
            }
        }

        #endregion

        #region Public members

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flag indicating if the SiteManager has already a global service provider or not.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        public static bool HasGlobalServiceProvider
        {
            get { return (GlobalServiceProvider != null); }
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the global service provider used by SiteManager.
        /// </summary>
        // --------------------------------------------------------------------------------------------
        public static IServiceProvider GlobalServiceProvider { get; private set; }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a global service with the specified address and behavior type.
        /// </summary>
        /// <typeparam name="SInterface">Address type of the service</typeparam>
        /// <typeparam name="TInterface">Behavior (endpoint) type of the service</typeparam>
        /// <returns>The service instance obtained.</returns>
        // --------------------------------------------------------------------------------------------
        public static TInterface GetGlobalService<SInterface, TInterface>()
            where TInterface : class
            where SInterface : class
        {
            if (!HasGlobalServiceProvider)
                throw new InvalidOperationException("The framework has not been sited!");
            var ti = GlobalServiceProvider.GetService<SInterface, TInterface>();
            if (ti == null)
                throw new NotSupportedException(typeof(SInterface).FullName);
            return ti;
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a global service with the specified behavior type.
        /// </summary>
        /// <typeparam name="TInterface">Behavior (endpoint) type of the service</typeparam>
        /// <returns>The service instance obtained.</returns>
        // --------------------------------------------------------------------------------------------
        public static TInterface GetGlobalService<TInterface>()
          where TInterface : class
        {
            return GetGlobalService<TInterface, TInterface>();
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Suggests a DTE object as the global service provider for SiteManager.
        /// </summary>
        /// <param name="dte">DTE object as a global service provider candidate.</param>
        // --------------------------------------------------------------------------------------------
        public static void SuggestGlobalServiceProvider(DTE dte)
        {
            SuggestGlobalServiceProvider(dte as IOleServiceProvider);
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Suggests a DTE2 object as the global service provider for SiteManager.
        /// </summary>
        /// <param name="dte2">DTE2 object as a global service provider candidate.</param>
        // --------------------------------------------------------------------------------------------
        public static void SuggestGlobalServiceProvider(DTE2 dte2)
        {
            SuggestGlobalServiceProvider(dte2 as IOleServiceProvider);
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Suggests an IServiceProvider object as the global service provider for SiteManager.
        /// </summary>
        /// <param name="serviceProvider">
        /// IServiceProvider object as a global service provider candidate.
        /// </param>
        // --------------------------------------------------------------------------------------------
        public static void SuggestGlobalServiceProvider(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) return;
            GlobalServiceProvider = serviceProvider;
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Suggests an IOleServiceProvider object as the global service provider for SiteManager.
        /// </summary>
        /// <param name="oleServiceProvider">
        /// IOleServiceProvider object as a global service provider candidate.
        /// </param>
        // --------------------------------------------------------------------------------------------
        public static void SuggestGlobalServiceProvider(IOleServiceProvider oleServiceProvider)
        {
            if (oleServiceProvider == null) return;
            GlobalServiceProvider = new ServiceProvider(oleServiceProvider);
        }

        #endregion

        #region Helper methods

        [DllImport("ole32.dll", PreserveSig = false)]
        private static extern void CreateBindCtx(uint reserved, out IBindCtx ppbc);

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Try to get the DTE object from the COM ROT.
        /// </summary>
        /// <remarks>Refer to http://books.google.com/books?id=kfRWvKSePmAC&pg=PA131&lpg=PA131&dq=COM+item+moniker&source=bl&ots=o6dVfcbIbq&sig=8PeV1ZW_8s4-Az036GBYoXxpIcU&hl=en&ei=YV2lScvmDobRkAXJuI28BQ&sa=X&oi=book_result&resnum=4&ct=result</remarks>
        // --------------------------------------------------------------------------------------------
        private static void TryToGetServiceProviderFromCurrentProcess(string vsMoniker)
        {
            // provides access to a bind context, which is an object
            // that stores information about a particular moniker binding operation. 
            IBindCtx ctx;

            // manages access to the Running Object Table (ROT), a globally accessible look-up
            // table on each workstation. A workstation's ROT keeps track of those objects that
            // can be identified by a moniker and that are currently running on the workstation.
            // Like the idea of Running Document Table.
            IRunningObjectTable rot;

            // used to enumerate the components of a moniker or to enumerate the monikers 
            // in a table of monikers. 
            IEnumMoniker enumMoniker;

            // contains methods that allow you to use a moniker object, which contains 
            // information that uniquely identifies a COM object.
            var moniker = new IMoniker[1];

            // ItemMoniker of the IDE, refer to http://msdn.microsoft.com/en-us/library/ms228755.aspx
            string ideMoniker = String.Format(vsMoniker, Process.GetCurrentProcess().Id);

            // Supplies a pointer to an implementation of IBindCtx (a bind context object). 
            CreateBindCtx(0, out ctx);

            // Supplies a pointer to the IRunningObjectTable interface on the local Running Object Table (ROT).
            ctx.GetRunningObjectTable(out rot);

            // Creates and returns a pointer to an enumerator that can list the monikers of 
            // all the objects currently registered in the Running Object Table (ROT).
            rot.EnumRunning(out enumMoniker);

            DTE2 dte = null;
            // Enum all the current monikers registered in COM ROT
            while (enumMoniker.Next(1, moniker, IntPtr.Zero) == 0)
            {
                string displayName;
                moniker[0].GetDisplayName(ctx, moniker[0], out displayName);
                if (displayName == ideMoniker)
                {
                    // --- Got the IDE Automation Object
                    Object oDTE;
                    rot.GetObject(moniker[0], out oDTE);
                    dte = oDTE as DTE2;
                    if (dte != null) break;
                }
            }

            SuggestGlobalServiceProvider(dte);
        }

        #endregion
    }
    /// <summary>
    /// This class defines useful extension methods used by MPF.
    /// </summary>
    // ================================================================================================
    public static class CommonExtensions
    {
        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the service described by the <typeparamref name="TService"/>
        /// type parameter.
        /// </summary>
        /// <returns>
        /// The service instance requested by the <typeparamref name="TService"/> parameter if found; otherwise null.
        /// </returns>
        /// <typeparam name="TService">The type of the service requested.</typeparam>
        /// <param name="serviceProvider">
        /// Service provider instance to request the service from.
        /// </param>
        // --------------------------------------------------------------------------------------------
        public static TService GetService<TService>(this IServiceProvider serviceProvider)
        {
            return (TService)serviceProvider.GetService(typeof(TService));
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the service described by the <typeparamref name="SInterface"/>
        /// type parameter and retrieves it as an interface type specified by the
        /// <typeparamref name="TInterface"/> type parameter.
        /// </summary>
        /// <returns>
        /// The service instance requested by the <see cref="SInterface"/> parameter if
        /// found; otherwise null.
        /// </returns>
        /// <typeparam name="SInterface">The type of the service requested.</typeparam>
        /// <typeparam name="TInterface">
        /// The type of interface retrieved. The object providing <see cref="SInterface"/>
        /// must implement <see cref="TInterface"/>.
        /// </typeparam>
        /// <param name="serviceProvider">
        /// Service provider instance to request the service from.
        /// </param>
        // --------------------------------------------------------------------------------------------
        public static TInterface GetService<SInterface, TInterface>(
          this IServiceProvider serviceProvider)
        {
            return (TInterface)serviceProvider.GetService(typeof(SInterface));
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds the specified service to the service container.
        /// </summary>
        /// <typeparam name="TService">Type of service to add to the container.</typeparam>
        /// <param name="container">Container to add the service instance.</param>
        /// <param name="callback">Callback method adding the service to the container.</param>
        // --------------------------------------------------------------------------------------------
        public static void AddService<TService>(this IServiceContainer container,
          ServiceCreatorCallback callback)
        {
            container.AddService(typeof(TService), callback);
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds the specified service to the service container.
        /// </summary>
        /// <typeparam name="TService">Type of service to add to the container.</typeparam>
        /// <param name="container">Container to add the service instance.</param>
        /// <param name="serviceInstance">Service instance</param>
        // --------------------------------------------------------------------------------------------
        public static void AddService<TService>(this IServiceContainer container,
          TService serviceInstance)
        {
            container.AddService(typeof(TService), serviceInstance);
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds the specified service to the service container.
        /// </summary>
        /// <typeparam name="TService">Type of service to add to the container.</typeparam>
        /// <param name="container">Container to add the service instance.</param>
        /// <param name="serviceInstance">Service instance</param>
        /// <param name="promote">
        /// True, if the service should be promoted to the parent service container.
        /// </param>
        // --------------------------------------------------------------------------------------------
        public static void AddService<TService>(this IServiceContainer container,
          TService serviceInstance, bool promote)
        {
            container.AddService(typeof(TService), serviceInstance, promote);
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds the specified service to the service container.
        /// </summary>
        /// <typeparam name="TService">Type of service to add to the container.</typeparam>
        /// <param name="container">Container to add the service instance.</param>
        /// <param name="callback">Callback method adding the service to the container.</param>
        /// <param name="promote">
        /// True, if the service should be promoted to the parent service container.
        /// </param>
        // --------------------------------------------------------------------------------------------
        public static void AddService<TService>(this IServiceContainer container,
          ServiceCreatorCallback callback, bool promote)
        {
            container.AddService(typeof(TService), callback, promote);
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Executes a simple action for all items in the specified collection.
        /// </summary>
        /// <typeparam name="T">Type of items in the collection.</typeparam>
        /// <param name="collection">Collection containing items.</param>
        /// <param name="action">Action to be executed on items.</param>
        // --------------------------------------------------------------------------------------------
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
                action(item);
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the specified custom attributes of a given type.
        /// </summary>
        /// <typeparam name="TAttr">Type of attributes to enumerate.</typeparam>
        /// <param name="type">Type tosearch for attributes.</param>
        /// <param name="inherit">True if the base class chain should be searched for attributes.</param>
        /// <returns>Enumerated attributes.</returns>
        // --------------------------------------------------------------------------------------------
        public static IEnumerable<TAttr> AttributesOfType<TAttr>(this Type type, bool inherit)
          where TAttr : Attribute
        {
            foreach (Attribute attr in type.GetCustomAttributes(typeof(TAttr), inherit))
            {
                var attrTypeOf = attr as TAttr;
                if (attrTypeOf != null) yield return attrTypeOf;
            }
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the specified custom attributes of a given type.
        /// </summary>
        /// <typeparam name="TAttr">Type of attributes to enumerate.</typeparam>
        /// <param name="type">Type tosearch for attributes.</param>
        /// <returns>Enumerated attributes.</returns>
        // --------------------------------------------------------------------------------------------
        public static IEnumerable<TAttr> AttributesOfType<TAttr>(this Type type)
          where TAttr : Attribute
        {
            foreach (Attribute attr in type.GetCustomAttributes(typeof(TAttr), false))
            {
                var attrTypeOf = attr as TAttr;
                if (attrTypeOf != null) yield return attrTypeOf;
            }
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Obtains an attribute with the specified type.
        /// </summary>
        /// <typeparam name="TAttr">Attribute type</typeparam>
        /// <param name="type">Type to search for attributes</param>
        /// <param name="inherit">Should be base classes searched?</param>
        /// <returns>Attribute if found; otherwise, null</returns>
        // --------------------------------------------------------------------------------------------
        public static TAttr GetAttribute<TAttr>(this Type type, bool inherit)
          where TAttr : Attribute
        {
            var attrs = type.GetCustomAttributes(typeof(TAttr), inherit);
            return attrs.Length > 0
                     ? attrs[0] as TAttr
                     : null;
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Obtains an attribute with the specified type.
        /// </summary>
        /// <typeparam name="TAttr">Attribute type</typeparam>
        /// <param name="type">Type to search for attributes</param>
        /// <returns>Attribute if found; otherwise, null</returns>
        // --------------------------------------------------------------------------------------------
        public static TAttr GetAttribute<TAttr>(this Type type)
          where TAttr : Attribute
        {
            var attrs = type.GetCustomAttributes(typeof(TAttr), true);
            return attrs.Length > 0
                     ? attrs[0] as TAttr
                     : null;
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks if the specified type derives from the given generic type.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <param name="generic">Generic type to check as base class</param>
        /// <returns>
        /// True, if the specified type derives from the given generic interface; otherwise, false.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        public static bool DerivesFromGenericType(this Type type, Type generic)
        {
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == generic)
                    return true;
                type = type.BaseType;
            }
            return false;
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the generic parameter of the specified generic types at the given posititon.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <param name="generic">Generic type definition to obtain parameter for.</param>
        /// <param name="position">Position of the parameter.</param>
        /// <returns>
        /// Type parameter at the specified position if the parameter exists; otherwise, null.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        public static Type GenericParameterOfType(this Type type, Type generic, int position)
        {
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == generic)
                {
                    var args = type.GetGenericArguments();
                    return position >= 0 && position < args.Length
                             ? args[position]
                             : null;
                }
                type = type.BaseType;
            }
            return null;
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks if a type implements the specified generic interface.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <param name="generic">Generic interface to check.</param>
        /// <returns>
        /// True, if the type implements the specified generic interface; otherwise, false.
        /// </returns>
        // --------------------------------------------------------------------------------------------
        public static bool ImplementsGenericType(this Type type, Type generic)
        {
            // --- We check only for generic interfaces.
            if (!generic.IsGenericTypeDefinition || !generic.IsInterface) return false;

            return type.GetInterfaces().Any(
              t => t.IsGenericType && !t.IsGenericTypeDefinition &&
                t.GetGenericTypeDefinition() == generic);
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the generic interface type
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <param name="generic">Generic interface to check.</param>
        /// <returns>Closed type definition implementing the generic interface.</returns>
        // --------------------------------------------------------------------------------------------
        public static Type GetImplementorOfGenericInterface(this Type type, Type generic)
        {
            return type.GetInterfaces().First(
              t => t.IsGenericType && !t.IsGenericTypeDefinition &&
                t.GetGenericTypeDefinition() == generic);
        }
    }

}
