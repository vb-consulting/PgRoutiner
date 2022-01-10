namespace PgRoutiner.Builder.CodeBuilders.Crud;

public class CodeCrudBuilder : CodeBuilder
{
    public CodeCrudBuilder(NpgsqlConnection connection, Settings settings, CodeSettings codeSettings) :
        base(connection, settings, codeSettings)
    {
    }

    protected override IEnumerable<CodeResult> GetCodes()
    {
        var nonExisting = connection.GetTablesThatDontExist(settings.CrudReadBy
            .Union(settings.CrudReadAll)
            .Union(settings.CrudUpdate)
            .Union(settings.CrudUpdateReturning)
            .Union(settings.CrudDeleteBy)
            .Union(settings.CrudDeleteByReturning)
            .Union(settings.CrudCreate)
            .Union(settings.CrudCreateReturning)
            .Union(settings.CrudCreateOnConflictDoNothing)
            .Union(settings.CrudCreateOnConflictDoNothingReturning)
            .Union(settings.CrudCreateOnConflictDoUpdate)
            .Union(settings.CrudCreateOnConflictDoUpdateReturning).ToList()).ToArray();

        if (nonExisting.Any())
        {
            Program.WriteLine(ConsoleColor.Yellow, "", $"WARNING: Some of the tables in CRUD configuration are not found in the database. Following tables will be skiped for the CRUD code generation: {string.Join(", ", nonExisting)}");
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


            if (OptionContains(settings.CrudReadBy, schema, name))
            {
                var result = GetCodeResult("read_by", () => new CrudReadByCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(settings.CrudReadAll, schema, name))
            {
                var result = GetCodeResult("read_all", () => new CrudReadAllCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(settings.CrudUpdate, schema, name))
            {
                var result = GetCodeResult("update", () => new CrudUpdateCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(settings.CrudUpdateReturning, schema, name))
            {
                var result = GetCodeResult("update_returning", () => new CrudUpdateReturningCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(settings.CrudDeleteBy, schema, name))
            {
                var result = GetCodeResult("delete_by", () => new CrudDeleteByCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(settings.CrudDeleteByReturning, schema, name))
            {
                var result = GetCodeResult("delete_by_returning", () => new CrudDeleteByReturningCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(settings.CrudCreate, schema, name))
            {
                var result = GetCodeResult("create", () => new CrudCreateCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(settings.CrudCreateReturning, schema, name))
            {
                var result = GetCodeResult("create_returning", () => new CrudCreateReturningCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(settings.CrudCreateOnConflictDoNothing, schema, name))
            {
                var result = GetCodeResult("create_on_conflict_do_nothing", () => new CrudCreateOnConflictDoNothingCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(settings.CrudCreateOnConflictDoNothingReturning, schema, name))
            {
                var result = GetCodeResult("create_on_conflict_do_nothing_returning", () => new CrudCreateOnConflictDoNothingReturningCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(settings.CrudCreateOnConflictDoUpdate, schema, name))
            {
                var result = GetCodeResult("create_on_conflict_do_update", () => new CrudCreateOnConflictDoUpdateCode(settings, group.Key, module.Namespace, group));
                if (result == null)
                {
                    continue;
                }
                yield return result;
            }
            if (OptionContains(settings.CrudCreateOnConflictDoUpdateReturning, schema, name))
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
        return option.Contains(name) ||
            option.Contains($"{schema}.{name}") ||
            option.Contains($"\"{schema}\".\"{name}\"") ||
            option.Contains($"{schema}.\"{name}\"") ||
            option.Contains($"\"{schema}\".{name}");
    }
}
