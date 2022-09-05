using Norm;

namespace PgRoutiner.Builder;

public class Executor
{
    public static void Execute(NpgsqlConnection connection, string content)
    {
        if (Current.Value.DumpConsole)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(content);
            Console.ResetColor();
            return;
        }
        const int max = 1000;
        var len = content.Length;
        var dump = content.Substring(0, max > len ? len : max);
        Program.WriteLine("SQL:");
        Program.WriteLine(ConsoleColor.Cyan, dump);
        if (len > max)
        {
            Program.WriteLine("...", $"[{len - max} more characters]");
        }
        Program.WriteLine("");

        if (!Current.Value.DumpConsole)
        {
            connection.Execute(content);
        }
    }

    public static bool ExecuteFromSetting(NpgsqlConnection connection)
    {
        if (Current.Value.Execute == null)
        {
            return false;
        }
        var file = Path.GetFullPath(Path.Combine(Program.CurrentDir, Current.Value.Execute));
        if (File.Exists(file))
        {
            ExecuteFile(connection, file);
            return true;
        }
        var dir = Path.GetDirectoryName(file);
        var fileName = Path.GetFileName(file);
        var pattern = string.IsNullOrEmpty(fileName) ? "*.*" : fileName;
        var fileList = Directory.EnumerateFiles(dir, pattern, SearchOption.TopDirectoryOnly).ToList();
        var count = fileList.Count();
        if (count == 0)
        {
            ExecutePsql(connection);
            return true;
        }

        Writer.DumpFormat("Found {0} files in top directory only of {1} for the searh pattern {2}", fileList.Count(), dir, pattern);
        foreach (var f in fileList)
        {
            ExecuteFile(connection, f);
        }
        return true;
    }

    public static void ExecutePsql(NpgsqlConnection connection)
    {
        var split = Current.Value.Execute.Split(';');
        var execute = string.Join(" ", split.Select(s =>
        {
            if (File.Exists(s))
            {
                return $"-f \"{s}\"";
            }
            return $"-c \"{s}\"";
        }));
        new PsqlRunner(Current.Value, connection).Run(execute);
    }

    public static void ExecuteFile(NpgsqlConnection connection, string fileName)
    {
        try
        {
            Writer.Dump("");
            Writer.DumpRelativePath("Executing file {0} ...", fileName);
            new PsqlRunner(Current.Value, connection).Run($"-f \"{fileName}\"");
        } 
        catch
        {
            try
            {
                Writer.Dump("");
                Writer.DumpRelativePath("Executing file {0} ...", fileName);
                Execute(connection, File.ReadAllText(fileName));
            }
            catch (Exception e)
            {
                Program.WriteLine(ConsoleColor.Red, $"Failed to execute file {fileName} content.", $"ERROR: {e.Message}");
            }
        }
    }
}
