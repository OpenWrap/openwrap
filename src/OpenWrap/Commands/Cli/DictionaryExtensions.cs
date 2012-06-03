using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Commands.Cli
{
    public static class DictionaryExtensions
    {
        const string CD = "openwrap.cd";
        const string SHELL_ARGS = "openwrap.shell.args";
        const string SHELL_COMMAND_LINE = "openwrap.shell.commandline";
        const string SYSPATH = "openwrap.syspath";
        const string SYSROOT = "openwrap.sysroot";
        const string SHELL_FORMATTER = "openwrap.shell.formatter";

        public static string CommandLine(this IDictionary<string, object> env)
        {
            return env.ContainsKey(SHELL_COMMAND_LINE) ? env[SHELL_COMMAND_LINE] as string : null;
        }

        public static string CurrentDirectory(this IDictionary<string, object> env)
        {
            return env.ContainsKey(CD) ? env[CD] as string ?? Environment.CurrentDirectory : Environment.CurrentDirectory;
        }

        public static IEnumerable<string> ShellArgs(this IDictionary<string, object> env)
        {
            return env.ContainsKey(SHELL_ARGS) ? env[SHELL_ARGS] as IEnumerable<string> : Enumerable.Empty<string>();
        }

        public static string SystemRepositoryPath(this IDictionary<string, object> env)
        {
            return env.ContainsKey(SYSPATH) ? env[SYSPATH] as string : null;
        }
        public static string SystemRootPath(this IDictionary<string, object> env)
        {
            return env.ContainsKey(SYSROOT) ? env[SYSROOT] as string : null;
        }
        public static string Formatter(this IDictionary<string, object> env)
        {
            return env.ContainsKey(SHELL_FORMATTER) ? env[SHELL_FORMATTER] as string : null;
        }
    }
}