namespace PgRoutiner.Builder;

public class Writer
{
    public static void CreateFile(string file, string content)
    {
        var relative = file.GetRelativePath();
        var shortFilename = Path.GetFileName(file);
        var dir = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(file)));
        var exists = File.Exists(file);

        if (!Current.Value.DumpConsole && !Directory.Exists(dir))
        {
            DumpRelativePath("Creating dir: {0} ...", dir);
            Directory.CreateDirectory(dir);
        }

        if (exists && Current.Value.Overwrite == false)
        {
            DumpFormat("File {0} exists, overwrite is set to false, skipping ...", relative);
            return;
        }
        if (exists && Current.Value.SkipIfExists != null && (
            Current.Value.SkipIfExists.Contains(shortFilename) || Current.Value.SkipIfExists.Contains(relative))
            )
        {
            DumpFormat("Skipping {0}, already exists ...", relative);
            return;
        }
        if (exists && Current.Value.AskOverwrite &&
            Program.Ask($"File {relative} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
        {
            DumpFormat("Skipping {0} ...", relative);
            return;
        }

        DumpFormat("Creating file {0} ...", relative);
        WriteFile(file, content);
    }
    
    public static bool WriteFile(string path, string content)
    {
        if (content == null)
        {
            return false;
        }
        if (Current.Value.DumpConsole)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(content);
            Console.ResetColor();
            return true;
        }
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(path, content);
            return true;
        }
        catch (Exception e)
        {
            Program.DumpError($"File {path} could not be written: {e.Message}");
            return false;
        }
    }

    public static void Dump(params string[] lines)
    {
        if (Current.Value.Silent)
        {
            return;
        }
        Program.WriteLine(ConsoleColor.Yellow, lines);
    }

    public static void DumpTitle(params string[] lines)
    {
        if (Current.Value.Silent)
        {
            return;
        }
        Program.WriteLine(ConsoleColor.Green, lines);
    }

    public static void DumpFormat(string msg, params object[] values)
    {
        if (Current.Value.Silent)
        {
            return;
        }
        msg = string.Format(msg, values.Select(v => $"`{v}`").ToArray());
        foreach (var (line, i) in msg.Split('`').Select((l, i) => (l, i)))
        {
            if (i % 2 == 0)
            {
                Program.Write(ConsoleColor.Yellow, line);
            }
            else
            {
                Program.Write(ConsoleColor.Cyan, line);
            }
        }

        Program.WriteLine("");
    }

    public static void DumpRelativePath(string msg, string path)
    {
        DumpFormat(msg, path.GetRelativePath());
    }

    public static void Error(string msg)
    {
        Program.WriteLine(ConsoleColor.Red, $"ERROR: {msg}");
    }
}
