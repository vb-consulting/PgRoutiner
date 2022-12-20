using System;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Xml.Linq;
using PgRoutiner.Builder.CodeBuilders;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.Builder;

public class Crud
{
    private static string Ident(int times)
    {
        return string.Join("", Enumerable.Repeat(" ", Current.Value.Ident * times));
    }
    private static string I1 => Ident(1);
    private static string I2 => Ident(2);
    private static string I3 => Ident(3);
    private static string I4 => Ident(4);
    private static string I5 => Ident(5);

    private static string NL = Environment.NewLine;


    public static void BuildCrudRoutines(NpgsqlConnection connection)
    {
        if (Current.Value.CrudCreate != null)
        {
            CheckAtomic(connection);
            BuildCrudCreate(connection);
        }

        if (Current.Value.CrudCreateReturning != null)
        {
            CheckAtomic(connection);
            BuildCrudCreateReturning(connection);
        }

        if (Current.Value.CrudCreateOnConflictDoNothing != null)
        {
            CheckAtomic(connection);
            BuildCrudCreateOnConflictDoNothing(connection);
        }

        if (Current.Value.CrudCreateOnConflictDoNothingReturning != null)
        {
            CheckAtomic(connection);
            BuildCrudCreateOnConflictDoNothingReturning(connection);
        }

        if (Current.Value.CrudCreateOnConflictDoUpdate != null)
        {
            CheckAtomic(connection);
            BuildCrudCreateOnConflictDoUpdate(connection);
        }

        if (Current.Value.CrudCreateOnConflictDoUpdateReturning != null)
        {
            CheckAtomic(connection);
            BuildCrudCreateOnConflictDoUpdateReturning(connection);
        }

        if (Current.Value.CrudReadBy != null)
        {
            CheckAtomic(connection);
            BuildCrudReadBy(connection);
        }
    }

    public static void CheckAtomic(NpgsqlConnection connection)
    {
        if (Current.Value.CrudUseAtomic && connection.PostgreSqlVersion.Major < 15)
        {
            Program.WriteLine(ConsoleColor.Yellow, "",
                    $"WARNING: Can't create atomic functions, your PostgreSQL major version is {connection.PostgreSqlVersion.Major} and atomic functions is supported in major versions 15+ ...",
                    $"Settings {nameof(Current.Value.CrudUseAtomic)} is set to false, reverting to non-atomic functions...");

            Current.Value.CrudUseAtomic = false;
        }
    }

    public static void BuildCrudCreate(NpgsqlConnection connection)
    {
        foreach (var group in connection.GetTableDefintions(Current.Value, Current.Value.CrudCreate))
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            
            var (schema, table) = group.Key;
            var (funcName, tableToken) = GetNames(schema, table, nameof(Current.Value.CrudCreate));
            
            var columns = Current.Value.CrudCreateDefaults ?
                group.Where(c => !c.IsGeneration && !c.IsIdentity).ToArray() :
                group.Where(c => !c.IsGeneration && !c.IsIdentity && !c.HasDefault).ToArray();

            var fullName = $"{funcName}({string.Join(", ", columns.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, columns, "void");

            sb.AppendLine($"insert into {tableToken}");
            sb.AppendLine("(");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c => $"{I1}\"{c.Name}\"")));
            sb.AppendLine(")");
            sb.AppendLine("values");
            sb.AppendLine("(");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c =>
            {
                if (c.HasDefault)
                {
                    return $"{I1}coalesce(_{c.Name}, {c.Default})";
                }
                return $"{I1}_{c.Name}";
            })));
            sb.AppendLine(");");
            
            EndFunc(sb);
            sb.AppendLine();
            AddComment(sb, fullName, $"Create new record in table {tableToken}.");

            Execute(connection, sb);
        }
    }
    
    public static void BuildCrudCreateReturning(NpgsqlConnection connection)
    {
        foreach (var group in connection.GetTableDefintions(Current.Value, Current.Value.CrudCreateReturning))
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var (schema, table) = group.Key;
            var (funcName, tableToken) = GetNames(schema, table, nameof(Current.Value.CrudCreateReturning));

            var columns = Current.Value.CrudCreateDefaults ?
                group.Where(c => !c.IsGeneration && !c.IsIdentity).ToArray() :
                group.Where(c => !c.IsGeneration && !c.IsIdentity && !c.HasDefault).ToArray();

            var fullName = $"{funcName}({string.Join(", ", columns.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, columns, $"setof {tableToken}");

            sb.AppendLine($"insert into {tableToken}");
            sb.AppendLine("(");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c => $"{I1}\"{c.Name}\"")));
            sb.AppendLine(")");
            sb.AppendLine("values");
            sb.AppendLine("(");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c =>
            {
                if (c.HasDefault)
                {
                    return $"{I1}coalesce(_{c.Name}, {c.Default})";
                }
                return $"{I1}_{c.Name}";
            })));
            sb.AppendLine(")");
            sb.AppendLine("returning ");
            sb.Append(string.Join($",{NL}", group.Select(c => $"{I1}\"{c.Name}\"")));
            sb.AppendLine(";");
            EndFunc(sb);
            sb.AppendLine();
            AddComment(sb, fullName, $"Create and return new record in table {tableToken}.");

            Execute(connection, sb);
        }
    }

    public static void BuildCrudCreateOnConflictDoNothing(NpgsqlConnection connection)
    {
        foreach (var group in connection.GetTableDefintions(Current.Value, Current.Value.CrudCreateOnConflictDoNothing))
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var (schema, table) = group.Key;
            var (funcName, tableToken) = GetNames(schema, table, nameof(Current.Value.CrudCreateOnConflictDoNothing));

            if (!group.Where(c => c.IsPk).Any())
            {
                Program.WriteLine(ConsoleColor.Yellow, "", 
                    $"WARNING: Table {tableToken} doesn't seem to contain any primary keys. Function {funcName} could not be generated");
                continue;
            }

            var columns = Current.Value.CrudCreateDefaults ?
                group.Where(c => c.IsPk || (!c.IsGeneration && !c.IsIdentity)).ToArray() :
                group.Where(c => c.IsPk || (!c.IsGeneration && !c.IsIdentity && !c.HasDefault)).ToArray();
            var pks = columns.Where(c => c.IsPk).ToArray();

            bool overriding = columns.Where(c => c.IsIdentity).Any();
            var fullName = $"{funcName}({string.Join(", ", columns.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, columns, "void");

            sb.AppendLine($"insert into {tableToken}");
            sb.AppendLine("(");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c => $"{I1}\"{c.Name}\"")));
            sb.AppendLine(")");
            sb.AppendLine($"{(overriding ? "overriding system value " : "")}values");
            sb.AppendLine("(");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c =>
            {
                if (c.HasDefault)
                {
                    return $"{I1}coalesce(_{c.Name}, {c.Default})";
                }
                return $"{I1}_{c.Name}";
            })));
            sb.AppendLine(")");
            sb.AppendLine($"on conflict ({string.Join(", ", pks.Select(c => $"\"{c.Name}\""))}) do nothing;");

            EndFunc(sb);
            sb.AppendLine();
            AddComment(sb, fullName, $"Create new record in table {tableToken} and avoid inserting a row on key{(pks.Length > 1 ? "s" : "")} violation ({string.Join(", ", pks.Select(c => c.Name))}).");

            Execute(connection, sb);
        }
    }

    public static void BuildCrudCreateOnConflictDoNothingReturning(NpgsqlConnection connection)
    {
        foreach (var group in connection.GetTableDefintions(Current.Value, Current.Value.CrudCreateOnConflictDoNothingReturning))
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var (schema, table) = group.Key;
            var (funcName, tableToken) = GetNames(schema, table, nameof(Current.Value.CrudCreateOnConflictDoNothingReturning));

            if (!group.Where(c => c.IsPk).Any())
            {
                Program.WriteLine(ConsoleColor.Yellow, "",
                    $"WARNING: Table {tableToken} doesn't seem to contain any primary keys. Function {funcName} could not be generated");
                continue;
            }

            var columns = Current.Value.CrudCreateDefaults ?
                group.Where(c => c.IsPk || (!c.IsGeneration && !c.IsIdentity)).ToArray() :
                group.Where(c => c.IsPk || (!c.IsGeneration && !c.IsIdentity && !c.HasDefault)).ToArray();
            var pks = columns.Where(c => c.IsPk).ToArray();

            bool overriding = columns.Where(c => c.IsIdentity).Any();
            var fullName = $"{funcName}({string.Join(", ", columns.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, columns, $"setof {tableToken}");

            sb.AppendLine($"insert into {tableToken}");
            sb.AppendLine("(");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c => $"{I1}\"{c.Name}\"")));
            sb.AppendLine(")");
            sb.AppendLine($"{(overriding ? "overriding system value " : "")}values");
            sb.AppendLine("(");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c =>
            {
                if (c.HasDefault)
                {
                    return $"{I1}coalesce(_{c.Name}, {c.Default})";
                }
                return $"{I1}_{c.Name}";
            })));
            sb.AppendLine(")");
            sb.AppendLine($"on conflict ({string.Join(", ", pks.Select(c => $"\"{c.Name}\""))}) do nothing");

            sb.AppendLine("returning ");
            sb.Append(string.Join($",{NL}", group.Select(c => $"{I1}\"{c.Name}\"")));
            sb.AppendLine(";");

            EndFunc(sb);
            sb.AppendLine();
            AddComment(sb, fullName, $"Create and return new record in table {tableToken} and avoid inserting a row on key{(pks.Length > 1 ? "s" : "")} violation ({string.Join(", ", pks.Select(c => c.Name))}).");

            Execute(connection, sb);
        }
    }

    public static void BuildCrudCreateOnConflictDoUpdate(NpgsqlConnection connection)
    {
        foreach (var group in connection.GetTableDefintions(Current.Value, Current.Value.CrudCreateOnConflictDoUpdate))
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var (schema, table) = group.Key;
            var (funcName, tableToken) = GetNames(schema, table, nameof(Current.Value.CrudCreateOnConflictDoUpdate));

            if (!group.Where(c => c.IsPk).Any())
            {
                Program.WriteLine(ConsoleColor.Yellow, "",
                    $"WARNING: Table {tableToken} doesn't seem to contain any primary keys. Function {funcName} could not be generated");
                continue;
            }

            var columns = Current.Value.CrudCreateDefaults ?
                group.Where(c => c.IsPk || (!c.IsGeneration && !c.IsIdentity)).ToArray() :
                group.Where(c => c.IsPk || (!c.IsGeneration && !c.IsIdentity && !c.HasDefault)).ToArray();
            var pks = columns.Where(c => c.IsPk).ToArray();

            bool overriding = columns.Where(c => c.IsIdentity).Any();
            var fullName = $"{funcName}({string.Join(", ", columns.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, columns, "void");

            sb.AppendLine($"insert into {tableToken}");
            sb.AppendLine("(");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c => $"{I1}\"{c.Name}\"")));
            sb.AppendLine(")");
            sb.AppendLine($"{(overriding ? "overriding system value " : "")}values");
            sb.AppendLine("(");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c =>
            {
                if (c.HasDefault)
                {
                    return $"{I1}coalesce(_{c.Name}, {c.Default})";
                }
                return $"{I1}_{c.Name}";
            })));
            sb.AppendLine(")");
            sb.AppendLine($"on conflict ({string.Join(", ", pks.Select(c => $"\"{c.Name}\""))}) do update set");

            sb.Append(string.Join($",{NL}", columns.Where(c => !c.IsIdentity).Select(c => $"{I1}\"{c.Name}\" = EXCLUDED.\"{c.Name}\"")));
            sb.AppendLine(";");

            EndFunc(sb);
            sb.AppendLine();
            AddComment(sb, fullName, $"Create new record in table {tableToken} and update row with new data on key{(pks.Length > 1 ? "s" : "")} violation ({string.Join(", ", pks.Select(c => c.Name))}).");

            Execute(connection, sb);
        }
    }

    public static void BuildCrudCreateOnConflictDoUpdateReturning(NpgsqlConnection connection)
    {
        foreach (var group in connection.GetTableDefintions(Current.Value, Current.Value.CrudCreateOnConflictDoUpdateReturning))
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var (schema, table) = group.Key;
            var (funcName, tableToken) = GetNames(schema, table, nameof(Current.Value.CrudCreateOnConflictDoUpdateReturning));

            if (!group.Where(c => c.IsPk).Any())
            {
                Program.WriteLine(ConsoleColor.Yellow, "",
                    $"WARNING: Table {tableToken} doesn't seem to contain any primary keys. Function {funcName} could not be generated");
                continue;
            }

            var columns = Current.Value.CrudCreateDefaults ?
                group.Where(c => c.IsPk || (!c.IsGeneration && !c.IsIdentity)).ToArray() :
                group.Where(c => c.IsPk || (!c.IsGeneration && !c.IsIdentity && !c.HasDefault)).ToArray();
            var pks = columns.Where(c => c.IsPk).ToArray();

            bool overriding = columns.Where(c => c.IsIdentity).Any();
            var fullName = $"{funcName}({string.Join(", ", columns.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, columns, "void");

            sb.AppendLine($"insert into {tableToken}");
            sb.AppendLine("(");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c => $"{I1}\"{c.Name}\"")));
            sb.AppendLine(")");
            sb.AppendLine($"{(overriding ? "overriding system value " : "")}values");
            sb.AppendLine("(");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c =>
            {
                if (c.HasDefault)
                {
                    return $"{I1}coalesce(_{c.Name}, {c.Default})";
                }
                return $"{I1}_{c.Name}";
            })));
            sb.AppendLine(")");
            sb.AppendLine($"on conflict ({string.Join(", ", pks.Select(c => $"\"{c.Name}\""))}) do update set");

            sb.AppendLine(string.Join($",{NL}", columns.Where(c => !c.IsIdentity).Select(c => $"{I1}\"{c.Name}\" = EXCLUDED.\"{c.Name}\"")));

            sb.AppendLine("returning ");
            sb.Append(string.Join($",{NL}", group.Select(c => $"{I1}\"{c.Name}\"")));
            sb.AppendLine(";");

            EndFunc(sb);
            sb.AppendLine();
            AddComment(sb, fullName, $"Create and return new record in table {tableToken} and update row with new data on key{(pks.Length > 1 ? "s" : "")} violation ({string.Join(", ", pks.Select(c => c.Name))}).");

            Execute(connection, sb);
        }
    }

    public static void BuildCrudReadBy(NpgsqlConnection connection)
    {
        //foreach (var group in connection.GetTableDefintions(Current.Value, Current.Value.CrudCreate))
        //{
        //    var sb = new StringBuilder();
        //    sb.AppendLine();

        //    var (schema, table) = group.Key;
        //    var (funcName, tableToken) = GetNames(schema, table, nameof(Current.Value.CrudCreate));

        //    var columns = Current.Value.CrudCreateDefaults ?
        //        group.Where(c => !c.IsGeneration && !c.IsIdentity).ToArray() :
        //        group.Where(c => !c.IsGeneration && !c.IsIdentity && !c.HasDefault).ToArray();

        //    var fullName = $"{funcName}({string.Join(", ", columns.Select(c => c._Type))})";

        //    BuildHeader(sb, fullName);
        //    BeginFunc(sb, funcName, columns, "void");

        //    sb.AppendLine($"insert into {tableToken}");
        //    sb.AppendLine("(");
        //    sb.AppendLine(string.Join($",{NL}", columns.Select(c => $"{I1}\"{c.Name}\"")));
        //    sb.AppendLine(")");
        //    sb.AppendLine("values");
        //    sb.AppendLine("(");
        //    sb.AppendLine(string.Join($",{NL}", columns.Select(c =>
        //    {
        //        if (c.HasDefault)
        //        {
        //            return $"{I1}coalesce(_{c.Name}, {c.Default})";
        //        }
        //        return $"{I1}_{c.Name}";
        //    })));
        //    sb.AppendLine(");");

        //    EndFunc(sb);
        //    sb.AppendLine();
        //    AddComment(sb, fullName, $"Create new record in table {tableToken}.");

        //    Execute(connection, sb);
        //}
    }

    private static void Execute(NpgsqlConnection connection, StringBuilder sb)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(sb.ToString());
        Console.ResetColor();
    }

    private static void AddComment(StringBuilder sb, string fullName, string comment)
    {
        sb.AppendLine($"comment on function {fullName} is '{comment}';");
    }

    private static void EndFunc(StringBuilder sb)
    {
        if (Current.Value.CrudUseAtomic)
        {
            sb.AppendLine("end;");
        }
        else 
        { 
            sb.AppendLine("$$;"); 
        }
    }

    private static void BeginFunc(StringBuilder sb, string funcName, PgColumnGroup[] columns, string returns)
    {
        sb.AppendLine($"create function {funcName}(");
        sb.AppendLine(string.Join($",{NL}", columns.Select(c => $"{I1}_{c.Name} {c._Type}")));
        sb.AppendLine(")");
        sb.AppendLine($"returns {returns}");
        sb.AppendLine("language sql");
        if (Current.Value.CrudUseAtomic)
        {
            sb.AppendLine("begin atomic");
        }
        else
        {
            sb.AppendLine("as $$");
        }
    }

    private static void BuildHeader(StringBuilder sb, string fullName)
    {
        sb.AppendLine($"drop function if exists {fullName};");
        sb.AppendLine();
        sb.AppendLine($"-- ");
        sb.AppendLine($"-- function {fullName}");
        sb.AppendLine($"-- ");
    }

    private static (string funcName, string tableToken) GetNames(string schema, string table, string key)
    {
        var schemaToken = schema == "public" ? "" : $"\"{schema}\".";
        return (
            string.Format(Current.Value.CrudNamePattern, schemaToken, table, key.Replace("Crud", "").ToKebabCase().Replace("-", "_")),
            $"{schemaToken}\"{table}\""
        );
    }
}
