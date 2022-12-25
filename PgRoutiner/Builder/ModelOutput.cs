using System;
using System.Data;
using System.Reflection.Metadata;
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
        if (string.IsNullOrEmpty(Current.Value.ModelOutput))
        {
            return;
        }
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        var (queries, ts) = GetInfo(connectionName);

        var modelOutputFile = string.Equals(Current.Value.ModelOutputFile?.ToLowerInvariant(), ".ts") ? null : Current.Value.ModelOutputFile;
        var builder = new ModelOutput(modelOutputFile);
        var header = builder.GetHeader(ts, connectionName);
        var footer = builder.GetFooter(ts);
        var sb = new StringBuilder();

        sb.Append(header);
        int modelCount = 0;
        
        foreach(var (content, name, schema) in builder.BuilModelOutputContent(connection, queries, ts))
        {
            if (content != null)
            {
                if (modelCount > 0)
                {
                    sb.AppendLine();
                }
                sb.Append(content);
                modelCount++;

                if (Current.Value.ModelSaveToModelDir)
                {
                    var modelSb = new StringBuilder();
                    modelSb.Append(header);
                    modelSb.Append(content);
                    modelSb.Append(footer);
                    var fileName = Path.Combine(Current.Value.ModelDir ?? Program.CurrentDir, 
                        ts ? $"{name.ToUpperCamelCase()}.ts" : $"{name.ToUpperCamelCase()}.cs");
                    SaveToFile(fileName, connectionName, modelSb.ToString(), schema);
                }
            }
        }
        sb.Append(footer);

        if (modelOutputFile == null && Current.Value.ModelSaveToModelDir == false)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(sb.ToString());
            Console.ResetColor();
        }

        if (modelOutputFile != null)
        {
            SaveToFile(modelOutputFile, connectionName, sb.ToString());
        }
    }

    private static void SaveToFile(string fileName, string connectionName, string content, string schema = null)
    {
        var file = schema == null ?
            string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, fileName)), connectionName) :
            string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, fileName)), schema == "public" ? "" : schema.ToUpperCamelCase())
                .Replace("//", "/")
                .Replace("\\\\", "\\");

        Writer.CreateFile(file, content);
    }

    private static (string queries, bool ts) GetInfo(string connectionName)
    {
        var file = string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, Current.Value.ModelOutput)), connectionName);
        string queries = File.Exists(file) ? File.ReadAllText(file) : Current.Value.ModelOutput;
        var ts = Current.Value.ModelOutputFile?.ToLowerInvariant()?.EndsWith(".ts") ?? false;

        return (queries, ts);
    }

    private string GetHeader(bool ts, string connectionName)
    {
        var sb = new StringBuilder();

        foreach (var line in Current.Value.SourceHeaderLines)
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
        return sb.ToString();
    }

    private string GetFooter(bool ts)
    {
        if (!ts && !Current.Value.UseFileScopedNamespaces)
        {
            return "}";
        }
        return "";
    }

    private IEnumerable<(string content, string name, string schema)> BuilModelOutputContent(NpgsqlConnection connection, string queries, bool ts)
    {
        var entries = queries.Split(";", StringSplitOptions.RemoveEmptyEntries);
        var singleWordHashes = entries.Where(e => !e.Contains(" ")).ToHashSet();
        var enums = connection.GetAllEnums().ToDictionary(k => $"{k.schema}.{k.name}", v => v);

        foreach (var enumInfo in enums.Values)
        {
            if (singleWordHashes.Contains(enumInfo.name) || singleWordHashes.Contains($"{enumInfo.name}.{enumInfo.schema}"))
            {
                yield return (BuildEnumModelContent(enumInfo, ts), enumInfo.name, enumInfo.schema);
            }
        }

        foreach (var entry in entries)
        {
            var query = entry.Trim();
            var singleWord = !query.Contains(" ");
            string name, schema, content;

            if (singleWord)
            {
                name = query;
                query = $"select * from {query} limit 1";
            }
            else
            {
                name = query.GetFrom();
            }
            if (string.IsNullOrEmpty(name))
            {
                Program.WriteLine(ConsoleColor.Red, $"ERROR: Following query might be malformed. Skipping...{NL}Query: {query}");
            }

            if (name.Contains("."))
            {
                var parts = name.Split(".");
                schema = parts[0];
                name = parts[1];
            }
            else
            {
                schema = "public";
            }
            if (enums.TryGetValue($"{schema}.{name}", out var enumInfo))
            {
                //yield return (BuildEnumModelContent(enumInfo, ts), name, schema);
                continue;
            }

            content = BuildModelContent(connection, query, name, ts);
            yield return (content, name, schema);
        }
    }

    private string BuildEnumModelContent((string schema, string name, string[] values, string comment) enumInfo, bool ts)
    {
        var name = enumInfo.name.ToUpperCamelCase();
        StringBuilder sb = new();
        if (!ts)
        {
            sb.AppendLine($"{I1}public enum {name}");
            sb.AppendLine($"{I1}{{");
            sb.AppendLine(string.Join($",{NL}", enumInfo.values.Select(v => $"{I2}{v}")));
            sb.AppendLine($"{I1}}}");
        }
        else
        {
            sb.AppendLine($"type {name} = ");
            sb.Append(string.Join($" |{NL}", enumInfo.values.Select(v => $"{GetIdent(1)}\"{v}\"")));
            sb.AppendLine(";");
        }
        return sb.ToString();
    }

    private string BuildModelContent(NpgsqlConnection connection, string query, string name, bool ts)
    {
        name = name.ToUpperCamelCase();
        StringBuilder sb = new();
        using var command = connection.CreateCommand();
        command.CommandText = query;
        try
        {
            using var reader = command.ExecuteReader(CommandBehavior.SingleRow);
            reader.Read();
            if (!ts)
            {
                if (!Current.Value.UseRecords)
                {
                    AddCsClass(reader, sb, name);
                }
                else
                {
                    AddCsRecord(reader, sb, name);
                }
            }
            else
            {
                AddTsModel(reader, sb, name);
            }
            return sb.ToString();
        }
        catch (Exception exc)
        {
            Program.WriteLine(ConsoleColor.Red, $"ERROR: Following query raised error. Skipping...{NL}Query: {query}{NL}Error: {exc.Message}");
            return null;
        }
    }

    private void AddCsClass(NpgsqlDataReader reader, StringBuilder sb, string name)
    {
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
    private void AddCsRecord(NpgsqlDataReader reader, StringBuilder sb, string name)
    {
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

    private void AddTsModel(NpgsqlDataReader reader, StringBuilder sb, string name)
    {
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
