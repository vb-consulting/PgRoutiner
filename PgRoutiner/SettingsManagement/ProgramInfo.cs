namespace PgRoutiner.SettingsManagement;

public class ProgramInfo
{
    public static void ShowVersion()
    {
        Console.WriteLine();
        Program.WriteLine("Version: ");
        Program.WriteLine(ConsoleColor.Cyan, " " + Program.Version);
    }

    public static void ShowStartupInfo()
    {
        Program.Write(ConsoleColor.Yellow, $"Copyright (c) VB Consulting and VB Software {DateTime.Now.Year}. This program and source code is licensed under the MIT license.");
        Program.WriteLine(ConsoleColor.Cyan, " https://github.com/vb-consulting/PgRoutiner/blob/master/LICENSE");
        Program.WriteLine(ConsoleColor.Yellow, $"PgRoutiner: {Program.Version}");
        Program.Write(ConsoleColor.Yellow, "Type ");
        Program.Write(ConsoleColor.Cyan, $"pgroutiner {Current.HelpArgs.Alias}");
        Program.Write(ConsoleColor.Yellow, " or ");
        Program.Write(ConsoleColor.Cyan, $"pgroutiner {Current.HelpArgs.Name}");
        Program.Write(ConsoleColor.Yellow, " to see help on available commands and settings.");
        Program.WriteLine("");
        Program.Write(ConsoleColor.Yellow, "Type ");
        Program.Write(ConsoleColor.Cyan, $"pgroutiner {Current.SettingsArgs.Alias}");
        Program.Write(ConsoleColor.Yellow, " or ");
        Program.Write(ConsoleColor.Cyan, $"pgroutiner {Current.SettingsArgs.Name}");
        Program.WriteLine(ConsoleColor.Yellow, " to see the currently selected settings.");
        
    }

    public static void ShowInfo()
    {
        ShowVersion();
        Console.WriteLine();
        
        void Show((string header, List<(string cmds, string help)> args) section)
        {
            if (section.header != null)
            {
                Program.WriteLine(ConsoleColor.Yellow, $"{section.header}");
            }
            foreach (var entry in section.args)
            {
                Program.Write(ConsoleColor.Cyan, $"  {entry.cmds}");
                Program.WriteLine($"    {entry.help}");
            }
            Console.WriteLine();
        }

        Show(FormatedSettings.SettingHelp());
        Show(FormatedSettings.GeneralSettingHelp());
        Show(FormatedSettings.CodeGenGeneralSettingHelp());
        Show(FormatedSettings.RoutinesSettingHelp());
        Show(FormatedSettings.UnitTestsSettingHelp());
        Show(FormatedSettings.SchemaDumpSettingHelp());
        Show(FormatedSettings.DataDumpSettingHelp());
        Show(FormatedSettings.ObjTreeSettingHelp());
        Show(FormatedSettings.MarkdownSettingHelp());
        Show(FormatedSettings.PsqlSettingHelp());
        Show(FormatedSettings.ModelSettingHelp());
        Show(FormatedSettings.CrudSettingHelp());

        Program.WriteLine("", "To get help please navigate to: ");
        Program.WriteLine(ConsoleColor.Cyan, " https://github.com/vb-consulting/PgRoutiner#readme", "");
    }
}
