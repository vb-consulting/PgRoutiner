using System;
using System.Data;
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

        if (Current.Value.CrudReadAll != null)
        {
            CheckAtomic(connection);
            BuildCrudReadAll(connection);
        }

        if (Current.Value.CrudReadPage != null)
        {
            CheckAtomic(connection);
            BuildCrudReadPage(connection);
        }

        if (Current.Value.CrudUpdate != null)
        {
            CheckAtomic(connection);
            BuildCrudUpdate(connection);
        }

        if (Current.Value.CrudUpdateReturning != null)
        {
            CheckAtomic(connection);
            BuildCrudUpdateReturning(connection);
        }

        if (Current.Value.CrudDeleteBy != null)
        {
            CheckAtomic(connection);
            BuildCrudDeleteBy(connection);
        }

        if (Current.Value.CrudDeleteByReturning != null)
        {
            CheckAtomic(connection);
            BuildCrudDeleteByReturning(connection);
        }
    }

    public static void CheckAtomic(NpgsqlConnection connection)
    {
        /*
        if (Current.Value.CrudUseAtomic && connection.PostgreSqlVersion.Major < 15)
        {
            Program.WriteLine(ConsoleColor.Yellow, "",
                    $"WARNING: Can't create atomic functions, your PostgreSQL major version is {connection.PostgreSqlVersion.Major} and atomic functions is supported in major versions 15+ ...",
                    $"Settings {nameof(Current.Value.CrudUseAtomic)} is set to false, reverting to non-atomic functions...");

            Current.Value.CrudUseAtomic = false;
        }
        */
    }

    public static void BuildCrudCreate(NpgsqlConnection connection)
    {
        foreach (var group in connection.GetTableDefintions(Current.Value, Current.Value.CrudCreate))
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            
            var (schema, table) = group.Key;
            var (funcName, tableToken) = GetNames(schema, table, nameof(Current.Value.CrudCreate));
            
            var columns = Current.Value.CrudCoalesceDefaults ?
                group.Where(c => !c.IsGeneration && !c.IsIdentity).ToArray() :
                group.Where(c => !c.IsGeneration && !c.IsIdentity && !c.HasDefault).ToArray();

            var fullName = $"{funcName}({string.Join(", ", columns.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, columns, "void", stable: false);

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
            AddComment(sb, fullName, $"Create a new record in table {tableToken}.");

            Execute(connection, sb, fullName);
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

            var columns = Current.Value.CrudCoalesceDefaults ?
                group.Where(c => !c.IsGeneration && !c.IsIdentity).ToArray() :
                group.Where(c => !c.IsGeneration && !c.IsIdentity && !c.HasDefault).ToArray();

            var fullName = $"{funcName}({string.Join(", ", columns.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, columns, tableToken, stable: false);

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
            AddComment(sb, fullName, $"Create and return a new record in table {tableToken}.");

            Execute(connection, sb, fullName);
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

            var columns = Current.Value.CrudCoalesceDefaults ?
                group.Where(c => c.IsPk || (!c.IsGeneration && !c.IsIdentity)).ToArray() :
                group.Where(c => c.IsPk || (!c.IsGeneration && !c.IsIdentity && !c.HasDefault)).ToArray();
            var pks = columns.Where(c => c.IsPk).ToArray();

            bool overriding = columns.Where(c => c.IsIdentity).Any();
            var fullName = $"{funcName}({string.Join(", ", columns.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, columns, "void", stable: false);

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
            AddComment(sb, fullName, $"Create a new record in table {tableToken} and avoid inserting a row on key{(pks.Length > 1 ? "s" : "")} violation ({string.Join(", ", pks.Select(c => c.Name))}).");

            Execute(connection, sb, fullName);
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

            var columns = Current.Value.CrudCoalesceDefaults ?
                group.Where(c => c.IsPk || (!c.IsGeneration && !c.IsIdentity)).ToArray() :
                group.Where(c => c.IsPk || (!c.IsGeneration && !c.IsIdentity && !c.HasDefault)).ToArray();
            var pks = columns.Where(c => c.IsPk).ToArray();

            bool overriding = columns.Where(c => c.IsIdentity).Any();
            var fullName = $"{funcName}({string.Join(", ", columns.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, columns, tableToken, stable: false);

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
            AddComment(sb, fullName, $"Create and return a new record in table {tableToken} and avoid inserting a row on key{(pks.Length > 1 ? "s" : "")} violation ({string.Join(", ", pks.Select(c => c.Name))}).");

            Execute(connection, sb, fullName);
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

            var columns = Current.Value.CrudCoalesceDefaults ?
                group.Where(c => c.IsPk || (!c.IsGeneration && !c.IsIdentity)).ToArray() :
                group.Where(c => c.IsPk || (!c.IsGeneration && !c.IsIdentity && !c.HasDefault)).ToArray();
            var pks = columns.Where(c => c.IsPk).ToArray();

            bool overriding = columns.Where(c => c.IsIdentity).Any();
            var fullName = $"{funcName}({string.Join(", ", columns.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, columns, "void", stable: false);

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
            AddComment(sb, fullName, $"Create a new record in table {tableToken} and update a row with new data on key{(pks.Length > 1 ? "s" : "")} violation ({string.Join(", ", pks.Select(c => c.Name))}).");

            Execute(connection, sb, fullName);
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

            var columns = Current.Value.CrudCoalesceDefaults ?
                group.Where(c => c.IsPk || (!c.IsGeneration && !c.IsIdentity)).ToArray() :
                group.Where(c => c.IsPk || (!c.IsGeneration && !c.IsIdentity && !c.HasDefault)).ToArray();
            var pks = columns.Where(c => c.IsPk).ToArray();

            bool overriding = columns.Where(c => c.IsIdentity).Any();
            var fullName = $"{funcName}({string.Join(", ", columns.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, columns, tableToken, stable: false);

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
            AddComment(sb, fullName, $"Create and return a new record in table {tableToken} and update a row with new data on key{(pks.Length > 1 ? "s" : "")} violation ({string.Join(", ", pks.Select(c => c.Name))}).");

            Execute(connection, sb, fullName);
        }
    }

    public static void BuildCrudReadBy(NpgsqlConnection connection)
    {
        foreach (var group in connection.GetTableDefintions(Current.Value, Current.Value.CrudReadBy))
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var (schema, table) = group.Key;
            var (funcName, tableToken) = GetNames(schema, table, nameof(Current.Value.CrudReadBy));

            var columns = group.ToArray();
            var pks = columns.Where(c => c.IsPk).ToArray();

            var fullName = $"{funcName}({string.Join(", ", pks.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, pks, tableToken, stable: true);

            sb.AppendLine("select");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c => $"{I1}\"{c.Name}\"")));
            sb.AppendLine("from");
            sb.AppendLine($"{I1}{tableToken}");
            sb.AppendLine("where");
            sb.Append(string.Join($"{NL}and ", pks.Select(c => $"{I1}\"{c.Name}\" = _{c.Name}")));
            sb.AppendLine(";");

            EndFunc(sb);
            sb.AppendLine();
            AddComment(sb, fullName, $"Select and return a row from {tableToken} table by primary keys ({string.Join(" and ", pks.Select(c => c.Name))}).");

            Execute(connection, sb, fullName);
        }
    }

    public static void BuildCrudReadAll(NpgsqlConnection connection)
    {
        foreach (var group in connection.GetTableDefintions(Current.Value, Current.Value.CrudReadAll))
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var (schema, table) = group.Key;
            var (funcName, tableToken) = GetNames(schema, table, nameof(Current.Value.CrudReadAll));

            var columns = group.ToArray();
            var fullName = $"{funcName}()";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, null, $"setof {tableToken}", stable: true);

            sb.AppendLine("select");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c => $"{I1}\"{c.Name}\"")));
            sb.AppendLine("from");
            sb.AppendLine($"{I1}{tableToken};");
            
            EndFunc(sb);
            sb.AppendLine();
            AddComment(sb, fullName, $"Select all rows from {tableToken} table.");

            Execute(connection, sb, fullName);
        }
    }

    public static void BuildCrudReadPage(NpgsqlConnection connection)
    {
        foreach (var group in connection.GetTableDefintions(Current.Value, Current.Value.CrudReadPage))
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var (schema, table) = group.Key;
            var (funcName, tableToken) = GetNames(schema, table, nameof(Current.Value.CrudReadPage));

            var columns = group.ToArray();
            var parms = new PgColumnGroup[] 
            {
                new PgColumnGroup { Name = "search", Type = "varchar" },
                new PgColumnGroup { Name = "skip", Type = "integer" }, 
                new PgColumnGroup { Name = "take", Type = "integer" } 
            };
            var pks = columns.Where(c => c.IsPk).ToArray();
            var fullName = $"{funcName}({string.Join(", ", parms.Select(c => c._Type))})";
            var search = columns.Where(c => c.Type.Contains("char") || c.Type.Contains("text")).FirstOrDefault();

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, parms, "json", language: "plpgsql", forceNonAtomic: true, stable: true);
            sb.AppendLine("declare");
            sb.AppendLine($"{I1}_count bigint;");
            sb.AppendLine("begin");

            sb.AppendLine($"{I1}_search = trim(_search);");
            sb.AppendLine();
            sb.AppendLine($"{I1}if _search = '' then");
            sb.AppendLine($"{I2}_search = null;");
            sb.AppendLine($"{I1}end if;");
            sb.AppendLine();
            sb.AppendLine($"{I1}create temp table _tmp on commit drop as");
            sb.AppendLine($"{I1}select");
            sb.AppendLine(string.Join($",{NL}", pks.Select(c => $"{I2}\"{c.Name}\"")));
            sb.AppendLine($"{I1}from");
            sb.AppendLine($"{I2}{tableToken}");
            if (search == null)
            {
                sb.AppendLine("/*");
                sb.AppendLine($"{I1}where");
                sb.AppendLine($"{I2}(_search is null or search_field ilike '%' || _search || '%');");
                sb.AppendLine("*/");
            }
            else
            {
                sb.AppendLine($"{I1}where");
                sb.AppendLine($"{I2}(_search is null or {search.Name} ilike '%' || _search || '%');");
            }
            sb.AppendLine();
            sb.AppendLine($"{I1}get diagnostics _count = row_count;");
            sb.AppendLine();
            sb.AppendLine($"{I1}return json_build_object(");
            sb.AppendLine($"{I2}'count', _count,");
            sb.AppendLine($"{I2}'page', (");
            sb.AppendLine($"{I3}select json_agg(sub)");
            sb.AppendLine($"{I3}from (");
            sb.AppendLine($"{I4}select");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c => $"{I5}a.\"{c.Name}\"")));
            sb.AppendLine($"{I4}from");
            sb.AppendLine($"{I5}{tableToken} a");
            sb.Append($"{I5}inner join _tmp b on ");
            sb.AppendLine(string.Join($" and ", pks.Select(c => $"a.\"{c.Name}\" = b.\"{c.Name}\"")));
            sb.AppendLine($"{I4}limit _take");
            sb.AppendLine($"{I4}offset _skip");
            sb.AppendLine($"{I3}) sub");
            sb.AppendLine($"{I2})");
            sb.AppendLine($"{I1});");
            sb.AppendLine("end");


            EndFunc(sb, forceNonAtomic: true);
            sb.AppendLine();
            AddComment(sb, fullName, $"Search table {tableToken} and return data page and count in JSON format.");

            Execute(connection, sb, fullName);
        }
    }

    public static void BuildCrudUpdate(NpgsqlConnection connection)
    {
        foreach (var group in connection.GetTableDefintions(Current.Value, Current.Value.CrudUpdate))
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var (schema, table) = group.Key;
            var (funcName, tableToken) = GetNames(schema, table, nameof(Current.Value.CrudUpdate));

            var columns = Current.Value.CrudCoalesceDefaults ?
                group.Where(c => !c.IsGeneration && !c.IsIdentity).ToArray() :
                group.Where(c => !c.IsGeneration && !c.IsIdentity && !c.HasDefault).ToArray();
            var pks = group.Where(c => c.IsPk).ToArray();
            var parms = pks.Union(columns).ToArray();

            var fullName = $"{funcName}({string.Join(", ", parms.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, parms, "void", stable: false);

            sb.AppendLine($"update {tableToken}");
            sb.AppendLine("set");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c =>
            { 
                if (c.HasDefault)
                {
                    return $"{I1}\"{c.Name}\" = coalesce(_{c.Name}, {c.Default})";
                }
                return $"{I1}\"{c.Name}\" = _{c.Name}";
            })));
            sb.AppendLine("where");
            sb.Append(string.Join($" and {NL}", pks.Select(c => $"{I1}\"{c.Name}\" = _{c.Name}")));
            sb.AppendLine(";");

            EndFunc(sb);
            sb.AppendLine();
            AddComment(sb, fullName, $"Update record in table {tableToken}.");

            Execute(connection, sb, fullName);
        }
    }

    public static void BuildCrudUpdateReturning(NpgsqlConnection connection)
    {
        foreach (var group in connection.GetTableDefintions(Current.Value, Current.Value.CrudUpdateReturning))
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var (schema, table) = group.Key;
            var (funcName, tableToken) = GetNames(schema, table, nameof(Current.Value.CrudUpdateReturning));

            var columns = Current.Value.CrudCoalesceDefaults ?
                group.Where(c => !c.IsGeneration && !c.IsIdentity).ToArray() :
                group.Where(c => !c.IsGeneration && !c.IsIdentity && !c.HasDefault).ToArray();
            var pks = group.Where(c => c.IsPk).ToArray();
            var parms = pks.Union(columns).ToArray();

            var fullName = $"{funcName}({string.Join(", ", parms.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, parms, tableToken, stable: false);

            sb.AppendLine($"update {tableToken}");
            sb.AppendLine("set");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c =>
            {
                if (c.HasDefault)
                {
                    return $"{I1}\"{c.Name}\" = coalesce(_{c.Name}, {c.Default})";
                }
                return $"{I1}\"{c.Name}\" = _{c.Name}";
            })));
            sb.AppendLine("where");
            sb.AppendLine(string.Join($" and {NL}", pks.Select(c => $"{I1}\"{c.Name}\" = _{c.Name}")));
            sb.AppendLine("returning ");
            sb.Append(string.Join($",{NL}", group.Select(c => $"{I1}\"{c.Name}\"")));
            sb.AppendLine(";");
            
            EndFunc(sb);
            sb.AppendLine();
            AddComment(sb, fullName, $"Update and return record in table {tableToken}.");

            Execute(connection, sb, fullName);
        }
    }

    public static void BuildCrudDeleteBy(NpgsqlConnection connection)
    {
        foreach (var group in connection.GetTableDefintions(Current.Value, Current.Value.CrudDeleteBy))
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var (schema, table) = group.Key;
            var (funcName, tableToken) = GetNames(schema, table, nameof(Current.Value.CrudDeleteBy));

            var pks = group.Where(c => c.IsPk).ToArray();

            var fullName = $"{funcName}({string.Join(", ", pks.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, pks, "void", stable: false);

            sb.AppendLine("delete");
            sb.AppendLine("from");
            sb.AppendLine($"{I1}{tableToken}");
            sb.AppendLine("where");
            sb.Append(string.Join($"{NL}and ", pks.Select(c => $"{I1}\"{c.Name}\" = _{c.Name}")));
            sb.AppendLine(";");

            EndFunc(sb);
            sb.AppendLine();
            AddComment(sb, fullName, $"Delete row from {tableToken} table by primary keys ({string.Join(" and ", pks.Select(c => c.Name))}).");

            Execute(connection, sb, fullName);
        }
    }

    public static void BuildCrudDeleteByReturning(NpgsqlConnection connection)
    {
        foreach (var group in connection.GetTableDefintions(Current.Value, Current.Value.CrudDeleteByReturning))
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var (schema, table) = group.Key;
            var (funcName, tableToken) = GetNames(schema, table, nameof(Current.Value.CrudDeleteByReturning));

            var pks = group.Where(c => c.IsPk).ToArray();

            var fullName = $"{funcName}({string.Join(", ", pks.Select(c => c._Type))})";

            BuildHeader(sb, fullName);
            BeginFunc(sb, funcName, pks, "void", stable: false);

            sb.AppendLine("delete");
            sb.AppendLine("from");
            sb.AppendLine($"{I1}{tableToken}");
            sb.AppendLine("where");
            sb.AppendLine(string.Join($"{NL}and ", pks.Select(c => $"{I1}\"{c.Name}\" = _{c.Name}")));
            sb.AppendLine("returning ");
            sb.Append(string.Join($",{NL}", group.Select(c => $"{I1}\"{c.Name}\"")));
            sb.AppendLine(";");

            EndFunc(sb);
            sb.AppendLine();
            AddComment(sb, fullName, $"Delete and return a row from {tableToken} table by primary keys ({string.Join(" and ", pks.Select(c => c.Name))}).");

            Execute(connection, sb, fullName);
        }
    }

    private static void Execute(NpgsqlConnection connection, StringBuilder sb, string funcName)
    {
        var content = sb.ToString();

        if (Current.Value.DumpConsole)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(content);
            Console.ResetColor();
        }
        else
        {
            var quote = "$__script__$";
            var script = string.Concat($"do {quote} begin", NL, content, NL, $"end {quote};");
            
            try
            {
                connection.Execute(script);
                if (!Current.Value.Silent)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("create function {0};", funcName);
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: Error trying to recreate function: {funcName}");
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(script);
                Console.WriteLine();
                Console.ResetColor();
            }
        }
    }

    private static void AddComment(StringBuilder sb, string fullName, string comment)
    {
        sb.AppendLine($"comment on function {fullName} is '{comment}';");
    }

    private static void EndFunc(StringBuilder sb, bool forceNonAtomic = false)
    {
        /*
        if (Current.Value.CrudUseAtomic && !forceNonAtomic)
        {
            sb.AppendLine("end;");
        }
        else 
        { 
            sb.AppendLine("$$;"); 
        }
        */
        sb.AppendLine("$$;");
    }

    private static void BeginFunc(StringBuilder sb, string funcName, PgColumnGroup[] columns, string returns, 
        string language = "sql", bool forceNonAtomic = false, bool stable = false)
    {
        if (columns == null)
        {
            sb.AppendLine($"create function {funcName}()");
        }
        else
        {
            sb.AppendLine($"create function {funcName}(");
            sb.AppendLine(string.Join($",{NL}", columns.Select(c => $"{I1}_{c.Name} {c._Type}")));
            sb.AppendLine(")");
        }
        sb.AppendLine($"returns {returns}");
        sb.AppendLine($"language {language}");

        if (stable)
        {
            sb.AppendLine("stable");
        }
        else
        {
            sb.AppendLine("volatile");
        }
        if (Current.Value.CrudFunctionAttributes != null)
        {
            sb.AppendLine(Current.Value.CrudFunctionAttributes);
        }

        /*
        if (Current.Value.CrudUseAtomic && !forceNonAtomic)
        {
            sb.AppendLine("begin atomic");
        }
        else
        {
            sb.AppendLine("as $$");
        }
        */
        sb.AppendLine("as $$");
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
            string.Format(Current.Value.CrudNamePattern, 
                schemaToken, 
                table, 
                key.Replace("Crud", "").ToKebabCase().Replace("-", "_")),
            $"{schemaToken}\"{table}\""
        );
    }
}
