namespace PgRoutiner.Builder.CodeBuilders.Crud;

public class CodeCrudBuilder : CodeBuilder
{
    public CodeCrudBuilder(NpgsqlConnection connection, Current settings, CodeSettings codeSettings) :
        base(connection, settings, codeSettings)
    {
    }

    protected override IEnumerable<CodeResult> GetCodes()
    {
        var crudReadBy = settings.CrudReadBy?.Split(";").ToHashSet() ?? new HashSet<string>();
        var crudReadAll = settings.CrudReadAll?.Split(";").ToHashSet() ?? new HashSet<string>();
        var crudUpdate = settings.CrudUpdate?.Split(";").ToHashSet() ?? new HashSet<string>();
        var crudUpdateReturning = settings.CrudUpdateReturning?.Split(";").ToHashSet() ?? new HashSet<string>();
        var crudDeleteBy = settings.CrudDeleteBy?.Split(";").ToHashSet() ?? new HashSet<string>();
        var crudDeleteByReturning = settings.CrudDeleteByReturning?.Split(";").ToHashSet() ?? new HashSet<string>();
        var crudCreate = settings.CrudCreate?.Split(";").ToHashSet() ?? new HashSet<string>();
        var crudCreateReturning = settings.CrudCreateReturning?.Split(";").ToHashSet() ?? new HashSet<string>();
        var crudCreateOnConflictDoNothing = settings.CrudCreateOnConflictDoNothing?.Split(";").ToHashSet() ?? new HashSet<string>();
        var crudCreateOnConflictDoNothingReturning = settings.CrudCreateOnConflictDoNothingReturning?.Split(";").ToHashSet() ?? new HashSet<string>();
        var crudCreateOnConflictDoUpdate = settings.CrudCreateOnConflictDoUpdate?.Split(";")?.ToHashSet() ?? new HashSet<string>();
        var crudCreateOnConflictDoUpdateReturning = settings.CrudCreateOnConflictDoUpdateReturning?.Split(";").ToHashSet() ?? new HashSet<string>();

        var nonExisting = connection.GetTablesThatDontExist(crudReadBy
            .Union(crudReadAll)
            .Union(crudUpdate)
            .Union(crudUpdateReturning)
            .Union(crudDeleteBy)
            .Union(crudDeleteByReturning)
            .Union(crudCreate)
            .Union(crudCreateReturning)
            .Union(crudCreateOnConflictDoNothing)
            .Union(crudCreateOnConflictDoNothingReturning)
            .Union(crudCreateOnConflictDoUpdate)
            .Union(crudCreateOnConflictDoUpdateReturning).ToList()).ToArray();

        if (nonExisting.Any())
        {
            Program.WriteLine(ConsoleColor.Yellow, "", $"WARNING: Some of the tables in CRUD configuration are not found in the database. Following tables will be skipped for the CRUD code generation: {string.Join(", ", nonExisting)}");
        }

        foreach (var group in connection.GetTableDefintions(settings))
        {
            var (schema, name) = group.Key;

            var modelName = name.ToUpperCamelCase();
            if (settings.CustomModels.ContainsKey(modelName))
            {
                modelName = settings.CustomModels[modelName];
            }
            var module = new RoutineModule(settings, codeSettings, schema);
            Code code = null;

            CodeResult GetCodeResult(string nameSuffix, Func<Code> codeFunc)
            {
                try
                {
                    code = codeFunc();
                }
                catch (ArgumentException e)
                {
                    Writer.Error($"Code for table {name} could not be generated. {e.Message}");
                    return null;
                }
                return new CodeResult
                {
                    Code = code,
                    Name = name,
                    Module = module,
                    Schema = schema,
                    NameSuffix = nameSuffix,
                    ForName = modelName
                };
            }


            if (OptionContains(crudReadBy, schema, name))
            {
                var result = GetCodeResult("read_by", () => new CrudReadByCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(crudReadAll, schema, name))
            {
                var result = GetCodeResult("read_all", () => new CrudReadAllCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(crudUpdate, schema, name))
            {
                var result = GetCodeResult("update", () => new CrudUpdateCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(crudUpdateReturning, schema, name))
            {
                var result = GetCodeResult("update_returning", () => new CrudUpdateReturningCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(crudDeleteBy, schema, name))
            {
                var result = GetCodeResult("delete_by", () => new CrudDeleteByCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(crudDeleteByReturning, schema, name))
            {
                var result = GetCodeResult("delete_by_returning", () => new CrudDeleteByReturningCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(crudCreate, schema, name))
            {
                var result = GetCodeResult("create", () => new CrudCreateCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(crudCreateReturning, schema, name))
            {
                var result = GetCodeResult("create_returning", () => new CrudCreateReturningCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(crudCreateOnConflictDoNothing, schema, name))
            {
                var result = GetCodeResult("create_on_conflict_do_nothing", () => new CrudCreateOnConflictDoNothingCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(crudCreateOnConflictDoNothingReturning, schema, name))
            {
                var result = GetCodeResult("create_on_conflict_do_nothing_returning", () => new CrudCreateOnConflictDoNothingReturningCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(crudCreateOnConflictDoUpdate, schema, name))
            {
                var result = GetCodeResult("create_on_conflict_do_update", () => new CrudCreateOnConflictDoUpdateCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(crudCreateOnConflictDoUpdateReturning, schema, name))
            {
                var result = GetCodeResult("create_on_conflict_do_update_returning", () => new CrudCreateOnConflictDoUpdateReturningCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
        }
    }

    public static bool OptionContains(HashSet<string> option, string schema, string name)
    {
        return option.Contains("*") || 
            option.Contains(name) ||
            option.Contains($"{schema}.{name}") ||
            option.Contains($"\"{schema}\".\"{name}\"") ||
            option.Contains($"{schema}.\"{name}\"") ||
            option.Contains($"\"{schema}\".{name}");
    }
}
