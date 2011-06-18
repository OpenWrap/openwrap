using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap;
using OpenWrap.Testing;
using OpenWrap.VisualStudio.Hooks;
using Tests;
using OpenFileSystem.IO.FileSystems.Local;

namespace Tests.VisualStudio.contexts
{
    public class addin_installer : context, IDisposable
    {
        protected ITemporaryDirectory InstallDir;
        PerUserComComponentInstaller _installer;
        

        public addin_installer()
        {
            InstallDir = LocalFileSystem.Instance.CreateTempDirectory();
        }
        protected void given_install<T>(string version)
        {
            new PerUserComComponentInstaller<T>(InstallDir.Path)
            {
                VersionProvider = file => version.ToVersion()
            }.Install(PerUserComComponentInstaller.ClrVersion4);
        }
        protected void when_installing<T>(string version)
        {
            _installer = new PerUserComComponentInstaller<T>(InstallDir.Path)
            {
                VersionProvider = file => version.ToVersion()
            };
            
            _installer.Install(PerUserComComponentInstaller.ClrVersion4);
        }

        protected void given_empty_registry_for<T>()
        {
            new PerUserComComponentInstaller<T>(InstallDir.Path)
                .Uninstall();
        }
        

        public void Dispose()
        {
            _installer.Uninstall();
        }

        protected string CodeBase<T>()
        {
            var guid = ((GuidAttribute)Attribute.GetCustomAttribute(typeof(T),typeof(GuidAttribute))).Value;
            var regKey = Registry.CurrentUser.OpenSubKey(string.Format(@"Software\Classes\CLSID\{{{0}}}\InprocServer32", guid));
            return regKey
                .Value("CodeBase")
                .ShouldBeOfType<string>();
        }

        protected string FileName<T>()
        {
            return typeof(T).Assembly.GetName().Name + ".dll";
        }
    }
}