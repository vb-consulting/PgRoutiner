namespace PgRoutiner.SettingsManagement;

public class ProgramInfo
{
    public static void ShowVersion()
    {
        Console.WriteLine();
        Console.ResetColor();
        Console.WriteLine($"PgRoutiner ({Program.Version})");
        Console.WriteLine();
    }

    public static void ShowStartupInfo()
    {
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
        Program.Write(ConsoleColor.Yellow, "Issues");
        Program.WriteLine(ConsoleColor.Cyan, "   https://github.com/vb-consulting/PgRoutiner/issues");
        Program.Write(ConsoleColor.Yellow, "Donate");
        Program.Write(ConsoleColor.Cyan, "   bitcoincash:qp93skpzyxtvw3l3lqqy7egwv8zrszn3wcfygeg0mv");
        Program.WriteLine(ConsoleColor.Cyan, "   https://www.paypal.com/paypalme/vbsoftware/");
        Program.WriteLine(ConsoleColor.Yellow, $"Copyright (c) VB Consulting and VB Software {DateTime.Now.Year}.",
            "This program and source code is licensed under the MIT license.");
        Program.WriteLine(ConsoleColor.Cyan, "https://github.com/vb-consulting/PgRoutiner/blob/master/LICENSE");
    }

    public static bool ShowDebug(bool customSettings)
    {
        if (Program.ConsoleSettings.Settings && !customSettings)
        {
            Current.ShowSettings();
            return true;
        }

        if (Program.ConsoleSettings.Debug)
        {
            Program.WriteLine("", "Debug: ");
            Program.WriteLine("Version: ");
            Program.WriteLine(ConsoleColor.Cyan, " " + Program.Version);
            var path = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            Program.WriteLine("Executable dir: ");
            Program.WriteLine(ConsoleColor.Cyan, " " + path);
            Program.WriteLine("OS: ");
            Program.WriteLine(ConsoleColor.Cyan, " " + Environment.OSVersion);
            Program.WriteLine("Run: ");
            Program.WriteLine(ConsoleColor.Cyan, $" {Current.Value.Routines}");
            Program.WriteLine("CommitComments: ");
            Program.WriteLine(ConsoleColor.Cyan, $" {Current.Value.CommitMd}");
            Program.WriteLine("Dump: ");
            Program.WriteLine(ConsoleColor.Cyan, $" {Current.Value.DumpConsole}");
            Program.WriteLine("Execute: ");
            Program.WriteLine(ConsoleColor.Cyan, $" {Current.Value.Execute ?? "<null>"}");
            Program.WriteLine("Diff: ");
            Program.WriteLine(ConsoleColor.Cyan, $" {Current.Value.Diff}");
            return true;
        }

        return false;
    }

    public static void ShowInfo()
    {
        ShowVersion();

        Program.WriteLine("", "To get help please navigate to: ");
        Program.WriteLine(ConsoleColor.Cyan, " https://github.com/vb-consulting/PgRoutiner#readme", "");
    }
}
