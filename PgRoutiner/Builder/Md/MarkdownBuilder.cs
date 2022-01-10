namespace PgRoutiner.Builder.Md;

public class MarkdownBuilder
{
    public static void BuilMd(NpgsqlConnection connection, string connectionName)
    {
        if (string.IsNullOrEmpty(Settings.Value.MdFile))
        {
            return;
        }
        var file = string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.MdFile)), connectionName);
        var relative = file.GetRelativePath();
        var shortFilename = Path.GetFileName(file);
        var dir = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(file)));
        var exists = File.Exists(file);

        if (!Settings.Value.Dump && !Directory.Exists(dir))
        {
            Writer.DumpRelativePath("Creating dir: {0} ...", dir);
            Directory.CreateDirectory(dir);
        }

        if (!Settings.Value.Dump && exists && Settings.Value.MdOverwrite == false)
        {
            Writer.DumpFormat("File {0} exists, overwrite is set to false, skipping ...", relative);
            return;
        }
        if (!Settings.Value.Dump && exists && Settings.Value.SkipIfExists != null && (
            Settings.Value.SkipIfExists.Contains(shortFilename) || Settings.Value.SkipIfExists.Contains(relative))
            )
        {
            Writer.DumpFormat("Skipping {0}, already exists ...", relative);
            return;
        }
        if (!Settings.Value.Dump && exists && Settings.Value.MdAskOverwrite &&
            Program.Ask($"File {relative} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
        {
            Writer.DumpFormat("Skipping {0} ...", relative);
            return;
        }

        Writer.DumpFormat("Creating markdown file {0} ...", relative);
        var builder = new MarkdownDocument(Settings.Value, connection);
        Writer.WriteFile(file, builder.Build());
    }

    public static bool BuildMdDiff(NpgsqlConnection connection)
    {
        if (!Settings.Value.CommitMd)
        {
            return false;
        }
        if (Settings.Value.MdFile == null)
        {
            Program.WriteLine(ConsoleColor.Red, $"ERROR: Markdown file setting is not set (CommentsMdFile setting). Can't commit comments.");
            return true;
        }
        var file = Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.MdFile));
        if (!File.Exists(file))
        {
            Program.WriteLine(ConsoleColor.Red, $"ERROR: Markdown file {Settings.Value.MdFile} does not exists. Can't commit comments.");
            return true;
        }

        string content = null;
        try
        {
            var builder = new MarkdownDocument(Settings.Value, connection);
            content = builder.BuildDiff(file);
        }
        catch (Exception e)
        {
            Program.WriteLine(ConsoleColor.Red, $"Could not parse {Settings.Value.MdFile} file.", $"ERROR: {e.Message}");
        }

        try
        {
            Executor.Execute(connection, content);
            Writer.Dump("Executed successfully!");
        }
        catch (Exception e)
        {
            Program.WriteLine(ConsoleColor.Red, $"Failed to execute comments script.", $"ERROR: {e.Message}");
        }

        return true;
    }
}
