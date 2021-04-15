using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Npgsql;

namespace PgRoutiner
{
    public class CodeCrudBuilder : CodeBuilder
    {
        public CodeCrudBuilder(NpgsqlConnection connection, Settings settings, CodeSettings codeSettings) :
            base(connection, settings, codeSettings)
        {
        }

        protected override Module GetModule(Settings settings, CodeSettings codeSettings) => new RoutineModule(settings, codeSettings);

        protected override IEnumerable<(Code code, string name, string shortFilename, string fullFileName, string relative, Module module)>
            GetCodes(string outputDir)
        {
            foreach (var group in connection.GetTableDefintions(settings))
            {
                var (schema, name) = group.Key;
                var module = GetModule(settings, codeSettings);
                string shortFilename = null, fullFileName = null, relative = null;
                Code code = null;
                if (OptionContains(settings.CrudReadBy, schema, name))
                {
                    try
                    {
            
                        (shortFilename, fullFileName, relative) = GetFileNames($"{name}_read_by", outputDir);
                        code = new CrudReadByCode(settings, group.Key, module.Namespace, group);
                    }
                    catch (ArgumentException e)
                    {
                        Builder.Error($"File {relative} could not be generated. {e.Message}");
                        continue;
                    }
                    yield return (code, name, shortFilename, fullFileName, relative, module);
                }
                if (OptionContains(settings.CrudReadAll, schema, name))
                {
                    try
                    {

                        (shortFilename, fullFileName, relative) = GetFileNames($"{name}_read_all", outputDir);
                        code = new CrudReadAllCode(settings, group.Key, module.Namespace, group);
                    }
                    catch (ArgumentException e)
                    {
                        Builder.Error($"File {relative} could not be generated. {e.Message}");
                        continue;
                    }
                    yield return (code, name, shortFilename, fullFileName, relative, module);
                }
                if (OptionContains(settings.CrudUpdate, schema, name))
                {
                    try
                    {

                        (shortFilename, fullFileName, relative) = GetFileNames($"{name}_update", outputDir);
                        code = new CrudUpdateCode(settings, group.Key, module.Namespace, group);
                    }
                    catch (ArgumentException e)
                    {
                        Builder.Error($"File {relative} could not be generated. {e.Message}");
                        continue;
                    }
                    yield return (code, name, shortFilename, fullFileName, relative, module);
                }
                if (OptionContains(settings.CrudUpdateReturning, schema, name))
                {
                    try
                    {

                        (shortFilename, fullFileName, relative) = GetFileNames($"{name}_update_returning", outputDir);
                        code = new CrudUpdateReturningCode(settings, group.Key, module.Namespace, group);
                    }
                    catch (ArgumentException e)
                    {
                        Builder.Error($"File {relative} could not be generated. {e.Message}");
                        continue;
                    }
                    yield return (code, name, shortFilename, fullFileName, relative, module);
                }
                if (OptionContains(settings.CrudDelete, schema, name))
                {
                    try
                    {

                        (shortFilename, fullFileName, relative) = GetFileNames($"{name}_delete", outputDir);
                        code = new CrudDeleteCode(settings, group.Key, module.Namespace, group);
                    }
                    catch (ArgumentException e)
                    {
                        Builder.Error($"File {relative} could not be generated. {e.Message}");
                        continue;
                    }
                    yield return (code, name, shortFilename, fullFileName, relative, module);
                }
                if (OptionContains(settings.CrudDeleteReturning, schema, name))
                {
                    try
                    {

                        (shortFilename, fullFileName, relative) = GetFileNames($"{name}_delete_returning", outputDir);
                        code = new CrudDeleteReturningCode(settings, group.Key, module.Namespace, group);
                    }
                    catch (ArgumentException e)
                    {
                        Builder.Error($"File {relative} could not be generated. {e.Message}");
                        continue;
                    }
                    yield return (code, name, shortFilename, fullFileName, relative, module);
                }
                if (OptionContains(settings.CrudCreate, schema, name))
                {
                    try
                    {

                        (shortFilename, fullFileName, relative) = GetFileNames($"{name}_create", outputDir);
                        code = new CrudCreateCode(settings, group.Key, module.Namespace, group);
                    }
                    catch (ArgumentException e)
                    {
                        Builder.Error($"File {relative} could not be generated. {e.Message}");
                        continue;
                    }
                    yield return (code, name, shortFilename, fullFileName, relative, module);
                }
                if (OptionContains(settings.CrudCreateReturning, schema, name))
                {
                    try
                    {

                        (shortFilename, fullFileName, relative) = GetFileNames($"{name}_create_returning", outputDir);
                        code = new CrudCreateReturningCode(settings, group.Key, module.Namespace, group);
                    }
                    catch (ArgumentException e)
                    {
                        Builder.Error($"File {relative} could not be generated. {e.Message}");
                        continue;
                    }
                    yield return (code, name, shortFilename, fullFileName, relative, module);
                }
            }
            //Program.WriteLine(ConsoleColor.Yellow, "", $"WARNING: Table {schema}.{name} not found, skipping...");
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
}
