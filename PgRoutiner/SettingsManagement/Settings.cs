using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PgRoutiner.SettingsManagement
{
    public class Switches
    {
        public bool Help { get; set; } = false;
        public bool Settings { get; set; } = false;
        public bool Debug { get; set; } = false;
        public bool Info { get; set; } = false;

        public static readonly Switches Value = new();
    }

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

    public partial class Settings
    {
        public static readonly Arg DirArgs = new("-dir", "dir");
        public static readonly Arg HelpArgs = new("-h", "help");
        public static readonly Arg VersionArgs = new("-v", "version");
        public static readonly Arg InfoArgs = new("-i", "info");
        public static readonly Arg SettingsArgs = new("-s", "settings");
        public static readonly Arg RoutinesArgs = new("-r", nameof(Routines));
        public static readonly Arg CommitMdArgs = new("-cc", nameof(CommitMd));
        public static readonly Arg ExecuteArgs = new("-x", nameof(Execute));
        public static readonly Arg DumpArgs = new("-d", nameof(Dump));
        public static readonly Arg DebugArgs = new("-dbg", "debug");
        public static readonly Arg ConnectionArgs = new("-c", nameof(Connection));
        public static readonly Arg SchemaArgs = new("-sch", nameof(SchemaSimilarTo));
        public static readonly Arg NotSchemaArgs = new("-nsch", nameof(SchemaNotSimilarTo));
        public static readonly Arg PgDumpArgs = new("-pgdump", nameof(PgDump));
        public static readonly Arg OutputDirArgs = new("-o", nameof(OutputDir));
        public static readonly Arg RoutinesOverwriteArgs = new("-row", nameof(RoutinesOverwrite));
        public static readonly Arg RoutinesAskOverwriteArgs = new("-rask", nameof(RoutinesAskOverwrite));
        public static readonly Arg NotSimilarToArgs = new("-nst", nameof(NotSimilarTo));
        public static readonly Arg SimilarToArgs = new("-st", nameof(SimilarTo));
        public static readonly Arg SkipSyncMethodsArgs = new("-ss", nameof(SkipSyncMethods));
        public static readonly Arg SkipAsyncMethodsArgs = new("-sa", nameof(SkipAsyncMethods));
        public static readonly Arg ModelDirArgs = new("-md", nameof(ModelDir));
        public static readonly Arg UnitTestsArgs = new("-ut", nameof(UnitTests));
        public static readonly Arg UnitTestsDirArgs = new("-utd", nameof(UnitTestsDir));
        public static readonly Arg SchemaDumpArgs = new("-sd", nameof(SchemaDump));
        public static readonly Arg SchemaDumpFileArgs = new("-sdf", nameof(SchemaDumpFile));
        public static readonly Arg SchemaDumpOverwriteArgs = new("-scow", nameof(SchemaDumpOverwrite));
        public static readonly Arg SchemaDumpAskOverwriteArgs = new("-scask", nameof(SchemaDumpAskOverwriteArgs));
        public static readonly Arg DataDumpArgs = new("-dd", nameof(DataDump));
        public static readonly Arg DataDumpFileArgs = new("-ddf", nameof(DataDumpFile));
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
        [JsonIgnore] public bool Dump { get; set; } = false;
        [JsonIgnore] public string Execute { get; set; } = null;
        /*general*/
        public string Connection { get; set; } = null;
        public bool SkipConnectionPrompt { get; set; } = false;
        public bool DumpPgCommands { get; set; } = true;
        public string SchemaSimilarTo { get; set; } = null;
        public string SchemaNotSimilarTo { get; set; } = null;

        public IList<string> SkipIfExists { get; set; } = new List<string>();
        public bool SkipUpdateReferences { get; set; } = false;
        public string PgDump { get; set; } = "pg_dump";
        public string PgDumpFallback { get; set; } = OperatingSystem.IsWindows() ?
            "C:\\Program Files\\PostgreSQL\\{0}\\bin\\pg_dump.exe" :
            "/usr/lib/postgresql/{0}/bin/pg_dump";
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
        public string MinNormVersion { get; set; } = "3.2.0";
        public string SourceHeader { get; set; } = "// pgroutiner auto-generated code";
        public int Ident { get; set; } = 4;
        public string ReturnMethod { get; set; } = "SingleOrDefault";
        public IDictionary<string, string> MethodParameterNames { get; set; } = new Dictionary<string, string>();

        /*routines data-access extensions*/
        public bool Routines { get; set; } = false;
        public string RoutinesSchemaSimilarTo { get; set; } = null;
        public string RoutinesSchemaNotSimilarTo { get; set; } = null;
        public string OutputDir { get; set; } = "./Extensions/{0}/";
        public bool RoutinesEmptyOutputDir { get; set; } = false;
        public bool RoutinesOverwrite { get; set; } = false;
        public bool RoutinesAskOverwrite { get; set; } = false;
        public string NotSimilarTo { get; set; } = null;
        public string SimilarTo { get; set; } = null;
        public IDictionary<string, string> RoutinesReturnMethods { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> RoutinesModelPropertyTypes { get; set; } = new Dictionary<string, string>();

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
        public string SchemaDumpFile { get; set; } = "./Database/{0}/Schema.sql";
        public bool SchemaDumpOverwrite { get; set; } = false;
        public bool SchemaDumpAskOverwrite { get; set; } = false;
        public bool SchemaDumpOwners { get; set; } = false;
        public bool SchemaDumpPrivileges { get; set; } = false;
        public bool SchemaDumpNoDropIfExists { get; set; } = false;
        public string SchemaDumpOptions { get; set; } = null;
        public bool SchemaDumpNoTransaction { get; set; } = false;

        /*data dump*/
        public bool DataDump { get; set; } = false;
        public string DataDumpFile { get; set; } = "./Database/{0}/Data.sql";
        public bool DataDumpOverwrite { get; set; } = false;
        public bool DataDumpAskOverwrite { get; set; } = false;
        public IList<string> DataDumpTables { get; set; } = new List<string>();
        public string DataDumpOptions { get; set; } = null;
        public bool DataDumpNoTransaction { get; set; } = false;

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
            { "Schemas", "Schemas" },
            { "Sequences", "Sequences/{0}" }
        };
        public bool DbObjectsSkipDeleteDir { get; set; } = false;
        public bool DbObjectsOverwrite { get; set; } = false;
        public bool DbObjectsAskOverwrite { get; set; } = false;
        public bool DbObjectsOwners { get; set; } = false;
        public bool DbObjectsPrivileges { get; set; } = false;
        public bool DbObjectsDropIfExists { get; set; } = false;
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
        public string MdNotSimilarTo { get; set; } = null;
        public string MdSimilarTo { get; set; } = null;
        public bool CommitMd { get; set; } = false;

        /*psql interactive terminal*/
        public bool Psql { get; set; } = false;
        public string PsqlTerminal { get; set; } = "wt";
        public string PsqlCommand { get; set; } = "psql";
        public string PsqlFallback { get; set; } = OperatingSystem.IsWindows() ?
            "C:\\Program Files\\PostgreSQL\\{0}\\bin\\psql.exe" :
            "/usr/lib/postgresql/{0}/bin/psql";
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

        public static readonly Settings Value = new();
    }
}
