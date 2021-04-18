using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Npgsql;

namespace PgRoutiner
{
    public partial class Settings
    {
        public static string BuildFormatedSettings(bool wrap = true, NpgsqlConnection connection = null)
        {
            StringBuilder sb = new();

            void AddSectionComment(string header, string helpUrl, params string[] tips)
            {
                sb.AppendLine($"    /*");
                sb.AppendLine($"      {header}");
                foreach(var tip in tips)
                {
                    sb.AppendLine($"      {tip}");
                }
                if (helpUrl != null)
                {
                    sb.AppendLine($"      - For more info see: {helpUrl}");
                }
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
                if (Value.Connection == null && connection != null)
                {
                    Value.Connection = $"{connection.Database.ToUpperCamelCase()}Connection";
                    sb.AppendLine($"    \"{Value.Connection}\": \"{connection.ToPsqlFormatString()}\"");
                }
                sb.AppendLine("    //\"Connection1\": \"Server={server};Db={database};Port={port};User Id={user};Password={password};\"");
                sb.AppendLine("    //\"Connection2\": \"postgresql://{user}:{password}@{server}:{port}/{database}\" ");

                sb.AppendLine("  },");
                sb.AppendLine("  /* see https://github.com/vb-consulting/PgRoutiner/wiki/1.-WORKING-WITH-SETTINGS for more info */");
                sb.AppendLine("  \"PgRoutiner\": {");
                sb.AppendLine();
            }

            AddSectionComment(
                "General settings:", 
                "https://github.com/vb-consulting/PgRoutiner/wiki/1.-WORKING-WITH-SETTINGS#general-settings",
                $"- Use \"{ConnectionArgs.Alias}\" or \"--{ConnectionArgs.Original.ToKebabCase()}\" option to set working connection from the command line.",
                $"- Use \"{SchemaArgs.Alias}\" or \"--{SchemaArgs.Original.ToKebabCase()}\" option to set schema similar to expression from the command line.",
                $"- Use \"{ExecuteArgs.Alias}\" or \"--{ExecuteArgs.Original.ToKebabCase()}\" option to execute SQL file or PSQL command on your current connection  from the command line.",
                $"- Use \"{DumpArgs.Alias}\" or \"--{DumpArgs.Original.ToKebabCase()}\" switch to redirect all outputs to the command line.");
            AddEntry(nameof(Connection), Value.Connection);
            AddEntry(nameof(SkipConnectionPrompt), Value.SkipConnectionPrompt);
            AddEntry(nameof(Schema), Value.Schema);
            AddEntry(nameof(Execute), Value.Execute);
            AddEntry(nameof(Dump), Value.Dump);
            AddEntry(nameof(SkipIfExists), Value.SkipIfExists);
            AddEntry(nameof(SkipUpdateReferences), Value.SkipUpdateReferences);
            AddEntry(nameof(PgDump), Value.PgDump);
            AddEntry(nameof(PgDumpFallback), Value.PgDumpFallback);

            sb.AppendLine();
            AddSectionComment(
                "Code generation general settings. Used in:", 
                null,
                $"- Routines code generation.",
                $"- CRUD code generation.");
            AddEntry(nameof(Namespace), Value.Namespace);
            AddEntry(nameof(UseRecords), Value.UseRecords);
            AddEntry(nameof(UseExpressionBody), Value.UseExpressionBody);
            AddEntry(nameof(Mapping), Value.Mapping);
            AddEntry(nameof(CustomModels), Value.CustomModels);
            AddEntry(nameof(ModelDir), Value.ModelDir);
            AddEntry(nameof(EmptyModelDir), Value.EmptyModelDir);
            AddEntry(nameof(SkipSyncMethods), Value.SkipSyncMethods);
            AddEntry(nameof(SkipAsyncMethods), Value.SkipAsyncMethods);
            AddEntry(nameof(MinNormVersion), Value.MinNormVersion);
            AddEntry(nameof(SourceHeader), Value.SourceHeader);
            AddEntry(nameof(Ident), Value.Ident);
            AddEntry(nameof(SingleLinqMethod), Value.SingleLinqMethod);

            sb.AppendLine();
            AddSectionComment(
                "Routines data-access extensions code-generation", 
                "https://github.com/vb-consulting/PgRoutiner/wiki/2.-WORKING-WITH-ROUTINES#routines-data-access-extensions-code-generation-settings",
                $"- Use \"{RoutinesArgs.Alias}\" or \"--{RoutinesArgs.Original.ToKebabCase()}\" switch to run routines data-access extensions code-generation from the command line.",
                $"- Use \"{OutputDirArgs.Alias}\" or \"--{OutputDirArgs.Original.ToKebabCase()}\" option to set the output dir for the generated code from the command line.",
                $"- Use \"{RoutinesOverwriteArgs.Alias}\" or \"--{RoutinesOverwriteArgs.Original.ToKebabCase()}\" switch to set the overwrite mode for the generated code from the command line.",
                $"- Use \"{ModelDirArgs.Alias}\" or \"--{ModelDirArgs.Original.ToKebabCase()}\" option to set the custom models output dir for the generated code from the command line.");
            AddEntry(nameof(Routines), Value.Routines);
            AddEntry(nameof(OutputDir), Value.OutputDir);
            AddEntry(nameof(RoutinesEmptyOutputDir), Value.RoutinesEmptyOutputDir);
            AddEntry(nameof(RoutinesOverwrite), Value.RoutinesOverwrite);
            AddEntry(nameof(RoutinesAskOverwrite), Value.RoutinesAskOverwrite);
            AddEntry(nameof(NotSimilarTo), Value.NotSimilarTo);
            AddEntry(nameof(SimilarTo), Value.SimilarTo);

            sb.AppendLine();
            AddSectionComment(
                "Unit tests code-generation settings", 
                "https://github.com/vb-consulting/PgRoutiner/wiki/3.-WORKING-WITH-UNIT-TESTS#unit-tests-code-generation-settings",
                $"- Use \"{UnitTestsArgs.Alias}\" or \"--{UnitTestsArgs.Original.ToKebabCase()}\" switch to run unit tests code-generation from the command line.",
                $"- Use \"{UnitTestsDirArgs.Alias}\" or \"--{UnitTestsDirArgs.Original.ToKebabCase()}\" option to set the output dir for the generated unit test project from the command line.");
            AddEntry(nameof(UnitTests), Value.UnitTests);
            AddEntry(nameof(UnitTestsDir), Value.UnitTestsDir);
            AddEntry(nameof(UnitTestsAskRecreate), Value.UnitTestsAskRecreate);
            AddEntry(nameof(UnitTestsSkipSyncMethods), Value.UnitTestsSkipSyncMethods);
            AddEntry(nameof(UnitTestsSkipAsyncMethods), Value.UnitTestsSkipAsyncMethods);

            sb.AppendLine();
            AddSectionComment(
                "Schema dump script settings", 
                "https://github.com/vb-consulting/PgRoutiner/wiki/4.-WORKING-WITH-SCHEMA-DUMP-SCRIPT#schema-dump-script-settings",
                $"- Use \"{SchemaDumpArgs.Alias}\" or \"--{SchemaDumpArgs.Original.ToKebabCase()}\" switch to run schema script dump from the command line.",
                $"- Use \"{SchemaDumpFileArgs.Alias}\" or \"--{SchemaDumpFileArgs.Original.ToKebabCase()}\" option to set generated schema file name from the command line.",
                $"- Use \"{SchemaDumpOverwriteArgs.Alias}\" or \"--{SchemaDumpOverwriteArgs.Original.ToKebabCase()}\" switch to set the overwrite mode for the generated schema file from the command line.",
                $"- Use \"--{nameof(SchemaDumpPrivileges).ToKebabCase()}\" switch to include object privileges in schema file from the command line.");
            AddEntry(nameof(SchemaDump), Value.SchemaDump);
            AddEntry(nameof(SchemaDumpFile), Value.SchemaDumpFile);
            AddEntry(nameof(SchemaDumpOverwrite), Value.SchemaDumpOverwrite);
            AddEntry(nameof(SchemaDumpAskOverwrite), Value.SchemaDumpAskOverwrite);
            AddEntry(nameof(SchemaDumpOwners), Value.SchemaDumpOwners);
            AddEntry(nameof(SchemaDumpPrivileges), Value.SchemaDumpPrivileges);
            AddEntry(nameof(SchemaDumpNoDropIfExists), Value.SchemaDumpNoDropIfExists);
            AddEntry(nameof(SchemaDumpOptions), Value.SchemaDumpOptions);
            AddEntry(nameof(SchemaDumpNoTransaction), Value.SchemaDumpNoTransaction);

            sb.AppendLine();
            AddSectionComment(
                "Data dump script settings", 
                "https://github.com/vb-consulting/PgRoutiner/wiki/5.-WORKING-WITH-DATA-DUMP-SCRIPT#data-dump-script-settings",
                $"- Use \"{DataDumpArgs.Alias}\" or \"--{DataDumpArgs.Original.ToKebabCase()}\" switch to run data script dump from the command line.",
                $"- Use \"{DataDumpFileArgs.Alias}\" or \"--{DataDumpFileArgs.Original.ToKebabCase()}\" option to set generated data script file name from the command line.",
                $"- Use \"{DataDumpOverwriteArgs.Alias}\" or \"--{DataDumpOverwriteArgs.Original.ToKebabCase()}\" switch to set the overwrite mode for the generated data script file from the command line.");
            AddEntry(nameof(DataDump), Value.DataDump);
            AddEntry(nameof(DataDumpFile), Value.DataDumpFile);
            AddEntry(nameof(DataDumpOverwrite), Value.DataDumpOverwrite);
            AddEntry(nameof(DataDumpAskOverwrite), Value.DataDumpAskOverwrite);
            AddEntry(nameof(DataDumpTables), Value.DataDumpTables);
            AddEntry(nameof(DataDumpOptions), Value.DataDumpOptions);
            AddEntry(nameof(DataDumpNoTransaction), Value.DataDumpNoTransaction);

            sb.AppendLine();
            AddSectionComment(
                "Object file tree settings",
                "https://github.com/vb-consulting/PgRoutiner/wiki/6.-WORKING-WITH-OBJECT-FILES-TREE#object-file-tree-settings",
                $"- Use \"{DbObjectsArgs.Alias}\" or \"--{DbObjectsArgs.Original.ToKebabCase()}\" switch to run object files tree dump from the command line.",
                $"- Use \"{DbObjectsDirArgs.Alias}\" or \"--{DbObjectsDirArgs.Original.ToKebabCase()}\" option to set the root output dir from the command line.",
                $"- Use \"{DbObjectsOverwriteArgs.Alias}\" or \"--{DbObjectsOverwriteArgs.Original.ToKebabCase()}\" switch to set the overwrite mode for the generated files from the command line.");
            AddEntry(nameof(DbObjects), Value.DbObjects);
            AddEntry(nameof(DbObjectsDir), Value.DbObjectsDir);
            AddEntry(nameof(DbObjectsOverwrite), Value.DbObjectsOverwrite);
            AddEntry(nameof(DbObjectsAskOverwrite), Value.DbObjectsAskOverwrite);
            AddEntry(nameof(DbObjectsDirNames), Value.DbObjectsDirNames);
            AddEntry(nameof(DbObjectsSkipDeleteDir), Value.DbObjectsSkipDeleteDir);
            AddEntry(nameof(DbObjectsOwners), Value.DbObjectsOwners);
            AddEntry(nameof(DbObjectsPrivileges), Value.DbObjectsPrivileges);
            AddEntry(nameof(DbObjectsDropIfExists), Value.DbObjectsDropIfExists);
            AddEntry(nameof(DbObjectsCreateOrReplace), Value.DbObjectsCreateOrReplace);
            AddEntry(nameof(DbObjectsRaw), Value.DbObjectsRaw);

            sb.AppendLine();
            AddSectionComment(
                "Markdown (MD) database dictionaries settings", 
                "https://github.com/vb-consulting/PgRoutiner/wiki/7.-WORKING-WITH-MARKDOWN-DATABASE-DICTIONARIES#markdown-md-database-dictionaries-settings",
                $"- Use \"{MarkdownArgs.Alias}\" or \"--{MarkdownArgs.Original.ToKebabCase()}\" switch to run markdown (MD) database dictionary file from the command line.",
                $"- Use \"{MdFileArgs.Alias}\" or \"--{MdFileArgs.Original.ToKebabCase()}\" option to set generated dictionary file name from the command line.",
                $"- Use \"{CommitMdArgs.Alias}\" or \"--{CommitMdArgs.Original.ToKebabCase()}\" switch to run commit changes in comments from the MD file back to the database from the command line.");
            AddEntry(nameof(Markdown), Value.Markdown);
            AddEntry(nameof(MdFile), Value.MdFile);
            AddEntry(nameof(MdOverwrite), Value.MdOverwrite);
            AddEntry(nameof(MdAskOverwrite), Value.MdAskOverwrite);
            AddEntry(nameof(MdSkipRoutines), Value.MdSkipRoutines);
            AddEntry(nameof(MdSkipViews), Value.MdSkipViews);
            AddEntry(nameof(MdNotSimilarTo), Value.MdNotSimilarTo);
            AddEntry(nameof(MdSimilarTo), Value.MdSimilarTo);
            AddEntry(nameof(CommitMd), Value.CommitMd);

            sb.AppendLine();
            AddSectionComment(
                "PSQL command-line utility settings", 
                "https://github.com/vb-consulting/PgRoutiner/wiki/8.-WORKING-WITH-PSQL#psql-command-line-utility-settings",
                $"- Use \"{PsqlArgs.Alias}\" or \"--{PsqlArgs.Original.ToKebabCase()}\" switch to open PSQL command-line utility from the command line.");
            AddEntry(nameof(Psql), Value.Psql);
            AddEntry(nameof(PsqlTerminal), Value.PsqlTerminal);
            AddEntry(nameof(PsqlCommand), Value.PsqlCommand);
            AddEntry(nameof(PsqlOptions), Value.PsqlOptions);

            sb.AppendLine();
            AddSectionComment(
                "Diff scripts settings", 
                "https://github.com/vb-consulting/PgRoutiner/wiki/9.-WORKING-WITH-DIFF-SCRIPTS#diff-scripts-settings",
                $"- Use \"{DiffArgs.Alias}\" or \"--{DiffArgs.Original.ToKebabCase()}\" switch to run diff script generation from the command line.",
                $"- Use \"{DiffTargetArgs.Alias}\" or \"--{DiffTargetArgs.Original.ToKebabCase()}\" option to set target connection for the diff script generator from the command line.");
            AddEntry(nameof(Diff), Value.Diff);
            AddEntry(nameof(DiffTarget), Value.DiffTarget);
            AddEntry(nameof(DiffFilePattern), Value.DiffFilePattern);
            AddEntry(nameof(DiffPgDump), Value.DiffPgDump);
            AddEntry(nameof(DiffPrivileges), Value.DiffPrivileges);

            sb.AppendLine();
            AddSectionComment(
                "CRUD scripts settings",
                "https://github.com/vb-consulting/PgRoutiner/wiki/10.-WORKING-WITH-CRUD#crud-settings",
                $"- Use \"{CrudArgs.Alias}\" or \"--{CrudArgs.Original.ToKebabCase()}\" switch to run CRUD extension methods generation from the command line.",
                $"- Use \"{CrudOutputDirArgs.Alias}\" or \"--{CrudOutputDirArgs.Original.ToKebabCase()}\" option to set the custom models output dir for the generated CRUD extension methods code from the command line.");
            AddEntry(nameof(Crud), Value.Crud);
            AddEntry(nameof(CrudOutputDir), Value.CrudOutputDir);
            AddEntry(nameof(CrudEmptyOutputDir), Value.CrudEmptyOutputDir);
            AddEntry(nameof(CrudOverwrite), Value.CrudOverwrite);
            AddEntry(nameof(CrudAskOverwrite), Value.CrudAskOverwrite);
            AddEntry(nameof(CrudNoPrepare), Value.CrudNoPrepare);
            AddEntry(nameof(CrudCreate), Value.CrudCreate);
            AddEntry(nameof(CrudCreateReturning), Value.CrudCreateReturning);
            AddEntry(nameof(CrudCreateOnConflictDoNothing), Value.CrudCreateOnConflictDoNothing);
            AddEntry(nameof(CrudCreateOnConflictDoNothingReturning), Value.CrudCreateOnConflictDoNothingReturning);
            AddEntry(nameof(CrudCreateOnConflictDoUpdate), Value.CrudCreateOnConflictDoUpdate);
            AddEntry(nameof(CrudCreateOnConflictDoUpdateReturning), Value.CrudCreateOnConflictDoUpdateReturning);
            AddEntry(nameof(CrudReadBy), Value.CrudReadBy);
            AddEntry(nameof(CrudReadAll), Value.CrudReadAll);
            AddEntry(nameof(CrudUpdate), Value.CrudUpdate);
            AddEntry(nameof(CrudUpdateReturning), Value.CrudUpdateReturning);
            AddEntry(nameof(CrudDelete), Value.CrudDelete);
            AddEntry(nameof(CrudDeleteReturning), Value.CrudDeleteReturning, "");

            if (wrap)
            {
                sb.AppendLine("  }");
                sb.AppendLine("}");
            }

            return sb.ToString();
        }
    }
}
