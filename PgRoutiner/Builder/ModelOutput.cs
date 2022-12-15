using System;
using System.Data;
using System.Xml.Linq;
using PgRoutiner.Builder.CodeBuilders;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.Builder;

public class ModelOutput : Code
{
    public ModelOutput(string name) : base(Current.Value, name)
    {
    }

    public static void BuilModelOutput(NpgsqlConnection connection, string connectionName)
    {
        if (string.IsNullOrEmpty(Current.Value.ModelOutputQuery))
        {
            return;
        }

        var modelOutputFile = string.Equals(Current.Value.ModelOutputFile?.ToLowerInvariant(), ".ts") ? null : Current.Value.ModelOutputFile;
        var content = new ModelOutput(modelOutputFile).BuilModelOutputContent(connection, connectionName);
        
        
        if (modelOutputFile == null)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(content);
            Console.ResetColor();
        }
        else
        {
            var file = string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, modelOutputFile)), connectionName);
            var relative = file.GetRelativePath();
            var shortFilename = Path.GetFileName(file);
            var dir = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(file)));
            var exists = File.Exists(file);

            if (!Current.Value.DumpConsole && !Directory.Exists(dir))
            {
                Writer.DumpRelativePath("Creating dir: {0} ...", dir);
                Directory.CreateDirectory(dir);
            }

            if (exists && Current.Value.Overwrite == false)
            {
                Writer.DumpFormat("File {0} exists, overwrite is set to false, skipping ...", relative);
                return;
            }
            if (exists && Current.Value.SkipIfExists != null && (
                Current.Value.SkipIfExists.Contains(shortFilename) || Current.Value.SkipIfExists.Contains(relative))
                )
            {
                Writer.DumpFormat("Skipping {0}, already exists ...", relative);
                return;
            }
            if (exists && Current.Value.Overwrite &&
                Program.Ask($"File {relative} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
            {
                Writer.DumpFormat("Skipping {0} ...", relative);
                return;
            }

            Writer.DumpFormat("Creating model file {0} ...", relative);
            Writer.WriteFile(file, content);
        }
    }

    private string BuilModelOutputContent(NpgsqlConnection connection, string connectionName)
    {
        var sb = new StringBuilder();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        var file = string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, Current.Value.ModelOutputQuery)), connectionName);
        string queries = File.Exists(file) ? File.ReadAllText(file) : Current.Value.ModelOutputQuery;
        var ts = Current.Value.ModelOutputFile?.ToLowerInvariant()?.EndsWith(".ts") ?? false;
        
        foreach(var line in Current.Value.SourceHeaderLines)
        {
            if (ts && line.StartsWith("#pragma"))
            {
                continue;
            }
            sb.AppendLine(line);
        }
        sb.AppendLine();

        if (!ts)
        {
            var ns = Current.Value.Namespace;
            if (string.IsNullOrEmpty(ns))
            {
                ns = Program.CurrentDir.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Last()
                    .SanitazeName()
                    .ToUpperCamelCase();
            }
            if (Name != null)
            {
                var modelDir = Path.GetDirectoryName(string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, Name)), connectionName));
                if (!string.Equals(modelDir, Program.CurrentDir))
                {
                    var extra = modelDir.Replace(Program.CurrentDir, "").PathToNamespace();
                    ns = string.Concat(ns, ".", extra);
                }
            }
            if (!Current.Value.UseFileScopedNamespaces)
            {
                sb.AppendLine($"namespace {ns}");
                sb.AppendLine("{");
            }
            else
            {
                sb.AppendLine($"namespace {ns};");
                sb.AppendLine();
            }
        }

        foreach (var (query, index) in queries.Split(";", StringSplitOptions.RemoveEmptyEntries).Select((q, i) => (q, i)))
        {
            var name = query.GetFrom()?.ToUpperCamelCase();
            if (name == null)
            {
                Program.WriteLine(ConsoleColor.Red, $"ERROR: Following query might be malformed, skipping. Skipping. Query:{NL}{query}");
            }
            using var command = connection.CreateCommand();
            command.CommandText = query;

            try
            {
                using var reader = command.ExecuteReader(CommandBehavior.SingleRow);
                if (!reader.Read())
                {
                    Program.WriteLine(ConsoleColor.Red, $"ERROR: Following query doesn't seem to contain any rows. Skipping. Query:{NL}{query}");
                }
                else
                {
                    if (!ts)
                    {
                        if (!Current.Value.UseRecords)
                        {
                            AddCsClass(reader, sb, name, index);
                        }
                        else
                        {
                            AddCsRecord(reader, sb, name, index);
                        }
                    }
                    else
                    {
                        AddTsModel(reader, sb, name, index);
                    }
                }
            }
            catch(Exception exc)
            {
                Program.WriteLine(ConsoleColor.Red, $"ERROR: Following query raised error, skipping. Skipping. Query:{NL}{query}{NL}Error:{NL}{exc}");
            }
        }
        if (!ts && !Current.Value.UseFileScopedNamespaces)
        {
            sb.AppendLine("}");
        }
        return sb.ToString();
    }

    private void AddCsClass(NpgsqlDataReader reader, StringBuilder sb, string name, int index)
    {
        if (index > 0)
        {
            sb.AppendLine();
        }
        sb.AppendLine($"{I1}public class {name}");
        sb.AppendLine($"{I1}{{");
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var type = GetCsMapping(reader, i);
            var fieldName = reader.GetName(i).ToUpperCamelCase();
            sb.AppendLine($"{I2}public {type} {fieldName} {{ get; set; }}");
        }
        sb.AppendLine($"{I1}}}");
    }
    private void AddCsRecord(NpgsqlDataReader reader, StringBuilder sb, string name, int index)
    {
        if (index > 0)
        {
            sb.AppendLine();
        }
        sb.AppendLine($"{I1}public record {name}(");

        for (int i = 0; i < reader.FieldCount; i++)
        {
            var type = GetCsMapping(reader, i);
            var fieldName = reader.GetName(i).ToCamelCase();
            sb.Append($"{I2}{type} {fieldName}");
            if (i < reader.FieldCount - 1)
            {
                sb.AppendLine(",");
            }
            else
            {
                sb.AppendLine();
            }
        }
        sb.AppendLine($"{I1});");
    }

    private void AddTsModel(NpgsqlDataReader reader, StringBuilder sb, string name, int index)
    {
        if (index > 0)
        {
            sb.AppendLine();
        }
        sb.AppendLine($"interface I{name} {{");
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var type = GetTsMapping(reader, i);
            var fieldName = reader.GetName(i).ToCamelCase();
            sb.Append($"{GetIdent(1)}{fieldName}?: {type}");
            if (i < reader.FieldCount - 1)
            {
                sb.AppendLine(",");
            }
            else
            {
                sb.AppendLine();
            }
        }
        sb.AppendLine("}");
    }

    private string GetTsMapping(NpgsqlDataReader reader, int i)
    {
        var type = reader.GetFieldType(i);
        if (typeof(Array) == type)
        {
            return "string[]";
        }

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Empty => "string",
            TypeCode.Object => "string",
            TypeCode.DBNull => "string",
            TypeCode.Boolean => "string",
            TypeCode.Char => "string",
            TypeCode.SByte => "string",
            TypeCode.Byte => "string",
            TypeCode.Int16 => "number",
            TypeCode.UInt16 => "number",
            TypeCode.Int32 => "number",
            TypeCode.UInt32 => "number",
            TypeCode.Int64 => "number",
            TypeCode.UInt64 => "number",
            TypeCode.Single => "number",
            TypeCode.Double => "number",
            TypeCode.Decimal => "number",
            TypeCode.DateTime => "string",
            TypeCode.String => "string",
            _ => "string",
        };
    }
    
    private string GetCsMapping(NpgsqlDataReader reader, int i)
    {
        string result;
        var type = reader.GetFieldType(i);
        var pgType = reader.GetDataTypeName(i);
        var pos = pgType.IndexOf('(');

        if (pos > -1)
        {
            pgType = pgType.Substring(0, pos);
        }
        pos = pgType.IndexOf('[');
        if (pos > -1)
        {
            pgType = pgType.Substring(0, pos);
        }
        if (settings.Mapping.TryGetValue(pgType, out var value))
        {
            result = value;
        }
        else
        {
            if (typeof(Array) == type)
            {
                result = "string";
            }
            else
            {
                result = type.Name == "String" ? type.Name.ToLowerInvariant() : type.Name;
            }
        }
        if (typeof(Array) == type)
        {
            result = $"{result}[]";
        }


        if (!Current.Value.UseNullableTypes && result == "string")
        {
            return result;
        }
        return $"{result}?";
    }
}
