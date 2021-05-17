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
                string location;
#if SELFCONTAINED
                location = $"{System.AppContext.BaseDirectory}pgroutiner.exe";
#else
                Assembly assembly = Assembly.GetExecutingAssembly();
                location = assembly.Location;
#endif
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(location);
                version = fvi.FileVersion;
                return version;
            }
        }
    }
}
