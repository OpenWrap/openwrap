using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using OpenWrap.PackageModel;
using OpenWrap.Testing;
using Tests.Commands;

namespace OpenWrap.Commands.add_wrap
{
    class adding_to_existing_scope : global::Tests.Commands.contexts.add_wrap
    {
        DateTimeOffset? DefaultDescriptorTimeStamp;
        DateTimeOffset? ScopedDescriptorTimeStamp;

        public adding_to_existing_scope()
        {
            given_system_package("sauron", "1.0.0");
            given_project_package("one-ring", "1.0.0");
            
            given_dependency("tests", "depends: one-ring");

            DefaultDescriptorTimeStamp = Environment.ScopedDescriptors[string.Empty].File.LastModifiedTimeUtc;
            ScopedDescriptorTimeStamp = Environment.ScopedDescriptors[string.Empty].File.LastModifiedTimeUtc;
            when_executing_command("sauron", "-scope", "tests");
        }

        [Test]
        public void command_is_successful()
        {
            Results.ShouldHaveNoError();
        }

        [Test]
        public void default_descriptor_is_not_updated()
        {
            WrittenDescriptor().Dependencies.ShouldBeEmpty();
        }

        [Test]
        public void scoped_descriptor_is_updated()
        {
            WrittenDescriptor("tests").Dependencies.ShouldHaveCountOf(2);
        }

        [Test]
        public void default_descriptor_timestamp_is_updated()
        {
            (Environment.ScopedDescriptors[string.Empty].File.LastModifiedTimeUtc > DefaultDescriptorTimeStamp).ShouldBeTrue();

        }

        [Test]
        public void scoped_descriptor_timestamp_is_updated()
        {
            (Environment.ScopedDescriptors["tests"].File.LastModifiedTimeUtc > ScopedDescriptorTimeStamp).ShouldBeTrue();
        }
    }
}
