using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;

namespace OpenWrap.Build.Tasks
{
    public class InitializeEnvironment : Task
    {
        public string DescriptorPath { get; set; }
        public string ProjectRepositoryPath { get; set; }
        //public string BuildTaskDirectory

        public override bool Execute()
        {
            
            throw new NotImplementedException();
        }
    }
}
