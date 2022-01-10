using PgRoutiner.Builder.Dump;
using PgRoutiner.Connection;

namespace PgRoutiner.Builder.DiffBuilder;

public class DiffScript
{
    public static void BuildDiffScript(NpgsqlConnection connection, string connectionName)
    {
        if (!Settings.Value.Diff)
        {
            return;
        }
        using var target = new ConnectionManager(Program.Config, nameof(Settings.DiffTarget), Settings.Value.Connection).ParseConnectionString();
        if (target == null)
        {
            return;
        }

        var sourceBuilder = new PgDumpBuilder(Settings.Value, connection);
        if (!PgDumpVersion.Check(sourceBuilder))
        {
            return;
        }

        var targetBuilder = new PgDumpBuilder(Settings.Value, target, nameof(Settings.DiffPgDump));
        if (!PgDumpVersion.Check(targetBuilder))
        {
            return;
        }
        Writer.Dump("");
        var targetName = (Settings.Value.DiffTarget ?? $"{target.Host}_{target.Port}_{target.Database}").SanitazePath();

        var now = DateTime.Now;
        string GetFilePattern(int ord)
        {
            var c1 = Settings.Value.DiffFilePattern.Contains("{0");
            var c2 = Settings.Value.DiffFilePattern.Contains("{1");
            var c3 = Settings.Value.DiffFilePattern.Contains("{2");
            var c4 = Settings.Value.DiffFilePattern.Contains("{3");

            if (c1 && c2 && c3 && c4)
            {
                return string.Format(Settings.Value.DiffFilePattern, connectionName, targetName, ord, now);
            }
            else if (c1 && c2 && c3)
            {
                return string.Format(Settings.Value.DiffFilePattern, connectionName, targetName, ord);
            }
            else if (c1 && c2)
            {
                return string.Format(Settings.Value.DiffFilePattern, connectionName, targetName);
            }
            else if (c1)
            {
                return string.Format(Settings.Value.DiffFilePattern, connectionName);
            }
            return Settings.Value.DiffFilePattern;
        }


        var title = string.Format("{0}__diff__{1}", connection.Database, target.Database).SanitazeName();
        var builder = new PgDiffBuilder(Settings.Value, connection, target, sourceBuilder, targetBuilder, title);
        var content = builder.Build((msg, step, total) =>
        {
            Writer.DumpFormat(string.Concat("Step ", step.ToString(), " of ", total.ToString(), ": {0}"), msg);
        });
        if (string.IsNullOrEmpty(content))
        {
            Writer.Dump($"No diff found between {connectionName} and {targetName}...");
            return;
        }
        if (Settings.Value.DiffFilePattern != null && !Settings.Value.Dump)
        {
            var dir = Path.GetFullPath(Path.GetDirectoryName(GetFilePattern(1)));
            if (!Directory.Exists(dir))
            {
                Writer.DumpFormat("Creating dir: {0}", dir);
                Directory.CreateDirectory(dir);
            }
            int i = 1;
            foreach (var existingFile in Directory.EnumerateFiles(dir))
            {
                var fileContent = File.ReadAllText(existingFile);
                if (Equals(content, fileContent))
                {
                    Writer.DumpRelativePath("File with same diff script {0} already exists ...", existingFile);
                    return;
                }
                i++;
            }

            var file = GetFilePattern(i);
            Writer.DumpRelativePath("Creating new diff file: {0} ...", Path.GetFullPath(file));
            Writer.WriteFile(file, content);
        }
        else
        {
            Writer.Dump("");
            Writer.DumpFormat("Source connection: {0}", connectionName);
            Writer.DumpFormat("Target connection: {0}", targetName);
            Writer.Dump("Diff script:");
            Console.Write(content);
        }
    }
}
