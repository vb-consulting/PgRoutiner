﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRoutiner
{
    public class UnitTestProjectCsprojFile
    {
        private readonly Settings settings;
        private readonly string dir;

        public UnitTestProjectCsprojFile(Settings settings, string dir)
        {
            this.settings = settings;
            this.dir = dir;
        }

        public override string ToString()
        {
            if (!string.Equals(settings.UnitTestProjectTargetFramework, "net5.0") &&
                !string.Equals(settings.UnitTestProjectTargetFramework, "net6.0"))
            {
                Program.DumpError("UnitTestProjectTargetFramework can only have values net5.0 or net6.0");
                return null;
            }

            if (settings.UnitTestProjectLangVersion != null &&
                !string.Equals(settings.UnitTestProjectLangVersion, "9") &&
                !string.Equals(settings.UnitTestProjectLangVersion, "10"))
            {
                Program.DumpError("UnitTestProjectLangVersion can be null (skipped) or have values 9 or 10");
                return null;
            }

            if (settings.UseFileScopedNamespaces &&
                string.Equals(settings.UnitTestProjectTargetFramework, "net5.0") &&
                (settings.UnitTestProjectLangVersion == null || !string.Equals(settings.UnitTestProjectLangVersion, "10")))
            {
                Program.DumpError("UseFileScopedNamespaces cannor be used with TargetFramework net5.0. Use net6.0 or LangVersion 10");
                return null;
            }

            if (settings.UseFileScopedNamespaces &&
                string.Equals(settings.UnitTestProjectTargetFramework, "net6.0") &&
                settings.UnitTestProjectLangVersion != null &&
                string.Equals(settings.UnitTestProjectLangVersion, "9"))
            {
                Program.DumpError("UseFileScopedNamespaces cannor be used with TargetFramework net6.0 and LangVersion 9. Set LangVersion to null or use TargetFramework net5.0");
                return null;
            }

            StringBuilder sb = new(@"<Project Sdk=""Microsoft.NET.Sdk"">");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(@"  <PropertyGroup>");
            sb.AppendLine(@$"    <TargetFramework>{settings.UnitTestProjectTargetFramework}</TargetFramework>");
            if (settings.UnitTestProjectLangVersion != null)
            {
                sb.AppendLine(@$"    <LangVersion>{settings.UnitTestProjectLangVersion}</LangVersion>");
            }
            sb.AppendLine(@"    <IsPackable>false</IsPackable>");
            sb.AppendLine(@"  </PropertyGroup>");
            sb.AppendLine();
            sb.AppendLine(@"  <ItemGroup>");
            sb.AppendLine(@"    <None Update=""testsettings.json"">");
            sb.AppendLine(@"      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>");
            sb.AppendLine(@"    </None>");
            sb.AppendLine(@"  </ItemGroup>");
            sb.AppendLine();
            if (Settings.ProjectInfo?.ProjectFile != null)
            {
                sb.AppendLine(@"  <ItemGroup>");
                sb.AppendLine(@$"    <ProjectReference Include=""{Path.GetRelativePath(dir, Settings.ProjectInfo.ProjectFile)}"" />");
                sb.AppendLine(@"  </ItemGroup>");
            }
            sb.AppendLine();
            sb.AppendLine(@"</Project>");
            return sb.ToString();
        }
    }
}
