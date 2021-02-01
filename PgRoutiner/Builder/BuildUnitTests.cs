using System;
using System.IO;
using System.Linq;
using Npgsql;

namespace PgRoutiner
{
    partial class Builder
    {
        private static void BuildUnitTests(NpgsqlConnection connection)
        {
            if (!Settings.Value.UnitTests || Settings.Value.UnitTestsDir == null || Settings.Value.Namespace == null)
            {
                return;
            }
            var shortDir = string.Format(Settings.Value.UnitTestsDir, Settings.Value.Namespace);
            var dir = Path.GetFullPath(Path.Join(Program.CurrentDir, shortDir));

            Program.WriteLine("Unit test dir:", dir);
        }
    }
}
