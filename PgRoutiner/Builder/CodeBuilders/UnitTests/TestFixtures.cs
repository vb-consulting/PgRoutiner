namespace PgRoutiner.Builder.CodeBuilders.UnitTests;

public class TestFixtures : Code
{
    private readonly HashSet<string> globalUsings;

    public TestFixtures(Current settings, HashSet<string> globalUsings = null) : base(settings, null)
    {
        this.globalUsings = globalUsings;

        Class = Build();
    }

    private StringBuilder Build()
    {
        StringBuilder sb = new();
        if (settings.SourceHeaderLines != null && settings.SourceHeaderLines.Count > 0)
        {
            foreach (var line in settings.SourceHeaderLines)
            {
                var value = string.Format(line, DateTime.Now);
                if (!value.StartsWith("#pragma"))
                {
                    sb.AppendLine(value);
                }
            }
        }
        if (globalUsings == null)
        {
            sb.AppendLine(@"using System.Threading.Tasks;");
            sb.AppendLine(@"using System.Linq;");
            sb.AppendLine(@"using System.Collections.Generic;");
            sb.AppendLine(@"using Norm;");
            sb.AppendLine(@"using FluentAssertions;");
            sb.AppendLine(@"using XUnit;");
            sb.AppendLine(@"using XUnit.Npgsql;");
            sb.AppendLine(@"");
        }
        else
        {
            foreach(var u in globalUsings)
            {
                sb.AppendLine($"global using {u};");
            }
            void AddUsing(string u)
            {
                if (!globalUsings.Contains(u))
                {
                    sb.AppendLine($"global using {u};");
                }
            }
            AddUsing(@"System.Threading.Tasks");
            AddUsing(@"System.Linq");
            AddUsing(@"System.Collections.Generic");
            AddUsing(@"Norm");
            AddUsing(@"FluentAssertions");
            AddUsing(@"XUnit");
            AddUsing(@"XUnit.Npgsql");
            sb.AppendLine(@"");
        }

        if (!settings.UseFileScopedNamespaces)
        {
            sb.AppendLine(@$"namespace {settings.Namespace}");
            sb.AppendLine(@"{");
        }
        else
        {
            sb.AppendLine(@$"namespace {settings.Namespace};");
            sb.AppendLine(@"");
        }

        sb.AppendLine(@$"{I1}[CollectionDefinition(""PostgreSqlDatabase"")]");
        sb.AppendLine(@$"{I1}public class TestCollection : ICollectionFixture<PostgreSqlUnitTestFixture> {{ }}");

        if (!settings.UseFileScopedNamespaces)
        {
            sb.AppendLine(@"}");
        }
        return sb;
    }
}
