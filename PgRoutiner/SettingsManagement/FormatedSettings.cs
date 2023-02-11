namespace PgRoutiner.SettingsManagement;

public class FormatedSettings
{
    public static (string header, List<(string cmds, string help)>) SettingHelp() => (
        "\"PgRoutiner\" configuration section is used to configure PgRoutiner. Use command line to override these values:",
        GetHelpTexts(
            (Current.VersionArgs, "(switch) to check out current version."),
            (Current.InfoArgs, "(switch) show environment info."),
            (Current.SettingsArgs, "(switch) to see all currently active settings, including command lines overrides and all configuration files."),
            (Current.DumpConsoleArgs, "(switch) to redirect all results into console output instead of files or database."),
            (Current.WriteConfigFileArgs, "to create new configuration file based on current settings."),
            (Current.ConfigFileArgs, "to load additional custom configuration file."),
            (Current.OverwriteArgs, "(switch) to force overwrite existing files for each generated files."),
            (Current.AskOverwriteArgs, "(switch) to prompt overwrite question of existing files for each generated files.")
        ));

    public static (string header, List<(string cmds, string help)>) GeneralSettingHelp() => (
        "General settings:", 
        GetHelpTexts(
            (Current.ConnectionArgs, "to set working connection from the command line."),
            (Current.SchemaArgs, "to set schema similar to expression from the command line."),
            (Current.ExecuteArgs, "option to execute SQL file or PSQL command on your current connection from the command line."),
            (Current.SilentArgs, "to silent not required console texts."),
            (Current.ListArgs, "to dump or search object list to console. Use switch to dump all objects or parameter value to search."),
            (Current.DefinitionArgs, "to dump object schema definition in console supplied as value parameter."),
            (Current.SearchArgs, "to search object schema definitions and dump highlighted results to console."),
            (Current.InsertsArgs, "to dump objects or queries (semicolon separated) insert definitions.")
        ));

    public static (string header, List<(string cmds, string help)>) CodeGenGeneralSettingHelp() => (
        "Code generation general settings:",
        GetHelpTexts(
            (Current.ModelDirArgs, "to set model output directory."),
            (Current.SkipSyncMethodsArgs, "to skip sync methods generation."),
            (Current.SkipAsyncMethodsArgs, "to skip async methods generation.")
        ));

    public static (string header, List<(string cmds, string help)>) RoutinesSettingHelp() => (
        "Routines data-access extensions code-generation:",
        GetHelpTexts(
            (Current.RoutinesArgs, "(switch) to run routines data-access extensions code-generation from the command line."),
            (Current.OutputDirArgs, "to set the output dir for the generated code from the command line."),
            (Current.RoutinesSimilarToArgs, "to set \"similar to\" expression that matches routines names to be included."),
            (Current.RoutinesNotSimilarToArgs, "to set \"not similar to\" expression that matches routines names not to be included.")
        ));

    public static (string header, List<(string cmds, string help)>) UnitTestsSettingHelp() => (
        "Unit tests code-generation settings:",
        GetHelpTexts(
            (Current.UnitTestsArgs, "to run unit tests templates code-generation from the command line."),
            (Current.UnitTestsDirArgs, "to set the output dir for the generated unit test project from the command line.")
        ));

    public static (string header, List<(string cmds, string help)>) SchemaDumpSettingHelp() => (
        "Schema dump script settings:",
        GetHelpTexts(
            (Current.SchemaDumpArgs, "to run schema script dump from the command line."),
            (Current.SchemaDumpFileArgs, "to set generated schema file name from the command line.")
        ));

    public static (string header, List<(string cmds, string help)>) DataDumpSettingHelp() => (
        "Schema dump script settings:",
        GetHelpTexts(
            (Current.DataDumpArgs, "to run data script dump from the command line."),
            (Current.DataDumpFileArgs, "to set generated data script file name from the command line."),
            (Current.DataDumpListArgs, "set semicolon separated list of tables or queries merged with \"DataDumpTables\" option and to be dumped.")
        ));

    public static (string header, List<(string cmds, string help)>) ObjTreeSettingHelp() => (
        "Object file tree settings:",
        GetHelpTexts(
            (Current.DbObjectsArgs, "(switch) to run object files tree dump from the command line."),
            (Current.DbObjectsDirArgs, "to set the root output dir from the command line.")
        ));

    public static (string header, List<(string cmds, string help)>) PsqlSettingHelp() => (
        "PSQL command-line utility settings:",
        GetHelpTexts(
            (Current.PsqlArgs, "(switch) to open PSQL command-line utility from the command line.")
        ));
    
    public static (string header, List<(string cmds, string help)>) MarkdownSettingHelp() => (
        "Markdown (MD) database dictionaries settings:",
        GetHelpTexts(
            (Current.MarkdownArgs, "(switch) to run markdown (MD) database dictionary file from the command line."),
            (Current.MdFileArgs, "to set generated dictionary file name from the command line."),
            (Current.CommitMdArgs, "(switch) to run commit changes in comments from the MD file back to the database from the command line.")
        ));

    public static (string header, List<(string cmds, string help)>) ModelSettingHelp() => (
        "Model output from a query, table, or enum settings:",
        GetHelpTexts(
            (Current.ModelOutputArgs, "to set file name with expressions from which to build models. If file doesn't exists, expressions are literal. It can be one or more queries or table/view/enum names separated by semicolon."),
            (Current.ModelOutputFileArgs, "to set a single file name for models output. If this file extensions is \"ts\" it will generate TypeScript code, otherwise it will generate CSharp code. To generate TypeScript code to console set this value to \".ts\"."),
            (Current.ModelSaveToModelDirArgs, "(switch) to enable saving each generated model file to model dir.")
        ));

    public static (string header, List<(string cmds, string help)>) CrudSettingHelp() => (
        "CRUD to routines settings:",
        GetHelpTexts(
            (Current.CrudCreateArgs, "similar to expression of table names to generate create routines."),
            (Current.CrudCreateReturningArgs, "similar to expression of table names to generate create returning routines."),
            (Current.CrudCreateOnConflictDoNothingArgs, "similar to expression of table names to generate create routines that will do nothing on primary key(s) conflicts."),
            (Current.CrudCreateOnConflictDoNothingReturningArgs, "similar to expression of table names to generate create returning routines that will do nothing on primary key(s) conflicts."),
            (Current.CrudCreateOnConflictDoUpdateArgs, "similar to expression of table names to generate create routines that will update on primary key(s) conflicts."),
            (Current.CrudCreateOnConflictDoUpdateReturningArgs, "similar to expression of table names to generate create returning routines that will update on primary key(s) conflicts."),
            (Current.CrudReadByArgs, "similar to expression of table names to generate read by primary key(s) routines."),
            (Current.CrudReadAllArgs, "similar to expression of table names to generate read all data routines."),
            (Current.CrudReadPageArgs, "similar to expression of table names to generate search and read page routines."),
            (Current.CrudUpdateArgs, "similar to expression of table names to generate update routines."),
            (Current.CrudUpdateReturningArgs, "similar to expression of table names to generate update returning routines."),
            (Current.CrudDeleteByArgs, "similar to expression of table names to generate delete by primary key(s) routines."),
            (Current.CrudDeleteByReturningArgs, "similar to expression of table names to generate delete returning by primary key(s) routines.")
        ));

    public static string Build(bool wrap = true, NpgsqlConnection connection = null)
    {
        StringBuilder sb = new();

        void AddComment((string header, List<(string cmds, string help)> args) section, int spaces = 4)
        {
            var s = string.Join("", Enumerable.Repeat(" ", spaces));
            var s2 = string.Join("", Enumerable.Repeat(" ", spaces + 2));
            sb.AppendLine($"{s}/*");
            if (section.header != null)
            {
                sb.AppendLine($"{s2}{section.header}");
            }
            foreach(var entry in section.args)
            {
                sb.AppendLine($"{s2}{entry.cmds} {entry.help}");
            }
            sb.AppendLine($"{s}*/");
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
            }
            if (connection != null)
            {
                sb.AppendLine($"    \"{Current.Value.Connection}\": \"{connection.ToPsqlFormatString()}\"");
            }
            sb.AppendLine("    //\"Connection1\": \"Server={server};Db={database};Port={port};User Id={user};Password={password};\"");
            sb.AppendLine("    //\"Connection2\": \"postgresql://{user}:{password}@{server}:{port}/{database}\"");

            sb.AppendLine("  },");
            //sb.AppendLine("  /* see https://github.com/vb-consulting/PgRoutiner#readme for more info */");
            AddComment(SettingHelp(), 2);
            sb.AppendLine("  \"PgRoutiner\": {");
            sb.AppendLine();
        }

        AddComment(GeneralSettingHelp());
        AddEntry(nameof(Current.Connection), Current.Value.Connection);
        AddEntry(nameof(Current.SkipConnectionPrompt), Current.Value.SkipConnectionPrompt);
        AddEntry(nameof(Current.DumpPgCommands), Current.Value.DumpPgCommands);
        AddEntry(nameof(Current.SchemaSimilarTo), Current.Value.SchemaSimilarTo);
        AddEntry(nameof(Current.SchemaNotSimilarTo), Current.Value.SchemaNotSimilarTo);
        AddEntry(nameof(Current.Execute), Current.Value.Execute);
        //AddEntry(nameof(Settings.Options), Settings.Value.Options);
        AddEntry(nameof(Current.List), Current.Value.List);
        AddEntry(nameof(Current.Definition), Current.Value.Definition);
        AddEntry(nameof(Current.Search), Current.Value.Search);
        AddEntry(nameof(Current.Inserts), Current.Value.Inserts);
        AddEntry(nameof(Current.Backup), Current.Value.Backup);
        AddEntry(nameof(Current.BackupOwner), Current.Value.BackupOwner);
        AddEntry(nameof(Current.Restore), Current.Value.Restore);
        AddEntry(nameof(Current.RestoreOwner), Current.Value.RestoreOwner);
        AddEntry(nameof(Current.DumpConsole), Current.Value.DumpConsole);
        AddEntry(nameof(Current.Silent), Current.Value.Silent);
        AddEntry(nameof(Current.Verbose), Current.Value.Verbose);
        AddEntry(nameof(Current.SkipIfExists), Current.Value.SkipIfExists);
        AddEntry(nameof(Current.SkipUpdateReferences), Current.Value.SkipUpdateReferences);
        AddEntry(nameof(Current.PgDump), Current.Value.PgDump);
        AddEntry(nameof(Current.PgDumpFallback), Current.Value.PgDumpFallback);
        AddEntry(nameof(Current.PgRestore), Current.Value.PgRestore);
        AddEntry(nameof(Current.PgRestoreFallback), Current.Value.PgRestoreFallback);
        AddEntry(nameof(Current.ConfigPath), Current.Value.ConfigPath);
        AddEntry(nameof(Current.Overwrite), Current.Value.Overwrite);
        AddEntry(nameof(Current.AskOverwrite), Current.Value.AskOverwrite);

        sb.AppendLine();

        AddComment(CodeGenGeneralSettingHelp());
        AddEntry(nameof(Current.Namespace), Current.Value.Namespace);
        AddEntry(nameof(Current.UseRecords), Current.Value.UseRecords);
        AddEntry(nameof(Current.UseRecordsForModels), Current.Value.UseRecordsForModels);
        //AddEntry(nameof(Current.UseExpressionBody), Current.Value.UseExpressionBody);
        AddEntry(nameof(Current.UseFileScopedNamespaces), Current.Value.UseFileScopedNamespaces);
        AddEntry(nameof(Current.UseNullableTypes), Current.Value.UseNullableTypes);
        AddEntry(nameof(Current.Mapping), Current.Value.Mapping);
        AddEntry(nameof(Current.CustomModels), Current.Value.CustomModels);
        AddEntry(nameof(Current.ModelDir), Current.Value.ModelDir);
        AddEntry(nameof(Current.ModelCustomNamespace), Current.Value.ModelCustomNamespace);
        AddEntry(nameof(Current.EmptyModelDir), Current.Value.EmptyModelDir);
        AddEntry(nameof(Current.SkipSyncMethods), Current.Value.SkipSyncMethods);
        AddEntry(nameof(Current.SkipAsyncMethods), Current.Value.SkipAsyncMethods);
        //AddEntry(nameof(Current.MinNormVersion), Current.Value.MinNormVersion);
        AddEntry(nameof(Current.SourceHeaderLines), Current.Value.SourceHeaderLines);
        AddEntry(nameof(Current.Ident), Current.Value.Ident);
        //AddEntry(nameof(Current.ReturnMethod), Current.Value.ReturnMethod);
        AddEntry(nameof(Current.MethodParameterNames), Current.Value.MethodParameterNames);

        sb.AppendLine();
        AddComment(RoutinesSettingHelp());
        AddEntry(nameof(Current.Routines), Current.Value.Routines);
        AddEntry(nameof(Current.RoutinesSimilarTo), Current.Value.RoutinesSimilarTo);
        AddEntry(nameof(Current.RoutinesNotSimilarTo), Current.Value.RoutinesNotSimilarTo);
        AddEntry(nameof(Current.RoutinesSchemaSimilarTo), Current.Value.RoutinesSchemaSimilarTo);
        AddEntry(nameof(Current.RoutinesSchemaNotSimilarTo), Current.Value.RoutinesSchemaNotSimilarTo);
        AddEntry(nameof(Current.OutputDir), Current.Value.OutputDir);
        AddEntry(nameof(Current.RoutinesEmptyOutputDir), Current.Value.RoutinesEmptyOutputDir);
        //AddEntry(nameof(Current.RoutinesOverwrite), Current.Value.RoutinesOverwrite);
        //AddEntry(nameof(Current.RoutinesAskOverwrite), Current.Value.RoutinesAskOverwrite);
        AddEntry(nameof(Current.RoutinesReturnMethods), Current.Value.RoutinesReturnMethods);
        AddEntry(nameof(Current.RoutinesModelPropertyTypes), Current.Value.RoutinesModelPropertyTypes);
        AddEntry(nameof(Current.RoutinesUnknownReturnTypes), Current.Value.RoutinesUnknownReturnTypes);
        AddEntry(nameof(Current.RoutinesCallerInfo), Current.Value.RoutinesCallerInfo);
        AddEntry(nameof(Current.RoutinesLanguages), Current.Value.RoutinesLanguages);
        AddEntry(nameof(Current.RoutinesCustomCodeLines), Current.Value.RoutinesCustomCodeLines);
        AddEntry(nameof(Current.RoutinesCancellationToken), Current.Value.RoutinesCancellationToken);
        AddEntry(nameof(Current.CustomDirs), Current.Value.CustomDirs);
        AddEntry(nameof(Current.RoutinesIncludeDefintionInComment), Current.Value.RoutinesIncludeDefintionInComment);
        AddEntry(nameof(Current.RoutinesOpenConnectionIfClosed), Current.Value.RoutinesOpenConnectionIfClosed);

        sb.AppendLine();
        AddComment(UnitTestsSettingHelp());
        AddEntry(nameof(Current.UnitTests), Current.Value.UnitTests);
        AddEntry(nameof(Current.UnitTestProjectTargetFramework), Current.Value.UnitTestProjectTargetFramework);
        AddEntry(nameof(Current.UnitTestProjectLangVersion), Current.Value.UnitTestProjectLangVersion);
        AddEntry(nameof(Current.UnitTestsDir), Current.Value.UnitTestsDir);
        AddEntry(nameof(Current.UnitTestsAskRecreate), Current.Value.UnitTestsAskRecreate);
        AddEntry(nameof(Current.UnitTestsSkipSyncMethods), Current.Value.UnitTestsSkipSyncMethods);
        AddEntry(nameof(Current.UnitTestsSkipAsyncMethods), Current.Value.UnitTestsSkipAsyncMethods);

        sb.AppendLine();
        AddComment(SchemaDumpSettingHelp());
        AddEntry(nameof(Current.SchemaDump), Current.Value.SchemaDump);
        AddEntry(nameof(Current.SchemaDumpFile), Current.Value.SchemaDumpFile);
        //AddEntry(nameof(Current.SchemaDumpOverwrite), Current.Value.SchemaDumpOverwrite);
        //AddEntry(nameof(Current.SchemaDumpAskOverwrite), Current.Value.SchemaDumpAskOverwrite);
        AddEntry(nameof(Current.SchemaDumpOwners), Current.Value.SchemaDumpOwners);
        AddEntry(nameof(Current.SchemaDumpPrivileges), Current.Value.SchemaDumpPrivileges);
        AddEntry(nameof(Current.SchemaDumpNoDropIfExists), Current.Value.SchemaDumpNoDropIfExists);
        AddEntry(nameof(Current.SchemaDumpOptions), Current.Value.SchemaDumpOptions);
        AddEntry(nameof(Current.SchemaDumpNoTransaction), Current.Value.SchemaDumpNoTransaction);

        sb.AppendLine();
        AddComment(DataDumpSettingHelp());
        AddEntry(nameof(Current.DataDump), Current.Value.DataDump);
        AddEntry(nameof(Current.DataDumpFile), Current.Value.DataDumpFile);
        //AddEntry(nameof(Current.DataDumpOverwrite), Current.Value.DataDumpOverwrite);
        //AddEntry(nameof(Current.DataDumpAskOverwrite), Current.Value.DataDumpAskOverwrite);
        AddEntry(nameof(Current.DataDumpList), Current.Value.DataDumpList);
        AddEntry(nameof(Current.DataDumpTables), Current.Value.DataDumpTables);
        AddEntry(nameof(Current.DataDumpOptions), Current.Value.DataDumpOptions);
        AddEntry(nameof(Current.DataDumpNoTransaction), Current.Value.DataDumpNoTransaction);
        AddEntry(nameof(Current.DataDumpRaw), Current.Value.DataDumpRaw);

        sb.AppendLine();
        AddComment(ObjTreeSettingHelp());
        AddEntry(nameof(Current.DbObjects), Current.Value.DbObjects);
        AddEntry(nameof(Current.DbObjectsDir), Current.Value.DbObjectsDir);
        //AddEntry(nameof(Current.DbObjectsOverwrite), Current.Value.DbObjectsOverwrite);
        //AddEntry(nameof(Current.DbObjectsAskOverwrite), Current.Value.DbObjectsAskOverwrite);
        AddEntry(nameof(Current.DbObjectsDirNames), Current.Value.DbObjectsDirNames);
        AddEntry(nameof(Current.DbObjectsSkipDeleteDir), Current.Value.DbObjectsSkipDeleteDir);
        //AddEntry(nameof(Settings.DbObjectsRemoveExistingDirs), Settings.Value.DbObjectsRemoveExistingDirs);
        AddEntry(nameof(Current.DbObjectsOwners), Current.Value.DbObjectsOwners);
        AddEntry(nameof(Current.DbObjectsPrivileges), Current.Value.DbObjectsPrivileges);
        AddEntry(nameof(Current.DbObjectsCreateOrReplace), Current.Value.DbObjectsCreateOrReplace);
        AddEntry(nameof(Current.DbObjectsRaw), Current.Value.DbObjectsRaw);
        AddEntry(nameof(Current.DbObjectsSchema), Current.Value.DbObjectsSchema);

        sb.AppendLine();
        AddComment(MarkdownSettingHelp());
        AddEntry(nameof(Current.Markdown), Current.Value.Markdown);
        AddEntry(nameof(Current.MdFile), Current.Value.MdFile);
        AddEntry(nameof(Current.MdSchemaSimilarTo), Current.Value.MdSchemaSimilarTo);
        AddEntry(nameof(Current.MdSchemaNotSimilarTo), Current.Value.MdSchemaNotSimilarTo);
        //AddEntry(nameof(Current.MdOverwrite), Current.Value.MdOverwrite);
        //AddEntry(nameof(Current.MdAskOverwrite), Current.Value.MdAskOverwrite);
        AddEntry(nameof(Current.MdSkipHeader), Current.Value.MdSkipHeader);
        AddEntry(nameof(Current.MdSkipToc), Current.Value.MdSkipToc);
        AddEntry(nameof(Current.MdSkipTables), Current.Value.MdSkipTables);
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
        AddEntry(nameof(Current.MdAdditionalCommentsSql), Current.Value.MdAdditionalCommentsSql);
        AddEntry(nameof(Current.MdExportToHtml), Current.Value.MdExportToHtml);
        AddEntry(nameof(Current.CommitMd), Current.Value.CommitMd);

        sb.AppendLine();
        AddComment(PsqlSettingHelp());
        AddEntry(nameof(Current.Psql), Current.Value.Psql);
        AddEntry(nameof(Current.PsqlTerminal), Current.Value.PsqlTerminal);
        AddEntry(nameof(Current.PsqlCommand), Current.Value.PsqlCommand);
        AddEntry(nameof(Current.PsqlFallback), Current.Value.PsqlFallback);
        AddEntry(nameof(Current.PsqlOptions), Current.Value.PsqlOptions);

        //sb.AppendLine();
        //AddSectionComment(
        //    "Diff scripts settings",
        //    null,
        //    $"- Use \"{Current.DiffArgs.Alias}\" or \"--{Current.DiffArgs.Original.ToKebabCase()}\" switch to run diff script generation from the command line.",
        //    $"- Use \"{Current.DiffTargetArgs.Alias}\" or \"--{Current.DiffTargetArgs.Original.ToKebabCase()}\" option to set target connection for the diff script generator from the command line.");
        //AddEntry(nameof(Current.Diff), Current.Value.Diff);
        //AddEntry(nameof(Current.DiffTarget), Current.Value.DiffTarget);
        //AddEntry(nameof(Current.DiffFilePattern), Current.Value.DiffFilePattern);
        //AddEntry(nameof(Current.DiffPgDump), Current.Value.DiffPgDump);
        //AddEntry(nameof(Current.DiffPrivileges), Current.Value.DiffPrivileges);
        //AddEntry(nameof(Current.DiffSkipSimilarTo), Current.Value.DiffSkipSimilarTo);

        sb.AppendLine();
        AddComment(ModelSettingHelp());
        AddEntry(nameof(Current.ModelOutput), Current.Value.ModelOutput);
        AddEntry(nameof(Current.ModelOutputFile), Current.Value.ModelOutputFile);
        AddEntry(nameof(Current.ModelSaveToModelDir), Current.Value.ModelSaveToModelDir);

        sb.AppendLine();
        AddComment(CrudSettingHelp());
        //AddEntry(nameof(Current.CrudUseAtomic), Current.Value.CrudUseAtomic);
        AddEntry(nameof(Current.CrudFunctionAttributes), Current.Value.CrudFunctionAttributes);
        AddEntry(nameof(Current.CrudCoalesceDefaults), Current.Value.CrudCoalesceDefaults);
        AddEntry(nameof(Current.CrudNamePattern), Current.Value.CrudNamePattern);
        AddEntry(nameof(Current.CrudCreate), Current.Value.CrudCreate);
        AddEntry(nameof(Current.CrudCreateReturning), Current.Value.CrudCreateReturning);
        AddEntry(nameof(Current.CrudCreateOnConflictDoNothing), Current.Value.CrudCreateOnConflictDoNothing);
        AddEntry(nameof(Current.CrudCreateOnConflictDoNothingReturning), Current.Value.CrudCreateOnConflictDoNothingReturning);
        AddEntry(nameof(Current.CrudCreateOnConflictDoUpdate), Current.Value.CrudCreateOnConflictDoUpdate);
        AddEntry(nameof(Current.CrudCreateOnConflictDoUpdateReturning), Current.Value.CrudCreateOnConflictDoUpdateReturning);
        AddEntry(nameof(Current.CrudReadBy), Current.Value.CrudReadBy);
        AddEntry(nameof(Current.CrudReadAll), Current.Value.CrudReadAll);
        AddEntry(nameof(Current.CrudReadPage), Current.Value.CrudReadPage);
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

    private static List<(string cmds, string help)> GetHelpTexts(params (Arg arg, string help)[] values)
    {
        IEnumerable<string> ReplacementsArg(Arg arg)
        {
            var original = $"--{arg.Original.ToKebabCase()}";
            foreach (var (rep, val) in Arg.ArgReplacements)
            {
                if (original == val)
                {
                    yield return rep;
                }
            }
        }
        List<(string cmds, string help)> result = new(values.Length);
        foreach (var value in values)
        {
            List<string> commands = new()
            {
                value.arg.Alias,
                $"--{value.arg.Original.ToKebabCase()}"
            };
            commands.AddRange(ReplacementsArg(value.arg));
            result.Add((string.Join(" ", commands.OrderBy(c => c.Length)), value.help));
        }
        return result;
    }
}
