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

            void AddComment(string comment)
            {
                sb.AppendLine($"    /* {comment} */");
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
                    sb.AppendLine($"    \"{Value.Connection}\": \"{connection.ConnectionString}\"");
                }
                else
                {
                    sb.AppendLine("    //\"Connection1\": \"Server={server};Db={database};Port={port};User Id={user};Password={password};\"");
                    sb.AppendLine("    //\"Connection2\": \"postgresql://{user}:{password}@{server}:{port}/{database}\" ");

                }
                sb.AppendLine("  },");
                sb.AppendLine("  /* see https://github.com/vb-consulting/PgRoutiner/wiki/1.-WORKING-WITH-SETTINGS for more info */");
                sb.AppendLine("  \"PgRoutiner\": {");
                sb.AppendLine();
            }

            AddComment("General settings, see https://github.com/vb-consulting/PgRoutiner/wiki/1.-WORKING-WITH-SETTINGS#general-settings for more info");
            AddEntry(nameof(Connection), Value.Connection);
            AddEntry(nameof(Schema), Value.Schema);
            AddEntry(nameof(Execute), Value.Execute);
            AddEntry(nameof(Dump), Value.Dump);
            AddEntry(nameof(SkipIfExists), Value.SkipIfExists);
            AddEntry(nameof(SkipUpdateReferences), Value.SkipUpdateReferences);
            AddEntry(nameof(Ident), Value.Ident);
            AddEntry(nameof(PgDump), Value.PgDump);
            AddEntry(nameof(PgDumpFallback), Value.PgDumpFallback);
            AddEntry(nameof(SourceHeader), Value.SourceHeader);

            sb.AppendLine();
            AddComment("Routines data-access extensions code-generation settings, https://github.com/vb-consulting/PgRoutiner/wiki/2.-WORKING-WITH-ROUTINES#routines-data-access-extensions-code-generation-settings see for more info");
            AddEntry(nameof(Routines), Value.Routines);
            AddEntry(nameof(OutputDir), Value.OutputDir);
            AddEntry(nameof(RoutinesOverwrite), Value.RoutinesOverwrite);
            AddEntry(nameof(RoutinesAskOverwrite), Value.RoutinesAskOverwrite);
            AddEntry(nameof(Namespace), Value.Namespace);
            AddEntry(nameof(NotSimilarTo), Value.NotSimilarTo);
            AddEntry(nameof(SimilarTo), Value.SimilarTo);
            AddEntry(nameof(MinNormVersion), Value.MinNormVersion);
            AddEntry(nameof(SkipSyncMethods), Value.SkipSyncMethods);
            AddEntry(nameof(SkipAsyncMethods), Value.SkipAsyncMethods);
            AddEntry(nameof(UseStatementBody), Value.UseStatementBody);
            AddEntry(nameof(ModelDir), Value.ModelDir);
            AddEntry(nameof(Mapping), Value.Mapping);
            AddEntry(nameof(CustomModels), Value.CustomModels);
            AddEntry(nameof(UseRecords), Value.UseRecords);

            sb.AppendLine();
            AddComment("Unit tests code-generation settings, see https://github.com/vb-consulting/PgRoutiner/wiki/3.-WORKING-WITH-UNIT-TESTS#unit-tests-code-generation-settings for more info");
            AddEntry(nameof(UnitTests), Value.UnitTests);
            AddEntry(nameof(UnitTestsDir), Value.UnitTestsDir);
            AddEntry(nameof(UnitTestsAskRecreate), Value.UnitTestsAskRecreate);

            sb.AppendLine();
            AddComment("Schema dump script settings, see https://github.com/vb-consulting/PgRoutiner/wiki/4.-WORKING-WITH-SCHEMA-DUMP-SCRIPT#schema-dump-script-settings for more info");
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
            AddComment("Data dump script settings, see https://github.com/vb-consulting/PgRoutiner/wiki/5.-WORKING-WITH-DATA-DUMP-SCRIPT#data-dump-script-settings for more info");
            AddEntry(nameof(DataDump), Value.DataDump);
            AddEntry(nameof(DataDumpFile), Value.DataDumpFile);
            AddEntry(nameof(DataDumpOverwrite), Value.DataDumpOverwrite);
            AddEntry(nameof(DataDumpAskOverwrite), Value.DataDumpAskOverwrite);
            AddEntry(nameof(DataDumpTables), Value.DataDumpTables);
            AddEntry(nameof(DataDumpOptions), Value.DataDumpOptions);
            AddEntry(nameof(DataDumpNoTransaction), Value.DataDumpNoTransaction);

            sb.AppendLine();
            AddComment("Object file tree settings, see https://github.com/vb-consulting/PgRoutiner/wiki/6.-WORKING-WITH-OBJECT-FILES-TREE#object-file-tree-settings for more info");
            AddEntry(nameof(DbObjects), Value.DbObjects);
            AddEntry(nameof(DbObjectsDir), Value.DbObjectsDir);
            AddEntry(nameof(DbObjectsOverwrite), Value.DbObjectsOverwrite);
            AddEntry(nameof(DbObjectsAskOverwrite), Value.DbObjectsAskOverwrite);
            AddEntry(nameof(DbObjectsDirNames), Value.DbObjectsDirNames);
            AddEntry(nameof(DbObjectsSkipDelete), Value.DbObjectsSkipDelete);
            AddEntry(nameof(DbObjectsOwners), Value.DbObjectsOwners);
            AddEntry(nameof(DbObjectsPrivileges), Value.DbObjectsPrivileges);
            AddEntry(nameof(DbObjectsDropIfExists), Value.DbObjectsDropIfExists);
            AddEntry(nameof(DbObjectsCreateOrReplace), Value.DbObjectsCreateOrReplace);
            AddEntry(nameof(DbObjectsRaw), Value.DbObjectsRaw);

            sb.AppendLine();
            AddComment("Markdown (MD) database dictionaries settings, see https://github.com/vb-consulting/PgRoutiner/wiki/7.-WORKING-WITH-MARKDOWN-DATABASE-DICTIONARIES#markdown-md-database-dictionaries-settings for more info");
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
            AddComment("PSQL command-line utility settings, see https://github.com/vb-consulting/PgRoutiner/wiki/8.-WORKING-WITH-PSQL#psql-command-line-utility-settings for more info");
            AddEntry(nameof(Psql), Value.Psql);
            AddEntry(nameof(PsqlTerminal), Value.PsqlTerminal);
            AddEntry(nameof(PsqlCommand), Value.PsqlCommand);
            AddEntry(nameof(PsqlOptions), Value.PsqlOptions);

            sb.AppendLine();
            AddComment("Diff scripts settings, see https://github.com/vb-consulting/PgRoutiner/wiki/9.-WORKING-WITH-DIFF-SCRIPTS#diff-scripts-settings for more info");
            AddEntry(nameof(Diff), Value.Diff);
            AddEntry(nameof(DiffTarget), Value.DiffTarget);
            AddEntry(nameof(DiffFilePattern), Value.DiffFilePattern);
            AddEntry(nameof(DiffPgDump), Value.DiffPgDump);
            AddEntry(nameof(DiffPrivileges), Value.DiffPrivileges, "");

            if (wrap)
            {
                sb.AppendLine("  }");
                sb.AppendLine("}");
            }

            return sb.ToString();
        }
    }
}
