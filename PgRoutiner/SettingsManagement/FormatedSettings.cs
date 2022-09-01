namespace PgRoutiner.SettingsManagement;

public class FormatedSettings
{
    public static string Build(bool wrap = true, NpgsqlConnection connection = null)
    {
        StringBuilder sb = new();

        void AddSectionComment(string header, string helpUrl, params string[] tips)
        {
            sb.AppendLine($"    /*");
            sb.AppendLine($"      {header}");
            foreach (var tip in tips)
            {
                sb.AppendLine($"      {tip}");
            }
            /*
            if (helpUrl != null)
            {
                sb.AppendLine($"      - For more info see: {helpUrl}");
            }
            */
            sb.AppendLine($"    */");
        }

        void AddEntry(string field, object fieldValue, string last = ",")
        {
            string v = null;
            if (fieldValue == null)
            {
                v = "null";
            }
            else if (fieldValue is string @string)
            {
                v = $"\"{@string.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
            }
            else if (fieldValue is bool boolean)
            {
                v = boolean == true ? "true" : "false";
            }
            else if (fieldValue is List<string> list)
            {
                if (list.Count == 0)
                {
                    v = "[ ]";
                }
                else
                {
                    v = $"[{Environment.NewLine}      {string.Join($",{Environment.NewLine}      ", list.Select(i => $"\"{i}\""))}{Environment.NewLine}    ]";
                }
            }
            else if (fieldValue is HashSet<string> hashset)
            {
                if (hashset.Count == 0)
                {
                    v = "[ ]";
                }
                else
                {
                    v = $"[{Environment.NewLine}      {string.Join($",{Environment.NewLine}      ", hashset.Select(i => $"\"{i}\""))}{Environment.NewLine}    ]";
                }
            }
            else if (fieldValue is int)
            {
                v = $"{fieldValue}";
            }
            else if (fieldValue is Dictionary<string, string> dictionary)
            {
                if (dictionary.Values.Count == 0)
                {
                    v = "{ }";
                }
                else
                {
                    v = $"{{{Environment.NewLine}      {string.Join($",{Environment.NewLine}      ", dictionary.Select(d => $"\"{d.Key}\": \"{d.Value}\""))}{Environment.NewLine}    }}";
                }
            }
            sb.AppendLine($"    \"{field}\": {v}{last}");
        }

        if (wrap)
        {
            sb.AppendLine($"/* PgRoutiner ({Program.Version}) settings */");
            sb.AppendLine("{");
            sb.AppendLine($"  /* see https://github.com/vb-consulting/PgRoutiner/wiki/10.-WORKING-WITH-CONNECTIONS for more info */");
            sb.AppendLine("  \"ConnectionStrings\": {");
            if (Settings.Value.Connection == null && connection != null)
            {
                Settings.Value.Connection = $"{connection.Database.ToUpperCamelCase()}Connection";
                sb.AppendLine($"    \"{Settings.Value.Connection}\": \"{connection.ToPsqlFormatString()}\"");
            }
            sb.AppendLine("    //\"Connection1\": \"Server={server};Db={database};Port={port};User Id={user};Password={password};\"");
            sb.AppendLine("    //\"Connection2\": \"postgresql://{user}:{password}@{server}:{port}/{database}\"");

            sb.AppendLine("  },");
            sb.AppendLine("  /* see https://github.com/vb-consulting/PgRoutiner/wiki/1.-WORKING-WITH-SETTINGS for more info */");
            sb.AppendLine("  \"PgRoutiner\": {");
            sb.AppendLine();
        }

        AddSectionComment(
            "General settings:",
            "https://github.com/vb-consulting/PgRoutiner/wiki/1.-WORKING-WITH-SETTINGS#general-settings",
            $"- Use \"{Settings.ConnectionArgs.Alias}\" or \"--{Settings.ConnectionArgs.Original.ToKebabCase()}\" option to set working connection from the command line.",
            $"- Use \"{Settings.SchemaArgs.Alias}\" or \"--{Settings.SchemaArgs.Original.ToKebabCase()}\" option to set schema similar to expression from the command line.",
            $"- Use \"{Settings.ExecuteArgs.Alias}\" or \"--{Settings.ExecuteArgs.Original.ToKebabCase()}\" option to execute SQL file or PSQL command on your current connection  from the command line.",
            $"- Use \"{Settings.DumpArgs.Alias}\" or \"--{Settings.DumpArgs.Original.ToKebabCase()}\" switch to redirect all outputs to the command line.",
            $"- Use \"{Settings.SilentArgs.Alias}\" or \"--{Settings.SilentArgs.Original.ToKebabCase()}\" to silent unrequired console texts.",
            $"- Use \"{Settings.ListArgs.Alias}\" or \"--{Settings.ListArgs.Original.ToKebabCase()}\" to dump object list for current connection and iwth current schema.",
            $"- Use \"{Settings.DefinitionArgs.Alias}\" or \"--{Settings.DefinitionArgs.Original.ToKebabCase()}\" to dump object schema definition in console supplied as value parameter.",
            $"- Use \"{Settings.InsertsArgs.Alias}\" or \"--{Settings.InsertsArgs.Original.ToKebabCase()}\" to dump objects or queries (semicolon separated) inserts.");
        AddEntry(nameof(Settings.Connection), Settings.Value.Connection);
        AddEntry(nameof(Settings.SkipConnectionPrompt), Settings.Value.SkipConnectionPrompt);
        AddEntry(nameof(Settings.DumpPgCommands), Settings.Value.DumpPgCommands);
        AddEntry(nameof(Settings.SchemaSimilarTo), Settings.Value.SchemaSimilarTo);
        AddEntry(nameof(Settings.SchemaNotSimilarTo), Settings.Value.SchemaNotSimilarTo);
        AddEntry(nameof(Settings.Execute), Settings.Value.Execute);
        AddEntry(nameof(Settings.List), Settings.Value.List);
        AddEntry(nameof(Settings.Definition), Settings.Value.Definition);
        AddEntry(nameof(Settings.Inserts), Settings.Value.Inserts);
        AddEntry(nameof(Settings.Dump), Settings.Value.Dump);
        AddEntry(nameof(Settings.Silent), Settings.Value.Silent);
        AddEntry(nameof(Settings.SkipIfExists), Settings.Value.SkipIfExists);
        AddEntry(nameof(Settings.SkipUpdateReferences), Settings.Value.SkipUpdateReferences);
        AddEntry(nameof(Settings.PgDump), Settings.Value.PgDump);
        AddEntry(nameof(Settings.PgDumpFallback), Settings.Value.PgDumpFallback);
        AddEntry(nameof(Settings.ConfigPath), Settings.Value.ConfigPath);

        sb.AppendLine();
        AddSectionComment(
            "Code generation general settings. Used in:",
            null,
            $"- Routines code generation.",
            $"- CRUD code generation.");
        AddEntry(nameof(Settings.Namespace), Settings.Value.Namespace);
        AddEntry(nameof(Settings.UseRecords), Settings.Value.UseRecords);
        AddEntry(nameof(Settings.UseExpressionBody), Settings.Value.UseExpressionBody);
        AddEntry(nameof(Settings.UseFileScopedNamespaces), Settings.Value.UseFileScopedNamespaces);
        AddEntry(nameof(Settings.UseNullableStrings), Settings.Value.UseNullableStrings);
        AddEntry(nameof(Settings.Mapping), Settings.Value.Mapping);
        AddEntry(nameof(Settings.CustomModels), Settings.Value.CustomModels);
        AddEntry(nameof(Settings.ModelDir), Settings.Value.ModelDir);
        AddEntry(nameof(Settings.ModelCustomNamespace), Settings.Value.ModelCustomNamespace);
        AddEntry(nameof(Settings.EmptyModelDir), Settings.Value.EmptyModelDir);
        AddEntry(nameof(Settings.SkipSyncMethods), Settings.Value.SkipSyncMethods);
        AddEntry(nameof(Settings.SkipAsyncMethods), Settings.Value.SkipAsyncMethods);
        AddEntry(nameof(Settings.MinNormVersion), Settings.Value.MinNormVersion);
        AddEntry(nameof(Settings.SourceHeaderLines), Settings.Value.SourceHeaderLines);
        AddEntry(nameof(Settings.Ident), Settings.Value.Ident);
        AddEntry(nameof(Settings.ReturnMethod), Settings.Value.ReturnMethod);
        AddEntry(nameof(Settings.MethodParameterNames), Settings.Value.MethodParameterNames);

        sb.AppendLine();
        AddSectionComment(
            "Routines data-access extensions code-generation",
            "https://github.com/vb-consulting/PgRoutiner/wiki/2.-WORKING-WITH-ROUTINES#routines-data-access-extensions-code-generation-settings",
            $"- Use \"{Settings.RoutinesArgs.Alias}\" or \"--{Settings.RoutinesArgs.Original.ToKebabCase()}\" switch to run routines data-access extensions code-generation from the command line.",
            $"- Use \"{Settings.OutputDirArgs.Alias}\" or \"--{Settings.OutputDirArgs.Original.ToKebabCase()}\" option to set the output dir for the generated code from the command line.",
            $"- Use \"{Settings.RoutinesOverwriteArgs.Alias}\" or \"--{Settings.RoutinesOverwriteArgs.Original.ToKebabCase()}\" switch to set the overwrite mode for the generated code from the command line.",
            $"- Use \"{Settings.ModelDirArgs.Alias}\" or \"--{Settings.ModelDirArgs.Original.ToKebabCase()}\" option to set the custom models output dir for the generated code from the command line.");
        AddEntry(nameof(Settings.Routines), Settings.Value.Routines);
        AddEntry(nameof(Settings.RoutinesSchemaSimilarTo), Settings.Value.RoutinesSchemaSimilarTo);
        AddEntry(nameof(Settings.RoutinesSchemaNotSimilarTo), Settings.Value.RoutinesSchemaNotSimilarTo);
        AddEntry(nameof(Settings.OutputDir), Settings.Value.OutputDir);
        AddEntry(nameof(Settings.RoutinesEmptyOutputDir), Settings.Value.RoutinesEmptyOutputDir);
        AddEntry(nameof(Settings.RoutinesOverwrite), Settings.Value.RoutinesOverwrite);
        AddEntry(nameof(Settings.RoutinesAskOverwrite), Settings.Value.RoutinesAskOverwrite);
        AddEntry(nameof(Settings.RoutinesNotSimilarTo), Settings.Value.RoutinesNotSimilarTo);
        AddEntry(nameof(Settings.RoutinesSimilarTo), Settings.Value.RoutinesSimilarTo);
        AddEntry(nameof(Settings.RoutinesReturnMethods), Settings.Value.RoutinesReturnMethods);
        AddEntry(nameof(Settings.RoutinesModelPropertyTypes), Settings.Value.RoutinesModelPropertyTypes);
        AddEntry(nameof(Settings.RoutinesUnknownReturnTypes), Settings.Value.RoutinesUnknownReturnTypes);
        AddEntry(nameof(Settings.RoutinesCallerInfo), Settings.Value.RoutinesCallerInfo);
        AddEntry(nameof(Settings.RoutinesLanguages), Settings.Value.RoutinesLanguages);
        
        sb.AppendLine();
        AddSectionComment(
            "Unit tests code-generation settings",
            "https://github.com/vb-consulting/PgRoutiner/wiki/3.-WORKING-WITH-UNIT-TESTS#unit-tests-code-generation-settings",
            $"- Use \"{Settings.UnitTestsArgs.Alias}\" or \"--{Settings.UnitTestsArgs.Original.ToKebabCase()}\" switch to run unit tests code-generation from the command line.",
            $"- Use \"{Settings.UnitTestsDirArgs.Alias}\" or \"--{Settings.UnitTestsDirArgs.Original.ToKebabCase()}\" option to set the output dir for the generated unit test project from the command line.");
        AddEntry(nameof(Settings.UnitTests), Settings.Value.UnitTests);
        AddEntry(nameof(Settings.UnitTestProjectTargetFramework), Settings.Value.UnitTestProjectTargetFramework);
        AddEntry(nameof(Settings.UnitTestProjectLangVersion), Settings.Value.UnitTestProjectLangVersion);
        AddEntry(nameof(Settings.UnitTestsDir), Settings.Value.UnitTestsDir);
        AddEntry(nameof(Settings.UnitTestsAskRecreate), Settings.Value.UnitTestsAskRecreate);
        AddEntry(nameof(Settings.UnitTestsSkipSyncMethods), Settings.Value.UnitTestsSkipSyncMethods);
        AddEntry(nameof(Settings.UnitTestsSkipAsyncMethods), Settings.Value.UnitTestsSkipAsyncMethods);

        sb.AppendLine();
        AddSectionComment(
            "Schema dump script settings",
            "https://github.com/vb-consulting/PgRoutiner/wiki/4.-WORKING-WITH-SCHEMA-DUMP-SCRIPT#schema-dump-script-settings",
            $"- Use \"{Settings.SchemaDumpArgs.Alias}\" or \"--{Settings.SchemaDumpArgs.Original.ToKebabCase()}\" switch to run schema script dump from the command line.",
            $"- Use \"{Settings.SchemaDumpFileArgs.Alias}\" or \"--{Settings.SchemaDumpFileArgs.Original.ToKebabCase()}\" option to set generated schema file name from the command line.",
            $"- Use \"{Settings.SchemaDumpOverwriteArgs.Alias}\" or \"--{Settings.SchemaDumpOverwriteArgs.Original.ToKebabCase()}\" switch to set the overwrite mode for the generated schema file from the command line.",
            $"- Use \"--{nameof(Settings.SchemaDumpPrivileges).ToKebabCase()}\" switch to include object privileges in schema file from the command line.");
        AddEntry(nameof(Settings.SchemaDump), Settings.Value.SchemaDump);
        AddEntry(nameof(Settings.SchemaDumpFile), Settings.Value.SchemaDumpFile);
        AddEntry(nameof(Settings.SchemaDumpOverwrite), Settings.Value.SchemaDumpOverwrite);
        AddEntry(nameof(Settings.SchemaDumpAskOverwrite), Settings.Value.SchemaDumpAskOverwrite);
        AddEntry(nameof(Settings.SchemaDumpOwners), Settings.Value.SchemaDumpOwners);
        AddEntry(nameof(Settings.SchemaDumpPrivileges), Settings.Value.SchemaDumpPrivileges);
        AddEntry(nameof(Settings.SchemaDumpNoDropIfExists), Settings.Value.SchemaDumpNoDropIfExists);
        AddEntry(nameof(Settings.SchemaDumpOptions), Settings.Value.SchemaDumpOptions);
        AddEntry(nameof(Settings.SchemaDumpNoTransaction), Settings.Value.SchemaDumpNoTransaction);

        sb.AppendLine();
        AddSectionComment(
            "Data dump script settings",
            "https://github.com/vb-consulting/PgRoutiner/wiki/5.-WORKING-WITH-DATA-DUMP-SCRIPT#data-dump-script-settings",
            $"- Use \"{Settings.DataDumpArgs.Alias}\" or \"--{Settings.DataDumpArgs.Original.ToKebabCase()}\" switch to run data script dump from the command line.",
            $"- Use \"{Settings.DataDumpFileArgs.Alias}\" or \"--{Settings.DataDumpFileArgs.Original.ToKebabCase()}\" option to set generated data script file name from the command line.",
            $"- Use \"{Settings.DataDumpOverwriteArgs.Alias}\" or \"--{Settings.DataDumpOverwriteArgs.Original.ToKebabCase()}\" switch to set the overwrite mode for the generated data script file from the command line.",
            $"- Use \"{Settings.DataDumpListArgs.Alias}\" or \"--{Settings.DataDumpListArgs.Original.ToKebabCase()}\" set semicolon separated list of tables or queries merged with \"DataDumpTables\" option and to be dumped.");
        AddEntry(nameof(Settings.DataDump), Settings.Value.DataDump);
        AddEntry(nameof(Settings.DataDumpFile), Settings.Value.DataDumpFile);
        AddEntry(nameof(Settings.DataDumpOverwrite), Settings.Value.DataDumpOverwrite);
        AddEntry(nameof(Settings.DataDumpAskOverwrite), Settings.Value.DataDumpAskOverwrite);
        AddEntry(nameof(Settings.DataDumpList), Settings.Value.DataDumpList);
        AddEntry(nameof(Settings.DataDumpTables), Settings.Value.DataDumpTables);
        AddEntry(nameof(Settings.DataDumpOptions), Settings.Value.DataDumpOptions);
        AddEntry(nameof(Settings.DataDumpNoTransaction), Settings.Value.DataDumpNoTransaction);
        AddEntry(nameof(Settings.DataDumpRaw), Settings.Value.DataDumpRaw);

        sb.AppendLine();
        AddSectionComment(
            "Object file tree settings",
            "https://github.com/vb-consulting/PgRoutiner/wiki/6.-WORKING-WITH-OBJECT-FILES-TREE#object-file-tree-settings",
            $"- Use \"{Settings.DbObjectsArgs.Alias}\" or \"--{Settings.DbObjectsArgs.Original.ToKebabCase()}\" switch to run object files tree dump from the command line.",
            $"- Use \"{Settings.DbObjectsDirArgs.Alias}\" or \"--{Settings.DbObjectsDirArgs.Original.ToKebabCase()}\" option to set the root output dir from the command line.",
            $"- Use \"{Settings.DbObjectsOverwriteArgs.Alias}\" or \"--{Settings.DbObjectsOverwriteArgs.Original.ToKebabCase()}\" switch to set the overwrite mode for the generated files from the command line.");
        AddEntry(nameof(Settings.DbObjects), Settings.Value.DbObjects);
        AddEntry(nameof(Settings.DbObjectsDir), Settings.Value.DbObjectsDir);
        AddEntry(nameof(Settings.DbObjectsOverwrite), Settings.Value.DbObjectsOverwrite);
        AddEntry(nameof(Settings.DbObjectsAskOverwrite), Settings.Value.DbObjectsAskOverwrite);
        AddEntry(nameof(Settings.DbObjectsDirNames), Settings.Value.DbObjectsDirNames);
        AddEntry(nameof(Settings.DbObjectsSkipDeleteDir), Settings.Value.DbObjectsSkipDeleteDir);
        //AddEntry(nameof(Settings.DbObjectsRemoveExistingDirs), Settings.Value.DbObjectsRemoveExistingDirs);
        AddEntry(nameof(Settings.DbObjectsOwners), Settings.Value.DbObjectsOwners);
        AddEntry(nameof(Settings.DbObjectsPrivileges), Settings.Value.DbObjectsPrivileges);
        AddEntry(nameof(Settings.DbObjectsCreateOrReplace), Settings.Value.DbObjectsCreateOrReplace);
        AddEntry(nameof(Settings.DbObjectsRaw), Settings.Value.DbObjectsRaw);

        sb.AppendLine();
        AddSectionComment(
            "Markdown (MD) database dictionaries settings",
            "https://github.com/vb-consulting/PgRoutiner/wiki/7.-WORKING-WITH-MARKDOWN-DATABASE-DICTIONARIES#markdown-md-database-dictionaries-settings",
            $"- Use \"{Settings.MarkdownArgs.Alias}\" or \"--{Settings.MarkdownArgs.Original.ToKebabCase()}\" switch to run markdown (MD) database dictionary file from the command line.",
            $"- Use \"{Settings.MdFileArgs.Alias}\" or \"--{Settings.MdFileArgs.Original.ToKebabCase()}\" option to set generated dictionary file name from the command line.",
            $"- Use \"{Settings.CommitMdArgs.Alias}\" or \"--{Settings.CommitMdArgs.Original.ToKebabCase()}\" switch to run commit changes in comments from the MD file back to the database from the command line.");
        AddEntry(nameof(Settings.Markdown), Settings.Value.Markdown);
        AddEntry(nameof(Settings.MdFile), Settings.Value.MdFile);
        AddEntry(nameof(Settings.MdSchemaSimilarTo), Settings.Value.MdSchemaSimilarTo);
        AddEntry(nameof(Settings.MdSchemaNotSimilarTo), Settings.Value.MdSchemaNotSimilarTo);
        AddEntry(nameof(Settings.MdOverwrite), Settings.Value.MdOverwrite);
        AddEntry(nameof(Settings.MdAskOverwrite), Settings.Value.MdAskOverwrite);
        AddEntry(nameof(Settings.MdSkipRoutines), Settings.Value.MdSkipRoutines);
        AddEntry(nameof(Settings.MdSkipViews), Settings.Value.MdSkipViews);
        AddEntry(nameof(Settings.MdSkipEnums), Settings.Value.MdSkipEnums);
        AddEntry(nameof(Settings.MdNotSimilarTo), Settings.Value.MdNotSimilarTo);
        AddEntry(nameof(Settings.MdSimilarTo), Settings.Value.MdSimilarTo);
        AddEntry(nameof(Settings.MdIncludeSourceLinks), Settings.Value.MdIncludeSourceLinks);
        AddEntry(nameof(Settings.MdIncludeExtensionLinks), Settings.Value.MdIncludeExtensionLinks);
        AddEntry(nameof(Settings.MdIncludeUnitTestsLinks), Settings.Value.MdIncludeUnitTestsLinks);
        AddEntry(nameof(Settings.MdSourceLinkRoot), Settings.Value.MdSourceLinkRoot);
        AddEntry(nameof(Settings.MdIncludeTableCountEstimates), Settings.Value.MdIncludeTableCountEstimates);
        AddEntry(nameof(Settings.MdIncludeTableStats), Settings.Value.MdIncludeTableStats);
        AddEntry(nameof(Settings.MdRoutinesFirst), Settings.Value.MdRoutinesFirst);
        AddEntry(nameof(Settings.MdExportToHtml), Settings.Value.MdExportToHtml);
        AddEntry(nameof(Settings.CommitMd), Settings.Value.CommitMd);

        sb.AppendLine();
        AddSectionComment(
            "PSQL command-line utility settings",
            "https://github.com/vb-consulting/PgRoutiner/wiki/8.-WORKING-WITH-PSQL#psql-command-line-utility-settings",
            $"- Use \"{Settings.PsqlArgs.Alias}\" or \"--{Settings.PsqlArgs.Original.ToKebabCase()}\" switch to open PSQL command-line utility from the command line.");
        AddEntry(nameof(Settings.Psql), Settings.Value.Psql);
        AddEntry(nameof(Settings.PsqlTerminal), Settings.Value.PsqlTerminal);
        AddEntry(nameof(Settings.PsqlCommand), Settings.Value.PsqlCommand);
        AddEntry(nameof(Settings.PsqlFallback), Settings.Value.PsqlFallback);
        AddEntry(nameof(Settings.PsqlOptions), Settings.Value.PsqlOptions);

        sb.AppendLine();
        AddSectionComment(
            "Diff scripts settings",
            "https://github.com/vb-consulting/PgRoutiner/wiki/9.-WORKING-WITH-DIFF-SCRIPTS#diff-scripts-settings",
            $"- Use \"{Settings.DiffArgs.Alias}\" or \"--{Settings.DiffArgs.Original.ToKebabCase()}\" switch to run diff script generation from the command line.",
            $"- Use \"{Settings.DiffTargetArgs.Alias}\" or \"--{Settings.DiffTargetArgs.Original.ToKebabCase()}\" option to set target connection for the diff script generator from the command line.");
        AddEntry(nameof(Settings.Diff), Settings.Value.Diff);
        AddEntry(nameof(Settings.DiffTarget), Settings.Value.DiffTarget);
        AddEntry(nameof(Settings.DiffFilePattern), Settings.Value.DiffFilePattern);
        AddEntry(nameof(Settings.DiffPgDump), Settings.Value.DiffPgDump);
        AddEntry(nameof(Settings.DiffPrivileges), Settings.Value.DiffPrivileges);
        AddEntry(nameof(Settings.DiffSkipSimilarTo), Settings.Value.DiffSkipSimilarTo);

        sb.AppendLine();
        AddSectionComment(
            "CRUD scripts settings",
            "https://github.com/vb-consulting/PgRoutiner/wiki/10.-WORKING-WITH-CRUD#crud-settings",
            $"- Use \"{Settings.CrudArgs.Alias}\" or \"--{Settings.CrudArgs.Original.ToKebabCase()}\" switch to run CRUD extension methods generation from the command line.",
            $"- Use \"{Settings.CrudOutputDirArgs.Alias}\" or \"--{Settings.CrudOutputDirArgs.Original.ToKebabCase()}\" option to set the custom models output dir for the generated CRUD extension methods code from the command line.");
        AddEntry(nameof(Settings.Crud), Settings.Value.Crud);
        AddEntry(nameof(Settings.CrudOutputDir), Settings.Value.CrudOutputDir);
        AddEntry(nameof(Settings.CrudEmptyOutputDir), Settings.Value.CrudEmptyOutputDir);
        AddEntry(nameof(Settings.CrudOverwrite), Settings.Value.CrudOverwrite);
        AddEntry(nameof(Settings.CrudAskOverwrite), Settings.Value.CrudAskOverwrite);
        AddEntry(nameof(Settings.CrudNoPrepare), Settings.Value.CrudNoPrepare);
        AddEntry(nameof(Settings.CrudReturnMethods), Settings.Value.CrudReturnMethods);
        AddEntry(nameof(Settings.CrudCreate), Settings.Value.CrudCreate);
        AddEntry(nameof(Settings.CrudCreateReturning), Settings.Value.CrudCreateReturning);
        AddEntry(nameof(Settings.CrudCreateOnConflictDoNothing), Settings.Value.CrudCreateOnConflictDoNothing);
        AddEntry(nameof(Settings.CrudCreateOnConflictDoNothingReturning), Settings.Value.CrudCreateOnConflictDoNothingReturning);
        AddEntry(nameof(Settings.CrudCreateOnConflictDoUpdate), Settings.Value.CrudCreateOnConflictDoUpdate);
        AddEntry(nameof(Settings.CrudCreateOnConflictDoUpdateReturning), Settings.Value.CrudCreateOnConflictDoUpdateReturning);
        AddEntry(nameof(Settings.CrudReadBy), Settings.Value.CrudReadBy);
        AddEntry(nameof(Settings.CrudReadAll), Settings.Value.CrudReadAll);
        AddEntry(nameof(Settings.CrudUpdate), Settings.Value.CrudUpdate);
        AddEntry(nameof(Settings.CrudUpdateReturning), Settings.Value.CrudUpdateReturning);
        AddEntry(nameof(Settings.CrudDeleteBy), Settings.Value.CrudDeleteBy);
        AddEntry(nameof(Settings.CrudDeleteByReturning), Settings.Value.CrudDeleteByReturning, "");

        if (wrap)
        {
            sb.AppendLine("  }");
            sb.AppendLine("}");
        }

        return sb.ToString();
    }
}
