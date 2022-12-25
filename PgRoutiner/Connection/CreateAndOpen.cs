namespace PgRoutiner.Connection;

public partial class ConnectionManager
{
    private static readonly IEnumerable<string> InfoLevels = new[] { "INFO", "NOTICE", "LOG" };
    private static readonly IEnumerable<string> ErrorLevels = new[] { "ERROR", "PANIC" };

    private NpgsqlConnection CreateAndOpen(string connectionStr)
    {
        var conn = new NpgsqlConnection(connectionStr);
        conn.Notice += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Notice.Severity))
            {
                Program.Write(ConsoleColor.Yellow, $"{args.Notice.Severity} ");
            }
            Program.WriteLine(ConsoleColor.Cyan, args.Notice.MessageText);
        };
        conn.Open();

        var keyValue = typeof(Current).GetProperty(connectionKey).GetValue(Current.Value) as string;
        if (Current.Value.Verbose || Current.Value.Info)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                Program.WriteLine("");
                Program.Write($"Using {name1} ");
                Program.Write(ConsoleColor.Yellow, keyValue);
                Program.Write(":");
                Program.WriteLine("");
                Program.Write(ConsoleColor.Cyan, " " + conn.ConnectionString);
                Program.WriteLine(ConsoleColor.Yellow, $"  (PostgreSQL {conn.PostgreSqlVersion})");
            }
            else
            {
                Program.WriteLine("");
                Program.Write($"Using {name1}:");
                Program.WriteLine(ConsoleColor.Cyan, " " + conn.ConnectionString);
                Program.Write(ConsoleColor.Yellow, " " + conn.PostgreSqlVersion);
                Program.WriteLine(ConsoleColor.Yellow, $"  (PostgreSQL {conn.PostgreSqlVersion})");
            }
        }
        return conn;
    }
}
