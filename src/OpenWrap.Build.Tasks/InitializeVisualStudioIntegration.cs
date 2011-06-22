using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenWrap.Build.Tasks.Hooks;
using OpenWrap.Collections;
using OpenWrap.Commands;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.Runtime;
using OpenWrap.Services;
using OpenWrap.VisualStudio;


namespace OpenWrap.Build.Tasks
{
    public class InitializeVisualStudioIntegration : Task
    {
        public bool EnableVisualStudioIntegration { get; set; }

        public override bool Execute()
        {   
            if (!EnableVisualStudioIntegration) return true;
            try
            {
                var message = SolutionAddInEnabler.Initialize();
                if (message != null) Log.LogMessage(MessageImportance.Low, message);
            }
            catch(Exception e)
            {
                // probably running on a machine with no vs install
                Log.LogMessage(MessageImportance.Low, "DTE not detected. See exception details below.\r\n{0}", e);
            }
            return true;
        }
    }
}