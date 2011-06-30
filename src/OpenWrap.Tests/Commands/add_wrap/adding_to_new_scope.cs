using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.add_wrap
{
    class adding_to_new_scope : global::Tests.Commands.contexts.add_wrap
    {
        DateTimeOffset? DefaultDescriptorTimeStamp;
        DateTimeOffset? ScopedDescriptorTimeStamp;
        public adding_to_new_scope()
        {
            given_system_package("sauron", "1.0.0");
            given_project_package("one-ring", "1.0.0");
            
            given_dependency("depends: one-ring");

            DefaultDescriptorTimeStamp = Environment.ScopedDescriptors[string.Empty].File.LastModifiedTimeUtc;
            ScopedDescriptorTimeStamp = Environment.ScopedDescriptors[string.Empty].File.LastModifiedTimeUtc;
            when_executing_command("sauron -scope tests");
        }
        [Test]
        public void should_succeed()
        {
            Results.ShouldHaveNoError();
        }

        [Test]
        public void default_descriptor_is_not_updated()
        {
            WrittenDescriptor().Dependencies.ShouldHaveCountOf(1)
                    .First().Name.ShouldBe("one-ring");
        }

        [Test]
        public void scoped_descriptor_is_updated()
        {
            WrittenDescriptor("tests").Dependencies.ShouldHaveCountOf(1)
                    .First().Name.ShouldBe("sauron");
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