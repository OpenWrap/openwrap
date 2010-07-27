using System;
using System.Linq;
using OpenWrap.Dependencies;
using NUnit.Framework;
using OpenWrap.Testing;
using OpenWrap.Tests.Dependencies.contexts;

// ReSharper disable InconsistentNaming
namespace OpenWrap.Tests.Dependencies
{
    class when_parsing_override_of_sonic : override_parser_context
    {
        public when_parsing_override_of_sonic()
        {
            given_override("override: sonic super-sonic");
        }

        [Test]
        public void sonic_is_changed_to_super_sonic()
        {
            var sonic = new WrapDependency { Name = "sonic" };
            wrapOverride.Apply(sonic);
            sonic.Name.ShouldBe("super-sonic");
        }

        [Test]
        public void tails_is_still_tails()
        {
            var tails = new WrapDependency { Name = "tails" };
            wrapOverride.Apply(tails);
            tails.Name.ShouldBe("tails");
        }
    }

    class when_parsing_non_override : override_parser_context
    {
        public when_parsing_non_override()
        {
            given_override("foo bar");
        }

        [Test]
        public void no_override_is_created()
        {
            wrapOverride.ShouldBeNull();
        }
    }

    class when_parsing_invalid_override_with_single_argument : override_parser_context
    {
        public when_parsing_invalid_override_with_single_argument()
        {
            given_override("override: single-argument");
        }

        [Test]
        public void Exception_is_thrown()
        {
            exception.ShouldNotBeNull();
        }
    }

    class when_parsing_invalid_override_with_no_arguments : override_parser_context
    {
        public when_parsing_invalid_override_with_no_arguments()
        {
            given_override("override");
        }

        [Test]
        public void Exception_is_thrown()
        {
            exception.ShouldNotBeNull();
        }
    }

    namespace contexts
    {
        public abstract class override_parser_context : context
        {
            protected WrapOverride wrapOverride;
            protected Exception exception;

            protected void given_override(string overrideLine)
            {
                var target = new WrapDescriptor();
                try
                {
                    new OverrideParser().Parse(overrideLine, target);
                    wrapOverride = target.Overrides.First();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }
        }
    }
}
// ReSharper restore InconsistentNaming