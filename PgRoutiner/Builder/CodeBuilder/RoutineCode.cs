using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;

namespace PgRoutiner
{
    public partial class RoutineCode : Code
    {
        private int recordModelCount = 0;
        private readonly string @namespace;
        private readonly IEnumerable<PgRoutineGroup> routines;
        private readonly NpgsqlConnection connection;
 
        public RoutineCode(
            Settings settings, 
            string name,
            string @namespace,
            IEnumerable<PgRoutineGroup> routines,
            NpgsqlConnection connection) : base(settings, name)
        {
            this.@namespace = @namespace;
            this.routines = routines;
            this.connection = connection;
            Build();
        }

        private void Build()
        {
            Class.AppendLine($"{I1}public static class PgRoutine{Name.ToUpperCamelCase()}");
            Class.AppendLine($"{I1}{{");
            Class.AppendLine($"{I2}public const string Name = \"{Name}\";");
            foreach (var routine in routines)
            {
                PrepareParams(routine);
                var @return = GetReturnInfo(routine);
                var @params = GetParamsInfo(routine);
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
            foreach(var p in routine.Parameters)
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
            Class.AppendLine();
            BuildCommentHeader(routine, @return, @params, true);
            var actualReturns = @return.IsInstance ? $"IEnumerable<{@return.Name}>" : @return.Name;
            void AddMethod() => Methods.Add(new Method(name, @namespace, @params, @return, actualReturns, true));

            void AddBodyCode(string bodyTab, string paramsTab)
            {
                Class.AppendLine($"{bodyTab}.AsProcedure()");
                if (@return.IsVoid)
                {
                    Class.Append($"{bodyTab}.Execute(Name");
                    if (@params.Count == 0)
                    {
                        Class.AppendLine(");");
                        AddMethod();
                        return;
                    }
                    else
                    {
                        Class.AppendLine(",");
                    }
                }
                else
                {
                    Class.Append($"{bodyTab}.Read<{@return.Name}>(Name");
                    if (@params.Count > 0)
                    {
                        Class.AppendLine(",");
                    }
                }
                BuildParams(@params, paramsTab);
                if (@return.IsVoid || @return.IsInstance)
                {
                    Class.AppendLine(");");
                    return;
                }
                Class.AppendLine(")");
                Class.AppendLine($"{bodyTab}.Single();");
            }

            if (settings.UseExpressionBody)
            {
                Class.Append($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection");
                BuildMethodParams(@params);
                Class.AppendLine(") => connection");
                AddBodyCode(I3, I4);
            }
            else
            {
                Class.Append($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection");
                BuildMethodParams(@params);
                Class.AppendLine(")");
                Class.AppendLine($"{I2}{{");
                if (@return.IsVoid)
                {
                    Class.AppendLine($"{I3}connection");
                }
                else
                {
                    Class.AppendLine($"{I3}return connection");
                }
                AddBodyCode(I4, I5);
                Class.AppendLine($"{I2}}}");
            }
            AddMethod();
        }

        private void BuildAsyncMethod(PgRoutineGroup routine, Return @return, List<Param> @params)
        {
            var name = $"{routine.RoutineName.ToUpperCamelCase()}Async";
            Class.AppendLine();
            BuildCommentHeader(routine, @return, @params, false);
            var actualReturns = @return.IsInstance ? $"IAsyncEnumerable<{@return.Name}>" : (@return.IsVoid ? "async ValueTask" : $"async ValueTask<{@return.Name}>");
            void AddMethod() => Methods.Add(new Method(name, @namespace, @params, @return, actualReturns, false));

            void AddBodyCode(string bodyTab, string paramsTab)
            {
                Class.AppendLine($"{bodyTab}.AsProcedure()");
                if (@return.IsVoid)
                {
                    Class.Append($"{bodyTab}.ExecuteAsync(Name");
                    if (@params.Count == 0)
                    {
                        Class.AppendLine(");");
                        AddMethod();
                        return;
                    }
                    else
                    {
                        Class.AppendLine(",");
                    }
                }
                else
                {
                    Class.Append($"{bodyTab}.ReadAsync<{@return.Name}>(Name");
                    if (@params.Count > 0)
                    {
                        Class.AppendLine(",");
                    }
                }
                BuildParams(@params, paramsTab);
                if (@return.IsVoid || @return.IsInstance)
                {
                    Class.AppendLine(");");

                }
                else
                {
                    Class.AppendLine(")");
                    Class.AppendLine($"{bodyTab}.SingleAsync();");
                }
            }

            if (settings.UseExpressionBody)
            {
                Class.Append($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection");
                BuildMethodParams(@params);
                Class.AppendLine(@return.IsInstance ? ") => connection" : ") => await connection");

                AddBodyCode(I3, I4);
            }
            else
            {
                Class.Append($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection");
                BuildMethodParams(@params);
                Class.AppendLine(")");
                Class.AppendLine($"{I2}{{");
                if (@return.IsVoid)
                {
                    Class.AppendLine(@return.IsInstance ? $"{I3}connection" : $"{I3}await connection");
                }
                else
                {
                    Class.AppendLine(@return.IsInstance ? $"{I3}return connection" : $"{I3}return await connection");
                }
                AddBodyCode(I4, I5);
                Class.AppendLine($"{I2}}}");
            }
            
            AddMethod();
        }

        private void BuildMethodParams(List<Param> @params)
        {
            if (@params.Count > 0)
            { 
                Class.Append(", ");
                Class.Append(string.Join(", ", @params.Select(p => $"{p.Type} {p.Name}")));
            }
        }

        private void BuildParams(List<Param> @params, string paramsTab)
        {
            if (@params.Count > 0)
            {
                Class.Append(string.Join($",{NL}", @params.Select(p => $"{paramsTab}(\"{p.PgName}\", {p.Name}, {p.DbType})")));
            }
        }

        private void BuildCommentHeader(PgRoutineGroup routine, Return @return, List<Param> @params, bool sync)
        {
            Class.AppendLine($"{I2}/// <summary>");
            Class.AppendLine($"{I2}/// {(sync ? "Executes" : "Asynchronously executes")} {routine.Language} {routine.RoutineType} \"{Name}\"");
            if (!string.IsNullOrEmpty(routine.Description))
            {
                Class.Append(I2);
                Class.AppendLine(string.Join($"{Environment.NewLine}{I2}", 
                    routine.Description.Split("\n").Select(d => $"/// {d}")));
            }
            Class.AppendLine($"{I2}/// </summary>");
            foreach(var p in @params)
            {
                Class.AppendLine($"{I2}/// <param name=\"{p.Name}\">{p.PgName} {p.PgType}</param>");
            }
            if (@return.IsInstance)
            {
                if (sync)
                {
                    Class.AppendLine($"{I2}/// <returns>IEnumerable of {@namespace}.{@return.Name} instances</returns>");
                }
                else
                {
                    Class.AppendLine($"{I2}/// <returns>IAsyncEnumerable of {@namespace}.{@return.Name} instances</returns>");
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
                    Class.AppendLine($"{I2}/// <returns>ValueTask whose Result property is {@return.Name}</returns>");
                }
            }
        }

        private List<Param> GetParamsInfo(PgRoutineGroup routine)
        {
            return routine.Parameters.Select(p => 
                new Param(p.Name, p.Name.ToCamelCase(), p.DataType, GetParamType(p), GetParamDbType(p))).ToList();
        }

        private Return GetReturnInfo(PgRoutineGroup routine)
        {
            if (routine == null || routine.DataType == null || routine.DataType == "void")
            {
                return new Return("void", "void", true, false);
            }
            if (TryGetRoutineMapping(routine, out var result))
            {
                if (routine.DataType == "ARRAY")
                {
                    return new Return($"{routine.TypeUdtName}[]", $"{result}[]", false, false);
                }
                if (result != "string")
                {
                    return new Return(routine.DataType, $"{result}?", false, false);
                }
                return new Return(routine.DataType, result, false, false);
            }
            if (routine.DataType == "USER-DEFINED")
            {
                return new Return(routine.TypeUdtName, BuildUserDefinedModel(routine), false, true);
            }
            if (routine.DataType == "record")
            {
                return new Return(routine.TypeUdtName, BuildRecordModel(routine), false, true);
            }
            throw new ArgumentException($"Could not find mapping \"{routine.DataType}\" for return type of routine \"{routine.RoutineName}\"");
        }

        private string BuildUserDefinedModel(PgRoutineGroup routine)
        {
            var name = routine.TypeUdtName.ToUpperCamelCase();
            if (settings.CustomModels.ContainsKey(name))
            {
                name = settings.CustomModels[name];
            }
            else if (settings.CustomModels.ContainsKey(routine.TypeUdtName))
            {
                name = settings.CustomModels[routine.TypeUdtName];
            }
            BuildModel(name, connection => connection.GetRoutineReturnsTable(routine));
            UserDefinedModels.Add(name);
            return name;
        }

        private string BuildRecordModel(PgRoutineGroup routine)
        {
            var suffix = ++recordModelCount == 1 ? "" : recordModelCount.ToString();
            var name = $"{this.Name.ToUpperCamelCase()}{suffix}Result";
            return BuildModel(name, connection => connection.GetRoutineReturnsRecord(routine));
        }

        private string BuildModel(string name, Func<NpgsqlConnection, IEnumerable<PgReturns>> func)
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
                        return $"{result}[]";
                    }
                    if (result != "string" && returnModel.Nullable)
                    {
                        return $"{result}?";
                    }
                    return result;
                }
                throw new ArgumentException($"Could not find mapping \"{returnModel.DataType}\" for result type of routine  \"{this.Name}\"");
            }
            var model = new StringBuilder();
            var modelContent = new StringBuilder();
            if (!settings.UseRecords)
            {
                model.AppendLine($"{I1}public class {name}");
                model.AppendLine($"{I1}{{");
                foreach (var item in func(connection))
                {
                    modelContent.AppendLine($"{I2}public {getType(item)} {item.Name.ToUpperCamelCase()} {{ get; set; }}");
                }
                model.Append(modelContent);
                model.AppendLine($"{I1}}}");
            }
            else
            {
                model.Append($"{I1}public record {name}(");
                modelContent.Append(string.Join(", ", func(connection).Select(item => $"{getType(item)} {item.Name.ToUpperCamelCase()}")));
                model.Append(modelContent);
                model.AppendLine($");");
            }
            foreach(var (key, value) in ModelContent)
            {
                if (value.Equals(modelContent))
                {
                    return key;
                }
            }
            Models.Add(name, model);
            ModelContent.Add(name, modelContent);
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
}
