using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Npgsql;
using System.Xml;

namespace PgRoutiner
{
    public partial class Settings
    {
        public static string BuildFormatedSettings(bool wrap = true)
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
                sb.AppendLine($"  /* see https://github.com/vb-consulting/PgRoutiner/CONNECTIONS.MD for more info */");
                sb.AppendLine("  \"ConnectionStrings\": {");
                sb.AppendLine("    //\"Connection1\": \"Server={server};Db={database};Port={port};User Id={user};Password={password};\"");
                sb.AppendLine("    //\"Connection2\": \"postgresql://{user}:{password}@{server}:{port}/{database}\" ");
                sb.AppendLine("  },");
                sb.AppendLine("  /* see https://github.com/vb-consulting/PgRoutiner/SETTINGS.MD for more info */");
                sb.AppendLine("  \"PgRoutiner\": {");
                sb.AppendLine();
            }

            AddComment("general settings");
            AddEntry(nameof(Connection), Value.Connection);
            AddEntry(nameof(Schema), Value.Schema);
            AddEntry(nameof(Overwrite), Value.Overwrite);
            AddEntry(nameof(AskOverwrite), Value.AskOverwrite);
            AddEntry(nameof(SkipIfExists), Value.SkipIfExists);
            AddEntry(nameof(MinNormVersion), Value.MinNormVersion);
            AddEntry(nameof(SkipUpdateReferences), Value.SkipUpdateReferences);
            AddEntry(nameof(Ident), Value.Ident);
            AddEntry(nameof(PgDump), Value.PgDump);

            sb.AppendLine();
            AddComment("routines data-access extensions settings");
            AddEntry(nameof(OutputDir), Value.OutputDir);
            AddEntry(nameof(Namespace), Value.Namespace);
            AddEntry(nameof(NotSimilarTo), Value.NotSimilarTo);
            AddEntry(nameof(SimilarTo), Value.SimilarTo);
            AddEntry(nameof(SourceHeader), Value.SourceHeader);
            AddEntry(nameof(SkipSyncMethods), Value.SkipSyncMethods);
            AddEntry(nameof(SkipAsyncMethods), Value.SkipAsyncMethods);
            AddEntry(nameof(ModelDir), Value.ModelDir);
            AddEntry(nameof(Mapping), Value.Mapping);
            AddEntry(nameof(CustomModels), Value.CustomModels);
            AddEntry(nameof(UseRecords), Value.UseRecords);

            sb.AppendLine();
            AddComment("schema dump settings");
            AddEntry(nameof(SchemaDumpFile), Value.SchemaDumpFile);
            AddEntry(nameof(SchemaDumpOwners), Value.SchemaDumpOwners);
            AddEntry(nameof(SchemaDumpPrivileges), Value.SchemaDumpPrivileges);
            AddEntry(nameof(SchemaDumpNoDropIfExists), Value.SchemaDumpNoDropIfExists);
            AddEntry(nameof(SchemaDumpOptions), Value.SchemaDumpOptions);
            AddEntry(nameof(SchemaDumpNoTransaction), Value.SchemaDumpNoTransaction);

            sb.AppendLine();
            AddComment("data dump settings");
            AddEntry(nameof(DataDumpFile), Value.DataDumpFile);
            AddEntry(nameof(DataDumpTables), Value.DataDumpTables);
            AddEntry(nameof(DataDumpOptions), Value.DataDumpOptions);
            AddEntry(nameof(DataDumpNoTransaction), Value.DataDumpNoTransaction);

            sb.AppendLine();
            AddComment("database object tree settings");
            AddEntry(nameof(DbObjectsDir), Value.DbObjectsDir);
            AddEntry(nameof(DbObjectsDirNames), Value.DbObjectsDirNames);
            AddEntry(nameof(DbObjectsSkipDelete), Value.DbObjectsSkipDelete);
            AddEntry(nameof(DbObjectsOwners), Value.DbObjectsOwners);
            AddEntry(nameof(DbObjectsPrivileges), Value.DbObjectsPrivileges);
            AddEntry(nameof(DbObjectsDropIfExists), Value.DbObjectsDropIfExists);
            AddEntry(nameof(DbObjectsNoCreateOrReplace), Value.DbObjectsNoCreateOrReplace);
            AddEntry(nameof(DbObjectsRaw), Value.DbObjectsRaw);

            sb.AppendLine();
            AddComment("comments markdown documentation file settings");
            AddEntry(nameof(CommentsMdFile), Value.CommentsMdFile);
            AddEntry(nameof(CommentsMdSkipRoutines), Value.CommentsMdSkipRoutines);
            AddEntry(nameof(CommentsMdSkipViews), Value.CommentsMdSkipViews);
            AddEntry(nameof(CommentsMdNotSimilarTo), Value.CommentsMdNotSimilarTo);
            AddEntry(nameof(CommentsMdSimilarTo), Value.CommentsMdSimilarTo, "");

            if (wrap)
            {
                sb.AppendLine("  }");
                sb.AppendLine("}");
            }

            return sb.ToString();
        }
    }
}
