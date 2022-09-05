﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PgRoutiner.SettingsManagement
{
    public class Arg
    {
        public string Alias { get; }
        public string Name { get; }
        public string Original { get; }

        public Arg(string alias, string name)
        {
            Alias = alias;
            Name = $"--{name.ToLower()}";
            Original = name;
        }
    }

    public partial class Current
    {
        public static readonly Arg DirArgs = new("-dir", nameof(Dir)); // set current dir
        public static readonly Arg HelpArgs = new("-h", nameof(Help));
        public static readonly Arg VersionArgs = new("-v", nameof(Version));
        public static readonly Arg InfoArgs = new("-info", nameof(Info));
        public static readonly Arg SettingsArgs = new("-s", nameof(Settings));
        public static readonly Arg RoutinesArgs = new("-r", nameof(Routines));
        public static readonly Arg CommitMdArgs = new("-cc", nameof(CommitMd));
        public static readonly Arg ExecuteArgs = new("-x", nameof(Execute));
        //public static readonly Arg OptionsArgs = new("-opt", nameof(Options));
        public static readonly Arg ListArgs = new("-l", nameof(List));
        public static readonly Arg DefinitionArgs = new("-def", nameof(Definition));
        public static readonly Arg InsertsArgs = new("-i", nameof(Inserts));
        public static readonly Arg BackupArgs = new("-backup", nameof(Backup));
        public static readonly Arg RestoreArgs = new("-restore", nameof(Restore));

        public static readonly Arg DumpConsoleArgs = new("-d", nameof(DumpConsole));
        public static readonly Arg SilentArgs = new("-silent", nameof(Silent));
        public static readonly Arg DebugArgs = new("-dbg", nameof(Debug));
        public static readonly Arg ConnectionArgs = new("-c", nameof(Connection));
        public static readonly Arg SchemaArgs = new("-sch", nameof(SchemaSimilarTo));
        public static readonly Arg NotSchemaArgs = new("-nsch", nameof(SchemaNotSimilarTo));
        public static readonly Arg PgDumpArgs = new("-pgdump", nameof(PgDump));
        public static readonly Arg PgRestoreArgs = new("-pgrestore", nameof(PgRestore));
        public static readonly Arg OutputDirArgs = new("-o", nameof(OutputDir));
        public static readonly Arg RoutinesOverwriteArgs = new("-row", nameof(RoutinesOverwrite));
        public static readonly Arg RoutinesAskOverwriteArgs = new("-rask", nameof(RoutinesAskOverwrite));
        public static readonly Arg NotSimilarToArgs = new("-nst", nameof(RoutinesNotSimilarTo));
        public static readonly Arg SimilarToArgs = new("-st", nameof(RoutinesSimilarTo));
        public static readonly Arg SkipSyncMethodsArgs = new("-ss", nameof(SkipSyncMethods));
        public static readonly Arg SkipAsyncMethodsArgs = new("-sa", nameof(SkipAsyncMethods));
        public static readonly Arg ModelDirArgs = new("-modeldir", nameof(ModelDir));
        public static readonly Arg UnitTestsArgs = new("-ut", nameof(UnitTests));
        public static readonly Arg UnitTestsDirArgs = new("-utd", nameof(UnitTestsDir));
        public static readonly Arg SchemaDumpArgs = new("-sd", nameof(SchemaDump));
        public static readonly Arg SchemaDumpFileArgs = new("-sdf", nameof(SchemaDumpFile));
        public static readonly Arg SchemaDumpOverwriteArgs = new("-scow", nameof(SchemaDumpOverwrite));
        public static readonly Arg SchemaDumpAskOverwriteArgs = new("-scask", nameof(SchemaDumpAskOverwriteArgs));
        public static readonly Arg DataDumpArgs = new("-dd", nameof(DataDump));
        public static readonly Arg DataDumpFileArgs = new("-ddf", nameof(DataDumpFile));
        public static readonly Arg DataDumpListArgs = new("-ddl", nameof(DataDumpList));
        public static readonly Arg DataDumpOverwriteArgs = new("-ddow", nameof(DataDumpOverwrite));
        public static readonly Arg DataDumpAskOverwriteArgs = new("-ddask", nameof(DataDumpAskOverwrite));
        public static readonly Arg DbObjectsArgs = new("-db", nameof(DbObjects));
        public static readonly Arg DbObjectsDirArgs = new("-dbd", nameof(DbObjectsDir));
        public static readonly Arg DbObjectsOverwriteArgs = new("-dbow", nameof(DbObjectsOverwrite));
        public static readonly Arg DbObjectsAskOverwriteArgs = new("-dbask", nameof(DbObjectsAskOverwrite));
        public static readonly Arg MarkdownArgs = new("-md", nameof(Markdown));
        public static readonly Arg MdFileArgs = new("-mdf", nameof(MdFile));
        public static readonly Arg MdOverwriteArgs = new("-mdow", nameof(MdOverwrite));
        public static readonly Arg MdAskOverwriteArgs = new("-mdask", nameof(MdAskOverwrite));
        public static readonly Arg PsqlArgs = new("-psql", nameof(Psql));
        public static readonly Arg DiffArgs = new("-diff", nameof(Diff));
        public static readonly Arg DiffPgDumpArgs = new("-diff-pg-dump", nameof(DiffPgDump));
        public static readonly Arg DiffTargetArgs = new("-diff-target", nameof(DiffTarget));

        public static readonly Arg CrudArgs = new("-crud", nameof(Crud));
        public static readonly Arg CrudOutputDirArgs = new("-crud-o", nameof(CrudOutputDir));

#if DEBUG
        [JsonIgnore] public string Project { get; set; }
#endif
        [JsonIgnore] public bool Help { get; set; } = false;
        [JsonIgnore] public bool Version { get; set; } = false;
        [JsonIgnore] public bool Info { get; set; } = false;
        [JsonIgnore] public bool Dir { get; set; } = false;
        [JsonIgnore] public bool Settings { get; set; } = false;
        [JsonIgnore] public bool Debug { get; set; } = false;

        public bool DumpConsole { get; set; } = false;
        public bool Silent { get; set; } = false;
        public string Execute { get; set; } = null;
        //public string Options { get; set; } = null;
        public bool List { get; set; } = false;
        public string Definition { get; set; } = null;
        public string Inserts { get; set; } = null;
        public string Backup { get; set; } = null;
        public bool BackupOwner { get; set; } = false;
        public string Restore { get; set; } = null;
        public bool RestoreOwner { get; set; } = false;

        /*general*/
        public string Connection { get; set; } = null;
        public bool SkipConnectionPrompt { get; set; } = false;
        public bool DumpPgCommands { get; set; } = true;
        public string SchemaSimilarTo { get; set; } = null;
        public string SchemaNotSimilarTo { get; set; } = null;

        public IList<string> SkipIfExists { get; set; } = new List<string>();
        public bool SkipUpdateReferences { get; set; } = false;
        public string PgDump { get; set; } = "pg_dump";
        public string PgDumpFallback { get; set; } = null;

        public string PgRestore { get; set; } = "pg_restore";
        public string PgRestoreFallback { get; set; } = null;

        public string ConfigPath { get; set; } = null;

        /*code generation general options*/
        public string Namespace { get; set; } = null;
        public bool UseRecords { get; set; } = false;
        public bool UseExpressionBody { get; set; } = false;
        public bool UseFileScopedNamespaces { get; set; } = true;
        public bool UseNullableStrings { get; set; } = true;
        public IDictionary<string, string> Mapping { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> CustomModels { get; set; } = new Dictionary<string, string>();
        public string ModelDir { get; set; } = "./Models/{0}/";
        public string ModelCustomNamespace { get; set; } = null;
        public bool EmptyModelDir { get; set; } = false;
        public bool SkipSyncMethods { get; set; } = false;
        public bool SkipAsyncMethods { get; set; } = false;
        public string MinNormVersion { get; set; } = "5.2.3";
        public HashSet<string> SourceHeaderLines { get; set; } = new()
        {
            "#pragma warning disable CS8632",
            $"// pgroutiner auto-generated code",
        };
        public int Ident { get; set; } = 4;
        public string ReturnMethod { get; set; } = "SingleOrDefault";
        public IDictionary<string, string> MethodParameterNames { get; set; } = new Dictionary<string, string>()
        {
            { "string", "@string" },
            { "int", "@int" },
            { "bool", "@bool" },
            { "void", "@void" },
            { "public", "@public" },
            { "private", "@private" },
            { "protected", "@protected" },
            { "class", "@class" },
            { "record", "@record" },
            { "enum", "@enum" },
            { "namespace", "@namespace" },
            { "using", "@using" },
        };

        /*routines data-access extensions*/
        public bool Routines { get; set; } = false;
        public string RoutinesSchemaSimilarTo { get; set; } = null;
        public string RoutinesSchemaNotSimilarTo { get; set; } = null;
        public string OutputDir { get; set; } = "./Extensions/{0}/";
        public bool RoutinesEmptyOutputDir { get; set; } = false;
        public bool RoutinesOverwrite { get; set; } = false;
        public bool RoutinesAskOverwrite { get; set; } = false;
        public string RoutinesNotSimilarTo { get; set; } = null;
        public string RoutinesSimilarTo { get; set; } = null;
        public IDictionary<string, string> RoutinesReturnMethods { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> RoutinesModelPropertyTypes { get; set; } = new Dictionary<string, string>();
        public HashSet<string> RoutinesUnknownReturnTypes { get; set; } = new()
        {
            "json", "jsonb", "text", "varchar"
        };
        public bool RoutinesCallerInfo { get; set; } = false;
        public HashSet<string> RoutinesLanguages { get; set; } = new()
        {
            "sql", "plpgsql"
        };

        /*unit tests*/
        public bool UnitTests { get; set; } = false;
        public string UnitTestProjectTargetFramework { get; set; } = "net6.0";
        public string UnitTestProjectLangVersion { get; set; } = null;
        public string UnitTestsDir { get; set; } = "../{0}Tests";
        public bool UnitTestsAskRecreate { get; set; } = false;
        public bool UnitTestsSkipSyncMethods { get; set; } = false;
        public bool UnitTestsSkipAsyncMethods { get; set; } = false;

        /*schema dump*/
        public bool SchemaDump { get; set; } = false;
        public string SchemaDumpFile { get; set; } = null;//"./Database/{0}/Schema.sql";
        public bool SchemaDumpOverwrite { get; set; } = false;
        public bool SchemaDumpAskOverwrite { get; set; } = false;
        public bool SchemaDumpOwners { get; set; } = false;
        public bool SchemaDumpPrivileges { get; set; } = false;
        public bool SchemaDumpNoDropIfExists { get; set; } = false;
        public string SchemaDumpOptions { get; set; } = null;
        public bool SchemaDumpNoTransaction { get; set; } = false;

        /*data dump*/
        public bool DataDump { get; set; } = false;
        public string DataDumpFile { get; set; } = null;//"./Database/{0}/Data.sql";
        public bool DataDumpOverwrite { get; set; } = false;
        public bool DataDumpAskOverwrite { get; set; } = false;
        public string DataDumpList { get; set; } = null;
        public IList<string> DataDumpTables { get; set; } = new List<string>();
        public string DataDumpOptions { get; set; } = null;
        public bool DataDumpNoTransaction { get; set; } = false;
        public bool DataDumpRaw { get; set; } = false;

        /*object tree*/
        public bool DbObjects { get; set; } = false;
        public string DbObjectsDir { get; set; } = "./Database/{0}/";
        public IDictionary<string, string> DbObjectsDirNames { get; set; } = new Dictionary<string, string>()
        {
            { "Tables", "Tables/{0}" },
            { "Views", "Views/{0}" },
            { "Functions", "Functions/{0}" },
            { "Procedures", "Procedures/{0}" },
            { "Domains", "Domains/{0}" },
            { "Types", "Types/{0}" },
            { "Schemas", "Schemas/{0}" },
            { "Sequences", "Sequences/{0}" },
            { "Extensions", "Extensions/{0}" },
        };
        public bool DbObjectsSkipDeleteDir { get; set; } = false;
        //public bool DbObjectsRemoveExistingDirs { get; set; } = true;
        public bool DbObjectsOverwrite { get; set; } = false;
        public bool DbObjectsAskOverwrite { get; set; } = false;
        public bool DbObjectsOwners { get; set; } = false;
        public bool DbObjectsPrivileges { get; set; } = false;
        public bool DbObjectsCreateOrReplace { get; set; } = false;
        public bool DbObjectsRaw { get; set; } = false;

        /*comments markdown file*/
        public bool Markdown { get; set; } = false;
        public string MdSchemaSimilarTo { get; set; } = null;
        public string MdSchemaNotSimilarTo { get; set; } = null;
        public string MdFile { get; set; } = "./Database/{0}/README.md";
        public bool MdOverwrite { get; set; } = false;
        public bool MdAskOverwrite { get; set; } = false;
        public bool MdSkipRoutines { get; set; } = false;
        public bool MdSkipViews { get; set; } = false;
        public bool MdSkipEnums { get; set; } = false;
        public string MdNotSimilarTo { get; set; } = null;
        public string MdSimilarTo { get; set; } = null;
        public bool MdIncludeSourceLinks { get; set; } = false;
        public bool MdIncludeExtensionLinks { get; set; } = false;
        public bool MdIncludeUnitTestsLinks { get; set; } = false;
        public string MdSourceLinkRoot { get; set; } = null;
        public bool MdIncludeTableCountEstimates { get; set; } = false;
        public bool MdIncludeTableStats { get; set; } = false;
        public bool MdRoutinesFirst { get; set; } = false;
        public bool MdExportToHtml { get; set; } = false;
        public bool CommitMd { get; set; } = false;

        /*psql interactive terminal*/
        public bool Psql { get; set; } = false;
        public string PsqlTerminal { get; set; } = "wt";
        public string PsqlCommand { get; set; } = "psql";

        public string PsqlFallback { get; set; } = null;
        public string PsqlOptions { get; set; } = null;

        /*diff settings*/
        public bool Diff { get; set; } = false;
        public string DiffPgDump { get; set; } = "pg_dump";
        public string DiffTarget { get; set; } = null;
        public string DiffFilePattern { get; set; } = "./Database/{0}-{1}/{2}-diff-{3:yyyyMMdd}.sql";
        public bool DiffPrivileges { get; set; } = false;
        public string DiffSkipSimilarTo { get; set; } = "pg_%";

        /*crud settings*/
        public bool Crud { get; set; } = false;
        public string CrudOutputDir { get; set; } = "./Extensions/{0}/";
        public bool CrudEmptyOutputDir { get; set; } = false;
        public bool CrudOverwrite { get; set; } = false;
        public bool CrudAskOverwrite { get; set; } = false;
        public bool CrudNoPrepare { get; set; } = false;
        public IDictionary<string, string> CrudReturnMethods { get; set; } = new Dictionary<string, string>();
        public HashSet<string> CrudCreate { get; set; } = new HashSet<string>();
        public HashSet<string> CrudCreateReturning { get; set; } = new HashSet<string>();
        public HashSet<string> CrudCreateOnConflictDoNothing { get; set; } = new HashSet<string>();
        public HashSet<string> CrudCreateOnConflictDoNothingReturning { get; set; } = new HashSet<string>();
        public HashSet<string> CrudCreateOnConflictDoUpdate { get; set; } = new HashSet<string>();
        public HashSet<string> CrudCreateOnConflictDoUpdateReturning { get; set; } = new HashSet<string>();
        public HashSet<string> CrudReadBy { get; set; } = new HashSet<string>();
        public HashSet<string> CrudReadAll { get; set; } = new HashSet<string>();
        public HashSet<string> CrudUpdate { get; set; } = new HashSet<string>();
        public HashSet<string> CrudUpdateReturning { get; set; } = new HashSet<string>();
        public HashSet<string> CrudDeleteBy { get; set; } = new HashSet<string>();
        public HashSet<string> CrudDeleteByReturning { get; set; } = new HashSet<string>();

        public static readonly Current Value = new();
    }
}
