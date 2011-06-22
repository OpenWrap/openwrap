using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using Process = System.Diagnostics.Process;
namespace OpenWrap.VisualStudio
{

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
            
        }

        static readonly object _syncLock = new object();
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
            {
                lock(_syncLock)
                {
                    if (!HasGlobalServiceProvider)
                    {
                        foreach (string moniker in VSMonikers)
                        {
                            TryToGetServiceProviderFromCurrentProcess(moniker);
                        }
                        if (!HasGlobalServiceProvider) return null;// try later
                    }
                }
            }
            return GlobalServiceProvider.GetService(typeof(SInterface)) as TInterface;
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
}
