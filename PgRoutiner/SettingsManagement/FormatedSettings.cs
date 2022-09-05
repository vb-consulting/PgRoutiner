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
            //sb.AppendLine($"  /* see https://github.com/vb-consulting/PgRoutiner/wiki/10.-WORKING-WITH-CONNECTIONS for more info */");
            sb.AppendLine("  \"ConnectionStrings\": {");
            if (Current.Value.Connection == null && connection != null)
            {
                Current.Value.Connection = $"{connection.Database.ToUpperCamelCase()}Connection";
                sb.AppendLine($"    \"{Current.Value.Connection}\": \"{connection.ToPsqlFormatString()}\"");
            }
            sb.AppendLine("    //\"Connection1\": \"Server={server};Db={database};Port={port};User Id={user};Password={password};\"");
            sb.AppendLine("    //\"Connection2\": \"postgresql://{user}:{password}@{server}:{port}/{database}\"");

            sb.AppendLine("  },");
            sb.AppendLine("  /* see https://github.com/vb-consulting/postgresql-driven-development-demo/blob/master/PDD.Database/README.md for more info */");
            sb.AppendLine("  \"PgRoutiner\": {");
            sb.AppendLine();
        }

        AddSectionComment(
            "General settings:",
            null,
            $"- Use \"{Current.ConnectionArgs.Alias}\" or \"--{Current.ConnectionArgs.Original.ToKebabCase()}\" option to set working connection from the command line.",
            $"- Use \"{Current.SchemaArgs.Alias}\" or \"--{Current.SchemaArgs.Original.ToKebabCase()}\" option to set schema similar to expression from the command line.",
            $"- Use \"{Current.ExecuteArgs.Alias}\" or \"--{Current.ExecuteArgs.Original.ToKebabCase()}\" option to execute SQL file or PSQL command on your current connection from the command line.",
            //$"- Use \"{Settings.OptionsArgs.Alias}\" or \"--{Settings.OptionsArgs.Original.ToKebabCase()}\" to set additional command options for the PSQL tool.",
            $"- Use \"{Current.DumpConsoleArgs.Alias}\" or \"--{Current.DumpConsoleArgs.Original.ToKebabCase()}\" switch to redirect all outputs to the command line.",
            $"- Use \"{Current.SilentArgs.Alias}\" or \"--{Current.SilentArgs.Original.ToKebabCase()}\" to silent not required console texts.",
            $"- Use \"{Current.ListArgs.Alias}\" or \"--{Current.ListArgs.Original.ToKebabCase()}\" to dump object list for current connection and with current schema.",
            $"- Use \"{Current.DefinitionArgs.Alias}\" or \"--{Current.DefinitionArgs.Original.ToKebabCase()}\" to dump object schema definition in console supplied as value parameter.",
            $"- Use \"{Current.InsertsArgs.Alias}\" or \"--{Current.InsertsArgs.Original.ToKebabCase()}\" to dump objects or queries (semicolon separated) inserts.");
        AddEntry(nameof(Current.Connection), Current.Value.Connection);
        AddEntry(nameof(Current.SkipConnectionPrompt), Current.Value.SkipConnectionPrompt);
        AddEntry(nameof(Current.DumpPgCommands), Current.Value.DumpPgCommands);
        AddEntry(nameof(Current.SchemaSimilarTo), Current.Value.SchemaSimilarTo);
        AddEntry(nameof(Current.SchemaNotSimilarTo), Current.Value.SchemaNotSimilarTo);
        AddEntry(nameof(Current.Execute), Current.Value.Execute);
        //AddEntry(nameof(Settings.Options), Settings.Value.Options);
        AddEntry(nameof(Current.List), Current.Value.List);
        AddEntry(nameof(Current.Definition), Current.Value.Definition);
        AddEntry(nameof(Current.Inserts), Current.Value.Inserts);
        AddEntry(nameof(Current.Backup), Current.Value.Backup);
        AddEntry(nameof(Current.BackupOwner), Current.Value.BackupOwner);
        AddEntry(nameof(Current.Restore), Current.Value.Restore);
        AddEntry(nameof(Current.RestoreOwner), Current.Value.RestoreOwner);
        AddEntry(nameof(Current.DumpConsole), Current.Value.DumpConsole);
        AddEntry(nameof(Current.Silent), Current.Value.Silent);
        AddEntry(nameof(Current.SkipIfExists), Current.Value.SkipIfExists);
        AddEntry(nameof(Current.SkipUpdateReferences), Current.Value.SkipUpdateReferences);
        AddEntry(nameof(Current.PgDump), Current.Value.PgDump);
        AddEntry(nameof(Current.PgDumpFallback), Current.Value.PgDumpFallback);
        AddEntry(nameof(Current.PgRestore), Current.Value.PgRestore);
        AddEntry(nameof(Current.PgRestoreFallback), Current.Value.PgRestoreFallback);
        AddEntry(nameof(Current.ConfigPath), Current.Value.ConfigPath);

        sb.AppendLine();
        AddSectionComment(
            "Code generation general settings. Used in:",
            null,
            $"- Routines code generation.",
            $"- CRUD code generation.");
        AddEntry(nameof(Current.Namespace), Current.Value.Namespace);
        AddEntry(nameof(Current.UseRecords), Current.Value.UseRecords);
        AddEntry(nameof(Current.UseExpressionBody), Current.Value.UseExpressionBody);
        AddEntry(nameof(Current.UseFileScopedNamespaces), Current.Value.UseFileScopedNamespaces);
        AddEntry(nameof(Current.UseNullableStrings), Current.Value.UseNullableStrings);
        AddEntry(nameof(Current.Mapping), Current.Value.Mapping);
        AddEntry(nameof(Current.CustomModels), Current.Value.CustomModels);
        AddEntry(nameof(Current.ModelDir), Current.Value.ModelDir);
        AddEntry(nameof(Current.ModelCustomNamespace), Current.Value.ModelCustomNamespace);
        AddEntry(nameof(Current.EmptyModelDir), Current.Value.EmptyModelDir);
        AddEntry(nameof(Current.SkipSyncMethods), Current.Value.SkipSyncMethods);
        AddEntry(nameof(Current.SkipAsyncMethods), Current.Value.SkipAsyncMethods);
        AddEntry(nameof(Current.MinNormVersion), Current.Value.MinNormVersion);
        AddEntry(nameof(Current.SourceHeaderLines), Current.Value.SourceHeaderLines);
        AddEntry(nameof(Current.Ident), Current.Value.Ident);
        AddEntry(nameof(Current.ReturnMethod), Current.Value.ReturnMethod);
        AddEntry(nameof(Current.MethodParameterNames), Current.Value.MethodParameterNames);

        sb.AppendLine();
        AddSectionComment(
            "Routines data-access extensions code-generation",
            null,
            $"- Use \"{Current.RoutinesArgs.Alias}\" or \"--{Current.RoutinesArgs.Original.ToKebabCase()}\" switch to run routines data-access extensions code-generation from the command line.",
            $"- Use \"{Current.OutputDirArgs.Alias}\" or \"--{Current.OutputDirArgs.Original.ToKebabCase()}\" option to set the output dir for the generated code from the command line.",
            $"- Use \"{Current.RoutinesOverwriteArgs.Alias}\" or \"--{Current.RoutinesOverwriteArgs.Original.ToKebabCase()}\" switch to set the overwrite mode for the generated code from the command line.",
            $"- Use \"{Current.ModelDirArgs.Alias}\" or \"--{Current.ModelDirArgs.Original.ToKebabCase()}\" option to set the custom models output dir for the generated code from the command line.");
        AddEntry(nameof(Current.Routines), Current.Value.Routines);
        AddEntry(nameof(Current.RoutinesSchemaSimilarTo), Current.Value.RoutinesSchemaSimilarTo);
        AddEntry(nameof(Current.RoutinesSchemaNotSimilarTo), Current.Value.RoutinesSchemaNotSimilarTo);
        AddEntry(nameof(Current.OutputDir), Current.Value.OutputDir);
        AddEntry(nameof(Current.RoutinesEmptyOutputDir), Current.Value.RoutinesEmptyOutputDir);
        AddEntry(nameof(Current.RoutinesOverwrite), Current.Value.RoutinesOverwrite);
        AddEntry(nameof(Current.RoutinesAskOverwrite), Current.Value.RoutinesAskOverwrite);
        AddEntry(nameof(Current.RoutinesNotSimilarTo), Current.Value.RoutinesNotSimilarTo);
        AddEntry(nameof(Current.RoutinesSimilarTo), Current.Value.RoutinesSimilarTo);
        AddEntry(nameof(Current.RoutinesReturnMethods), Current.Value.RoutinesReturnMethods);
        AddEntry(nameof(Current.RoutinesModelPropertyTypes), Current.Value.RoutinesModelPropertyTypes);
        AddEntry(nameof(Current.RoutinesUnknownReturnTypes), Current.Value.RoutinesUnknownReturnTypes);
        AddEntry(nameof(Current.RoutinesCallerInfo), Current.Value.RoutinesCallerInfo);
        AddEntry(nameof(Current.RoutinesLanguages), Current.Value.RoutinesLanguages);
        
        sb.AppendLine();
        AddSectionComment(
            "Unit tests code-generation settings",
            null,
            $"- Use \"{Current.UnitTestsArgs.Alias}\" or \"--{Current.UnitTestsArgs.Original.ToKebabCase()}\" switch to run unit tests code-generation from the command line.",
            $"- Use \"{Current.UnitTestsDirArgs.Alias}\" or \"--{Current.UnitTestsDirArgs.Original.ToKebabCase()}\" option to set the output dir for the generated unit test project from the command line.");
        AddEntry(nameof(Current.UnitTests), Current.Value.UnitTests);
        AddEntry(nameof(Current.UnitTestProjectTargetFramework), Current.Value.UnitTestProjectTargetFramework);
        AddEntry(nameof(Current.UnitTestProjectLangVersion), Current.Value.UnitTestProjectLangVersion);
        AddEntry(nameof(Current.UnitTestsDir), Current.Value.UnitTestsDir);
        AddEntry(nameof(Current.UnitTestsAskRecreate), Current.Value.UnitTestsAskRecreate);
        AddEntry(nameof(Current.UnitTestsSkipSyncMethods), Current.Value.UnitTestsSkipSyncMethods);
        AddEntry(nameof(Current.UnitTestsSkipAsyncMethods), Current.Value.UnitTestsSkipAsyncMethods);

        sb.AppendLine();
        AddSectionComment(
            "Schema dump script settings",
            null,
            $"- Use \"{Current.SchemaDumpArgs.Alias}\" or \"--{Current.SchemaDumpArgs.Original.ToKebabCase()}\" switch to run schema script dump from the command line.",
            $"- Use \"{Current.SchemaDumpFileArgs.Alias}\" or \"--{Current.SchemaDumpFileArgs.Original.ToKebabCase()}\" option to set generated schema file name from the command line.",
            $"- Use \"{Current.SchemaDumpOverwriteArgs.Alias}\" or \"--{Current.SchemaDumpOverwriteArgs.Original.ToKebabCase()}\" switch to set the overwrite mode for the generated schema file from the command line.",
            $"- Use \"--{nameof(Current.SchemaDumpPrivileges).ToKebabCase()}\" switch to include object privileges in schema file from the command line.");
        AddEntry(nameof(Current.SchemaDump), Current.Value.SchemaDump);
        AddEntry(nameof(Current.SchemaDumpFile), Current.Value.SchemaDumpFile);
        AddEntry(nameof(Current.SchemaDumpOverwrite), Current.Value.SchemaDumpOverwrite);
        AddEntry(nameof(Current.SchemaDumpAskOverwrite), Current.Value.SchemaDumpAskOverwrite);
        AddEntry(nameof(Current.SchemaDumpOwners), Current.Value.SchemaDumpOwners);
        AddEntry(nameof(Current.SchemaDumpPrivileges), Current.Value.SchemaDumpPrivileges);
        AddEntry(nameof(Current.SchemaDumpNoDropIfExists), Current.Value.SchemaDumpNoDropIfExists);
        AddEntry(nameof(Current.SchemaDumpOptions), Current.Value.SchemaDumpOptions);
        AddEntry(nameof(Current.SchemaDumpNoTransaction), Current.Value.SchemaDumpNoTransaction);

        sb.AppendLine();
        AddSectionComment(
            "Data dump script settings",
            null,
            $"- Use \"{Current.DataDumpArgs.Alias}\" or \"--{Current.DataDumpArgs.Original.ToKebabCase()}\" switch to run data script dump from the command line.",
            $"- Use \"{Current.DataDumpFileArgs.Alias}\" or \"--{Current.DataDumpFileArgs.Original.ToKebabCase()}\" option to set generated data script file name from the command line.",
            $"- Use \"{Current.DataDumpOverwriteArgs.Alias}\" or \"--{Current.DataDumpOverwriteArgs.Original.ToKebabCase()}\" switch to set the overwrite mode for the generated data script file from the command line.",
            $"- Use \"{Current.DataDumpListArgs.Alias}\" or \"--{Current.DataDumpListArgs.Original.ToKebabCase()}\" set semicolon separated list of tables or queries merged with \"DataDumpTables\" option and to be dumped.");
        AddEntry(nameof(Current.DataDump), Current.Value.DataDump);
        AddEntry(nameof(Current.DataDumpFile), Current.Value.DataDumpFile);
        AddEntry(nameof(Current.DataDumpOverwrite), Current.Value.DataDumpOverwrite);
        AddEntry(nameof(Current.DataDumpAskOverwrite), Current.Value.DataDumpAskOverwrite);
        AddEntry(nameof(Current.DataDumpList), Current.Value.DataDumpList);
        AddEntry(nameof(Current.DataDumpTables), Current.Value.DataDumpTables);
        AddEntry(nameof(Current.DataDumpOptions), Current.Value.DataDumpOptions);
        AddEntry(nameof(Current.DataDumpNoTransaction), Current.Value.DataDumpNoTransaction);
        AddEntry(nameof(Current.DataDumpRaw), Current.Value.DataDumpRaw);

        sb.AppendLine();
        AddSectionComment(
            "Object file tree settings",
            null,
            $"- Use \"{Current.DbObjectsArgs.Alias}\" or \"--{Current.DbObjectsArgs.Original.ToKebabCase()}\" switch to run object files tree dump from the command line.",
            $"- Use \"{Current.DbObjectsDirArgs.Alias}\" or \"--{Current.DbObjectsDirArgs.Original.ToKebabCase()}\" option to set the root output dir from the command line.",
            $"- Use \"{Current.DbObjectsOverwriteArgs.Alias}\" or \"--{Current.DbObjectsOverwriteArgs.Original.ToKebabCase()}\" switch to set the overwrite mode for the generated files from the command line.");
        AddEntry(nameof(Current.DbObjects), Current.Value.DbObjects);
        AddEntry(nameof(Current.DbObjectsDir), Current.Value.DbObjectsDir);
        AddEntry(nameof(Current.DbObjectsOverwrite), Current.Value.DbObjectsOverwrite);
        AddEntry(nameof(Current.DbObjectsAskOverwrite), Current.Value.DbObjectsAskOverwrite);
        AddEntry(nameof(Current.DbObjectsDirNames), Current.Value.DbObjectsDirNames);
        AddEntry(nameof(Current.DbObjectsSkipDeleteDir), Current.Value.DbObjectsSkipDeleteDir);
        //AddEntry(nameof(Settings.DbObjectsRemoveExistingDirs), Settings.Value.DbObjectsRemoveExistingDirs);
        AddEntry(nameof(Current.DbObjectsOwners), Current.Value.DbObjectsOwners);
        AddEntry(nameof(Current.DbObjectsPrivileges), Current.Value.DbObjectsPrivileges);
        AddEntry(nameof(Current.DbObjectsCreateOrReplace), Current.Value.DbObjectsCreateOrReplace);
        AddEntry(nameof(Current.DbObjectsRaw), Current.Value.DbObjectsRaw);

        sb.AppendLine();
        AddSectionComment(
            "Markdown (MD) database dictionaries settings",
            null,
            $"- Use \"{Current.MarkdownArgs.Alias}\" or \"--{Current.MarkdownArgs.Original.ToKebabCase()}\" switch to run markdown (MD) database dictionary file from the command line.",
            $"- Use \"{Current.MdFileArgs.Alias}\" or \"--{Current.MdFileArgs.Original.ToKebabCase()}\" option to set generated dictionary file name from the command line.",
            $"- Use \"{Current.CommitMdArgs.Alias}\" or \"--{Current.CommitMdArgs.Original.ToKebabCase()}\" switch to run commit changes in comments from the MD file back to the database from the command line.");
        AddEntry(nameof(Current.Markdown), Current.Value.Markdown);
        AddEntry(nameof(Current.MdFile), Current.Value.MdFile);
        AddEntry(nameof(Current.MdSchemaSimilarTo), Current.Value.MdSchemaSimilarTo);
        AddEntry(nameof(Current.MdSchemaNotSimilarTo), Current.Value.MdSchemaNotSimilarTo);
        AddEntry(nameof(Current.MdOverwrite), Current.Value.MdOverwrite);
        AddEntry(nameof(Current.MdAskOverwrite), Current.Value.MdAskOverwrite);
        AddEntry(nameof(Current.MdSkipRoutines), Current.Value.MdSkipRoutines);
        AddEntry(nameof(Current.MdSkipViews), Current.Value.MdSkipViews);
        AddEntry(nameof(Current.MdSkipEnums), Current.Value.MdSkipEnums);
        AddEntry(nameof(Current.MdNotSimilarTo), Current.Value.MdNotSimilarTo);
        AddEntry(nameof(Current.MdSimilarTo), Current.Value.MdSimilarTo);
        AddEntry(nameof(Current.MdIncludeSourceLinks), Current.Value.MdIncludeSourceLinks);
        AddEntry(nameof(Current.MdIncludeExtensionLinks), Current.Value.MdIncludeExtensionLinks);
        AddEntry(nameof(Current.MdIncludeUnitTestsLinks), Current.Value.MdIncludeUnitTestsLinks);
        AddEntry(nameof(Current.MdSourceLinkRoot), Current.Value.MdSourceLinkRoot);
        AddEntry(nameof(Current.MdIncludeTableCountEstimates), Current.Value.MdIncludeTableCountEstimates);
        AddEntry(nameof(Current.MdIncludeTableStats), Current.Value.MdIncludeTableStats);
        AddEntry(nameof(Current.MdRoutinesFirst), Current.Value.MdRoutinesFirst);
        AddEntry(nameof(Current.MdExportToHtml), Current.Value.MdExportToHtml);
        AddEntry(nameof(Current.CommitMd), Current.Value.CommitMd);

        sb.AppendLine();
        AddSectionComment(
            "PSQL command-line utility settings",
            null,
            $"- Use \"{Current.PsqlArgs.Alias}\" or \"--{Current.PsqlArgs.Original.ToKebabCase()}\" switch to open PSQL command-line utility from the command line.");
        AddEntry(nameof(Current.Psql), Current.Value.Psql);
        AddEntry(nameof(Current.PsqlTerminal), Current.Value.PsqlTerminal);
        AddEntry(nameof(Current.PsqlCommand), Current.Value.PsqlCommand);
        AddEntry(nameof(Current.PsqlFallback), Current.Value.PsqlFallback);
        AddEntry(nameof(Current.PsqlOptions), Current.Value.PsqlOptions);

        sb.AppendLine();
        AddSectionComment(
            "Diff scripts settings",
            null,
            $"- Use \"{Current.DiffArgs.Alias}\" or \"--{Current.DiffArgs.Original.ToKebabCase()}\" switch to run diff script generation from the command line.",
            $"- Use \"{Current.DiffTargetArgs.Alias}\" or \"--{Current.DiffTargetArgs.Original.ToKebabCase()}\" option to set target connection for the diff script generator from the command line.");
        AddEntry(nameof(Current.Diff), Current.Value.Diff);
        AddEntry(nameof(Current.DiffTarget), Current.Value.DiffTarget);
        AddEntry(nameof(Current.DiffFilePattern), Current.Value.DiffFilePattern);
        AddEntry(nameof(Current.DiffPgDump), Current.Value.DiffPgDump);
        AddEntry(nameof(Current.DiffPrivileges), Current.Value.DiffPrivileges);
        AddEntry(nameof(Current.DiffSkipSimilarTo), Current.Value.DiffSkipSimilarTo);

        sb.AppendLine();
        AddSectionComment(
            "CRUD scripts settings",
            null,
            $"- Use \"{Current.CrudArgs.Alias}\" or \"--{Current.CrudArgs.Original.ToKebabCase()}\" switch to run CRUD extension methods generation from the command line.",
            $"- Use \"{Current.CrudOutputDirArgs.Alias}\" or \"--{Current.CrudOutputDirArgs.Original.ToKebabCase()}\" option to set the custom models output dir for the generated CRUD extension methods code from the command line.");
        AddEntry(nameof(Current.Crud), Current.Value.Crud);
        AddEntry(nameof(Current.CrudOutputDir), Current.Value.CrudOutputDir);
        AddEntry(nameof(Current.CrudEmptyOutputDir), Current.Value.CrudEmptyOutputDir);
        AddEntry(nameof(Current.CrudOverwrite), Current.Value.CrudOverwrite);
        AddEntry(nameof(Current.CrudAskOverwrite), Current.Value.CrudAskOverwrite);
        AddEntry(nameof(Current.CrudNoPrepare), Current.Value.CrudNoPrepare);
        AddEntry(nameof(Current.CrudReturnMethods), Current.Value.CrudReturnMethods);
        AddEntry(nameof(Current.CrudCreate), Current.Value.CrudCreate);
        AddEntry(nameof(Current.CrudCreateReturning), Current.Value.CrudCreateReturning);
        AddEntry(nameof(Current.CrudCreateOnConflictDoNothing), Current.Value.CrudCreateOnConflictDoNothing);
        AddEntry(nameof(Current.CrudCreateOnConflictDoNothingReturning), Current.Value.CrudCreateOnConflictDoNothingReturning);
        AddEntry(nameof(Current.CrudCreateOnConflictDoUpdate), Current.Value.CrudCreateOnConflictDoUpdate);
        AddEntry(nameof(Current.CrudCreateOnConflictDoUpdateReturning), Current.Value.CrudCreateOnConflictDoUpdateReturning);
        AddEntry(nameof(Current.CrudReadBy), Current.Value.CrudReadBy);
        AddEntry(nameof(Current.CrudReadAll), Current.Value.CrudReadAll);
        AddEntry(nameof(Current.CrudUpdate), Current.Value.CrudUpdate);
        AddEntry(nameof(Current.CrudUpdateReturning), Current.Value.CrudUpdateReturning);
        AddEntry(nameof(Current.CrudDeleteBy), Current.Value.CrudDeleteBy);
        AddEntry(nameof(Current.CrudDeleteByReturning), Current.Value.CrudDeleteByReturning, "");

        if (wrap)
        {
            sb.AppendLine("  }");
            sb.AppendLine("}");
        }

        return sb.ToString();
    }
}
