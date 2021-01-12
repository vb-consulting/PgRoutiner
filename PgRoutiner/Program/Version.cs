using System;
using System.Diagnostics;
using System.Reflection;

namespace PgRoutiner
{
    static partial class Program
    {
        private static string version = null;

        public static string Version 
        { 
            get
            {
                if (version != null)
                {
                    return version;
                }
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                version = fvi.FileVersion;
                return version;
            }
        }
    }
}
