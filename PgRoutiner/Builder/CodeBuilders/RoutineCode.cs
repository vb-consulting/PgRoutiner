using PgRoutiner.Builder.CodeBuilders.Models;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.Builder.CodeBuilders;

public class RoutineCode : Code
{
    private int recordModelCount = 0;
    private readonly string schema;
    private readonly string @namespace;
    private readonly IEnumerable<PgRoutineGroup> routines;
    private readonly NpgsqlConnection connection;

    public RoutineCode(
        Current settings,
        string name,
        string schema,
        string @namespace,
        IEnumerable<PgRoutineGroup> routines,
        NpgsqlConnection connection) : base(settings, name)
    {
        this.schema = schema;
        this.@namespace = @namespace;
        this.routines = routines;
        this.connection = connection;
        Build();
    }

    private void Build()
    {
        Class.AppendLine($"{I1}public static class PgRoutine{Name.ToUpperCamelCase()}");
        Class.AppendLine($"{I1}{{");
        Class.AppendLine($"{I2}public const string Name = \"{schema}.{Name}\";");

        var queryAdded = false;
        foreach (var routine in routines)
        {
            PrepareParams(routine);
            var @return = GetReturnInfo(routine);
            var @params = GetParamsInfo(routine);
            if (!queryAdded)
            {
                Class.AppendLine($"{I2}public const string Query = {BuildSelectExpression(routine, @return, @params)};");
                queryAdded = true;
            }
            if (!settings.SkipSyncMethods)
            {
                BuildSyncMethod(routine, @return, @params);
            }
            if (!settings.SkipAsyncMethods)
            {
                BuildAsyncMethod(routine, @return, @params);
            }
        }
        Class.AppendLine($"{I1}}}");
    }

    private void PrepareParams(PgRoutineGroup routine)
    {
        var i = 0;
        foreach (var p in routine.Parameters)
        {
            if (p.Name == null)
            {
                p.Name = $"param{++i}";
            }
        }
    }

    private void BuildSyncMethod(PgRoutineGroup routine, Return @return, List<Param> @params)
    {
        var name = routine.RoutineName.ToUpperCamelCase();
        var returnMethod = GetReturnMethod(routine, name);
        Class.AppendLine();
        BuildCommentHeader(routine, @return, @params, true, returnMethod);

        var returnName = !@return.Name.Contains("?") && !routine.IsSet && @return.Record.Any() ? $"{@return.Name}?" : @return.Name;
        var actualReturns = @return.IsEnumerable ? $"IEnumerable<{returnName}>" : (returnMethod == null ? $"IEnumerable<{returnName}>" : returnName);

        BuildExtensionsStart(@return, @params, name, actualReturns, false);

        Class.AppendLine($"{I3}if (connection.State != System.Data.ConnectionState.Open)");
        Class.AppendLine($"{I3}{{");
        Class.AppendLine($"{I4}connection.Open();");
        Class.AppendLine($"{I3}}}");

        if (@return.IsVoid)
        {
            Class.AppendLine($"{I3}command.ExecuteNonQuery();");
        }
        else
        {
            if (!@return.IsEnumerable)
            {
                Class.AppendLine($"{I3}using var reader = command.ExecuteReader(System.Data.CommandBehavior.SingleResult);");
                Class.AppendLine($"{I3}if (reader.Read())");
                MapReturn(routine, @return);
                Class.AppendLine($"{I3}return default;");
            }
            else
            {
                Class.AppendLine($"{I3}using var reader = command.ExecuteReader(System.Data.CommandBehavior.Default);");
                Class.AppendLine($"{I3}while (reader.Read())");
                MapReturn(routine, @return);
            }
        }

        Class.AppendLine($"{I2}}}");
        Methods.Add(new Method
        {
            Name = name,
            Namespace = @namespace,
            Description = routine.Description,
            Routine = $"{routine.Language} {routine.RoutineType} {routine.SpecificSchema}.{routine.RoutineName}({string.Join(", ", @params.Select(p => p.PgType))})",
            Params = @params,
            Returns = @return,
            ActualReturns = actualReturns,
            Sync = true
        });
    }

    private void BuildAsyncMethod(PgRoutineGroup routine, Return @return, List<Param> @params)
    {
        var name = $"{routine.RoutineName.ToUpperCamelCase()}Async";
        var returnMethod = GetReturnMethod(routine, name);
        Class.AppendLine();
        BuildCommentHeader(routine, @return, @params, false, returnMethod);

        var returnName = !@return.Name.Contains("?") && !routine.IsSet && @return.Record.Any() ? $"{@return.Name}?" : @return.Name;
        var actualReturns = @return.IsEnumerable ? $"async IAsyncEnumerable<{returnName}>" : (@return.IsVoid ? "async Task" : (returnMethod == null ? $"async IAsyncEnumerable<{returnName}>" : $"async Task<{returnName}>"));

        BuildExtensionsStart(@return, @params, name, actualReturns, true);

        Class.AppendLine($"{I3}if (connection.State != System.Data.ConnectionState.Open)");
        Class.AppendLine($"{I3}{{");
        Class.AppendLine($"{I4}await connection.OpenAsync({(settings.RoutinesCancellationToken ? "cancellationToken" : "")});");
        Class.AppendLine($"{I3}}}");

        if (@return.IsVoid)
        {
            Class.AppendLine($"{I3}await command.ExecuteNonQueryAsync({(settings.RoutinesCancellationToken ? "cancellationToken" : "")});");
        }
        else
        {
            if (!@return.IsEnumerable)
            {
                Class.AppendLine($"{I3}using var reader = await command.ExecuteReaderAsync(System.Data.CommandBehavior.SingleResult{(settings.RoutinesCancellationToken ? ", cancellationToken" : "")});");
                Class.AppendLine($"{I3}if (await reader.ReadAsync({(settings.RoutinesCancellationToken ? "cancellationToken" : "")}))");
                MapReturn(routine, @return);
                Class.AppendLine($"{I3}return default;");
            }
            else
            {
                Class.AppendLine($"{I3}using var reader = await command.ExecuteReaderAsync(System.Data.CommandBehavior.Default{(settings.RoutinesCancellationToken ? ", cancellationToken" : "")});");
                Class.AppendLine($"{I3}while (await reader.ReadAsync({(settings.RoutinesCancellationToken ? "cancellationToken" : "")}))");
                MapReturn(routine, @return);
            }
        }

        Class.AppendLine($"{I2}}}");

        Methods.Add(new Method
        {
            Name = name,
            Namespace = @namespace,
            Routine = $"{routine.Language} {routine.RoutineType} {routine.SpecificSchema}.{routine.RoutineName}({string.Join(", ", @params.Select(p => p.PgType))})",
            Description = routine.Description,
            Params = @params,
            Returns = @return,
            ActualReturns = actualReturns,
            Sync = false
        });
    }

    private void MapReturn(PgRoutineGroup routine, Return @return)
    {
        var returnExp = @return.IsEnumerable ? "yield return" : "return";
        Class.AppendLine($"{I3}{{");

        if (@return.Record.Count <= 1)
        {
            Class.AppendLine($"{I4}var value = reader.GetProviderSpecificValue(0);");
            if (@return.Name.Contains('?'))
            {
                Class.AppendLine($"{I4}{returnExp} value == DBNull.Value ? null : ({@return.Name.Replace("?", "")})value;");
            }
            else
            {
                Class.AppendLine($"{I4}{returnExp} ({@return.Name})value;");
            }
        }
        else
        {
            Class.AppendLine($"{I4}object[] values = new object[{@return.Record.Count}];");

            if (settings.UseRecords || settings.UseRecordsForModels.Contains(@return.Name))
            {
                Class.AppendLine($"{I4}reader.GetProviderSpecificValues(values);");
                Class.AppendLine($"{I4}{returnExp} new {@return.Name}(");
                Class.AppendLine(string.Join($",{NL}", routine.ModelItems.Select((m, idx) =>
                {
                    if (m.type.Contains('?'))
                    {
                        return $"{I5}values[{idx}] == DBNull.Value ? null : ({m.type.Replace("?", "")})values[{idx}]";
                    }
                    return $"{I5}({m.type})values[{idx}]";
                })));
                Class.AppendLine($"{I4});");
            }
            else
            {
                Class.AppendLine($"{I4}reader.GetProviderSpecificValues(values);");
                Class.AppendLine($"{I4}{returnExp} new {@return.Name}");
                Class.AppendLine($"{I4}{{");
                Class.AppendLine(string.Join($",{NL}", routine.ModelItems.Select((m, idx) =>
                {
                    if (m.type.Contains('?'))
                    {
                        return $"{I5}{m.name} = values[{idx}] == DBNull.Value ? null : ({m.type.Replace("?", "")})values[{idx}]";
                    }
                    return $"{I5}{m.name} = ({m.type})values[{idx}]";
                })));
                Class.AppendLine($"{I4}}};");
            }
        }
        Class.AppendLine($"{I3}}}");
    }

    private void BuildExtensionsStart(Return @return, List<Param> @params, string name, string actualReturns, bool isAsync)
    {
        Class.Append($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection");
        BuildMethodParams(@params);
        
        if (isAsync && settings.RoutinesCancellationToken)
        {
            if (@return.IsEnumerable)
            {
                Class.Append($",{NL}{I3}[EnumeratorCancellation] CancellationToken cancellationToken = default");
            }
            else
            {
                Class.Append($",{NL}{I3}CancellationToken cancellationToken = default");
            }
        }

        if (settings.RoutinesCallerInfo)
        {
            Class.Append($",{NL}{I3}[CallerMemberName] string memberName = \"\",{NL}{I3}[CallerFilePath] string sourceFilePath = \"\",{NL}{I3}[CallerLineNumber] int sourceLineNumber = 0");
        }
        Class.AppendLine(")");
        Class.AppendLine($"{I2}{{");

        Class.AppendLine($"{I3}using var command = new NpgsqlCommand(Query, connection)");
        Class.AppendLine($"{I3}{{");
        Class.AppendLine($"{I4}CommandType = System.Data.CommandType.Text,");
        if (@params.Any())
        {
            Class.AppendLine($"{I4}Parameters =");
            Class.AppendLine($"{I4}{{");
            Class.AppendLine(string.Join($",{NL}", @params.Select(p => $"{I5}new() {{ NpgsqlDbType = {p.DbType}, Value = {p.Name} == null ? DBNull.Value : {p.Name} }}")));
            Class.AppendLine($"{I4}}},");
        }
        if (settings.RoutinesUnknownReturnTypes.Contains(@return.PgName))
        {
            Class.AppendLine($"{I4}AllResultTypesAreUnknown = true");
        }
        else if (@return.Record.Any())
        {
            if (@return.Record.All(r => !r.Array && settings.RoutinesUnknownReturnTypes.Contains(r.Type) || settings.RoutinesUnknownReturnTypes.Contains(r.DataType)))
            {
                Class.AppendLine($"{I4}AllResultTypesAreUnknown = true");
            }
            else
            {
                Class.AppendLine($"{I4}UnknownResultTypeList = new bool[] {{ {string.Join(", ", @return.Record.Select(r => !r.Array && settings.RoutinesUnknownReturnTypes.Contains(r.Type) || settings.RoutinesUnknownReturnTypes.Contains(r.DataType) ? "true" : "false"))} }}");
            }
        }
        Class.AppendLine($"{I3}}};");
        if (settings.RoutinesCustomCodeLines != null && settings.RoutinesCustomCodeLines.Any())
        {
            foreach (var line in settings.RoutinesCustomCodeLines)
            {
                Class.AppendLine($"{I3}{line}");
            }
        }
    }

    private string BuildSelectExpression(PgRoutineGroup routine, Return @return, List<Param> @params)
    {
        string Select()
        {
            if (@return.IsVoid)
            {
                return "select ";
            }
            var any = @return.Record.Any();
            if (!@return.IsEnumerable && !any)
            {
                return "select ";
            }
            if (any)
            {
                return $"select {string.Join(", ", @return.Record.OrderBy(r => r.Ordinal).Select(r => r.Name))} from ";
            }
            return $"select * from ";
        }
        return $"$\"{Select()}{{Name}}({string.Join(", ", @params.Select((p, i) => $"${i + 1}"))})\"";
    }
    
    private string GetReturnMethod(PgRoutineGroup routine, string name)
    {
        if (settings.RoutinesReturnMethods.TryGetValue(routine.RoutineName, out var result))
        {
            return string.IsNullOrEmpty(result) ? null : result;
        }
        if (settings.RoutinesReturnMethods.TryGetValue(name, out result))
        {
            return string.IsNullOrEmpty(result) ? null : result;
        }
        return "";// settings.ReturnMethod;
    }

    private void BuildMethodParams(List<Param> @params)
    {
        if (@params.Count > 0)
        {
            Class.Append(", ");
            Class.Append(string.Join(", ", @params.Select(p => $"{p.Type} {p.Name}")));
        }
    }

    private void BuildCommentHeader(PgRoutineGroup routine, Return @return, List<Param> @params, bool sync, string returnMethod)
    {
        Class.AppendLine($"{I2}/// <summary>");
        Class.AppendLine($"{I2}/// {(sync ? "Executes" : "Asynchronously executes")} {routine.Language} {routine.RoutineType} {routine.SpecificSchema}.{routine.RoutineName}({string.Join(", ", @params.Select(p => p.PgType))})");
        if (!string.IsNullOrEmpty(routine.Description))
        {
            var description = routine.Description.Replace("\r\n", "\n").Trim('\n');
            Class.Append(I2);
            Class.AppendLine(string.Join($"{Environment.NewLine}{I2}",
                description.Split("\n").Select(d => $"/// {d}")));
        }
        Class.AppendLine($"{I2}/// </summary>");
        foreach (var p in @params)
        {
            Class.AppendLine($"{I2}/// <param name=\"{p.Name}\">{p.PgName} {p.PgType}</param>");
        }
        if (@return.IsEnumerable || returnMethod == null)
        {
            if (sync)
            {
                Class.AppendLine($"{I2}/// <returns>IEnumerable of {@return.Name} instances</returns>");
            }
            else
            {
                Class.AppendLine($"{I2}/// <returns>IAsyncEnumerable of {@return.Name} instances</returns>");
            }
        }
        else
        {
            if (sync)
            {
                Class.AppendLine($"{I2}/// <returns>{@return.Name}</returns>");
            }
            else
            {
                Class.AppendLine($"{I2}/// <returns>Task whose Result property is {@return.Name}</returns>");
            }
        }
    }

    private List<Param> GetParamsInfo(PgRoutineGroup routine)
    {
        return routine.Parameters.Select(p => new Param(settings)
        {
            PgName = p.Name,
            PgType = p.DataType,
            Type = GetParamType(p),
            DbType = GetParamDbType(p)
        }).ToList();
    }

    private Return GetReturnInfo(PgRoutineGroup routine)
    {
        List<PgReturns> record = connection.GetRoutineReturnsRecord(routine).ToList();
        if (record.Count == 0)
        {
            record = connection.GetRoutineReturnsTable(routine).ToList();
        }
        Return GetResult()
        {
            if (routine == null || routine.DataType == null || routine.DataType == "void")
            {
                return new Return { PgName = "void", Name = "void", IsVoid = true, IsEnumerable = routine.IsSet };
            }
            if (TryGetRoutineMapping(routine, out var result))
            {
                if (routine.DataType == "record")
                {
                    return new Return { PgName = routine.DataType, Name = $"{result}?", IsVoid = false, IsEnumerable = routine.IsSet };
                }
                if (routine.DataType == "ARRAY")
                {
                    return new Return { PgName = $"{routine.TypeUdtName}[]", Name = $"{result}[]", IsVoid = false, IsEnumerable = routine.IsSet };
                }
                if (settings.UseNullableTypes)
                {
                    return new Return { PgName = routine.DataType, Name = $"{result}?", IsVoid = false, IsEnumerable = routine.IsSet };
                }
                if (result != "string")
                {
                    return new Return { PgName = routine.DataType, Name = $"{result}?", IsVoid = false, IsEnumerable = routine.IsSet };
                }
                return new Return { PgName = routine.DataType, Name = result, IsVoid = false, IsEnumerable = routine.IsSet };
            }
            if (routine.DataType == "USER-DEFINED")
            {
                return new Return { PgName = routine.TypeUdtName, Name = BuildUserDefinedModel(routine, record), IsVoid = false, IsEnumerable = routine.IsSet };
            }
            if (routine.DataType == "record")
            {
                return new Return { PgName = routine.TypeUdtName, Name = BuildRecordModel(routine, record), IsVoid = false, IsEnumerable = routine.IsSet };
            }
            throw new ArgumentException($"Could not find mapping \"{routine.DataType}\" for return type of routine \"{routine.RoutineName}\"");
        }
        var result = GetResult();
        result.Record = record;
        if (settings.RoutinesUnknownReturnTypes.Contains(result.PgName))
        {
            result.Name = "string?";
        }

        return result;
    }

    private string BuildUserDefinedModel(PgRoutineGroup routine, List<PgReturns> items)
    {
        var name = routine.TypeUdtName.ToUpperCamelCase();
        if (settings.Mapping.TryGetValue(name, out var custom))
        {
            return custom;
        }
        if (settings.CustomModels.ContainsKey(name))
        {
            name = settings.CustomModels[name];
        }
        else if (settings.CustomModels.ContainsKey(routine.TypeUdtName))
        {
            name = settings.CustomModels[routine.TypeUdtName];
        }
        BuildModel(routine, name, items);
        UserDefinedModels.Add(name);
        //return routine.IsSet ? name : $"{name}?";
        return name;
    }

    private string BuildRecordModel(PgRoutineGroup routine, List<PgReturns> items)
    {
        var suffix = ++recordModelCount == 1 ? "" : recordModelCount.ToString();
        var name = $"{this.Name.ToUpperCamelCase()}{suffix}Result";
        if (settings.Mapping.TryGetValue(name, out var custom))
        {
            recordModelCount--;
            return custom;
        }
        if (settings.CustomModels.ContainsKey(name))
        {
            name = settings.CustomModels[name];
        }
        else if (settings.CustomModels.ContainsKey(routine.TypeUdtName))
        {
            name = settings.CustomModels[routine.TypeUdtName];
        }
        return BuildModel(routine, name, items);
    }

    private string BuildModel(PgRoutineGroup routine, string name, List<PgReturns> items)
    {
        if (Models.ContainsKey(name))
        {
            return name;
        }
        string getType(PgReturns returnModel)
        {
            if (TryGetReturnMapping(returnModel, out var result))
            {
                if (returnModel.Array)
                {
                    if (settings.UseNullableTypes)
                    {
                        return $"{result}[]?";
                    }
                    return $"{result}[]";
                }
                if (result != "string" && returnModel.Nullable)
                {
                    var key = $"{name}.{returnModel.Name.ToUpperCamelCase()}";
                    if (settings.RoutinesModelPropertyTypes.TryGetValue(key, out var value))
                    {
                        return value;
                    }
                    return $"{result}?";
                }
                if (settings.UseNullableTypes && result == "string" && returnModel.Nullable)
                {
                    return $"{result}?";
                }
                return result;
            }
            throw new ArgumentException($"Could not find mapping \"{returnModel.DataType}\" type \"{returnModel.Type}\" for result type of routine \"{this.Name}\". Consider adding a new mapping for type \"{returnModel.Type}\"");
        }
        var model = new StringBuilder();
        var modelContent = new StringBuilder();

        var columns = new List<string>();

        List<(string type, string name)> modelItems = items.Select(item => (getType(item), item.Name.ToUpperCamelCase())).ToList();
        
        if (settings.UseRecords || settings.UseRecordsForModels.Contains(name))
        {
            columns.AddRange(items.Select(i => i.Name));
            model.Append($"{I1}public record {name}(");
            modelContent.Append(string.Join(", ", modelItems.Select(item => $"{item.type} {item.name}")));
            model.Append(modelContent);
            model.AppendLine($");");
        }
        else
        {
            model.AppendLine($"{I1}public class {name}");
            model.AppendLine($"{I1}{{");
            foreach (var item in modelItems)
            {
                modelContent.AppendLine($"{I2}public {item.type} {item.name} {{ get; set; }}");
                columns.Add(item.name);
            }
            model.Append(modelContent);
            model.AppendLine($"{I1}}}");
        }
        routine.ModelItems = modelItems;

        foreach (var (key, value) in ModelContent)
        {
            if (value.Equals(modelContent))
            {
                return key;
            }
        }
        Models.Add(name, model);
        ModelContent.Add(name, modelContent);
        //return routine.IsSet ? name : $"{name}?";
        return name;
    }

    private bool TryGetRoutineMapping(PgRoutineGroup r, out string value)
    {
        if (settings.Mapping.TryGetValue(r.TypeUdtName, out value))
        {
            return true;
        }
        return settings.Mapping.TryGetValue(r.DataType, out value);
    }

    private bool TryGetReturnMapping(PgReturns r, out string value)
    {
        if (settings.Mapping.TryGetValue(r.Type, out value))
        {
            return true;
        }
        return settings.Mapping.TryGetValue(r.DataType, out value);
    }
}
