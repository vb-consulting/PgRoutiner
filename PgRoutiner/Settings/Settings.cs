using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PgRoutiner
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
        public static readonly Arg SchemaArgs = new("-sch", nameof(Schema));
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
#if DEBUG
        [JsonIgnore] public string Project { get; set; }
#endif
        [JsonIgnore] public bool Dump { get; set; } = false;
        [JsonIgnore] public string Execute { get; set; } = null;

        /*general*/
        public string Connection { get; set; } = null;
        public bool SkipConnectionPrompt { get; set; } = false;
        public string Schema { get; set; } = null;

        public IList<string> SkipIfExists { get; set; } = new List<string>();
        public bool SkipUpdateReferences { get; set; } = false;
        public int Ident { get; set; } = 4;
        public string PgDump { get; set; } = "pg_dump";
        public string PgDumpFallback { get; set; } = OperatingSystem.IsWindows() ? 
            "C:\\Program Files\\PostgreSQL\\{0}\\bin\\pg_dump.exe" :
            "/usr/lib/postgresql/{0}/bin/pg_dump";
        public string SourceHeader { get; set; } = "// <auto-generated />";

        /*routines data-access extensions*/
        public bool Routines { get; set; } = false;
        public string OutputDir { get; set; } = "./DataAccess";
        public bool RoutinesOverwrite { get; set; } = false;
        public bool RoutinesAskOverwrite { get; set; } = false;
        public string Namespace { get; set; } = null;
        public string NotSimilarTo { get; set; } = null;
        public string SimilarTo { get; set; } = null;
        public string MinNormVersion { get; set; } = "3.1.2";
        public bool SkipSyncMethods { get; set; } = false;
        public bool SkipAsyncMethods { get; set; } = false;
        public bool UseStatementBody { get; set; } = false;
        public string ModelDir { get; set; } = null;
        public IDictionary<string, string> Mapping { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> CustomModels { get; set; } = new Dictionary<string, string>();
        public bool UseRecords { get; set; } = false;

        /*unit tests*/
        public bool UnitTests { get; set; } = false;
        public string UnitTestsDir { get; set; } = "../{0}Tests";
        public bool UnitTestsAskRecreate { get; set; } = false;

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
            { "Tables", "Tables" }, 
            { "Views", "Views" }, 
            { "Functions", "Functions" }, 
            { "Procedures", "Procedures" }, 
            { "Domains", "Domains" },
            { "Types", "Types" },
            { "Schemas", "Schemas" },
            { "Sequences", "Sequences" }
        };
        public bool DbObjectsSkipDelete { get; set; } = false;
        public bool DbObjectsOverwrite { get; set; } = false;
        public bool DbObjectsAskOverwrite { get; set; } = false;
        public bool DbObjectsOwners { get; set; } = false;
        public bool DbObjectsPrivileges { get; set; } = false;
        public bool DbObjectsDropIfExists { get; set; } = false;
        public bool DbObjectsCreateOrReplace { get; set; } = false;
        public bool DbObjectsRaw { get; set; } = false;

        /*comments markdown file*/
        public bool Markdown { get; set; } = false;
        public string MdFile { get; set; } = "./Database/{0}/Dictionary.md";
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
        public string PsqlOptions { get; set; } = null;

        /*diff settings*/
        public bool Diff { get; set; } = false;
        public string DiffPgDump { get; set; } = "pg_dump";
        public string DiffTarget { get; set; } = null;
        public string DiffFilePattern { get; set; } = "./Database/{0}-{1}/{2}-diff-{3:yyyyMMdd}.sql";
        public bool DiffPrivileges { get; set; } = false;

        public static readonly Settings Value = new();
    }
}
