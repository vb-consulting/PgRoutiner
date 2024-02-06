```jsonc
/* PgRoutiner (5.4.0.0) settings */
{
  "ConnectionStrings": {
    "PostgresConnection": "postgresql://postgres:postgres@127.0.0.1:5432/postgres"
    //"Connection1": "Server={server};Db={database};Port={port};User Id={user};Password={password};"
    //"Connection2": "postgresql://{user}:{password}@{server}:{port}/{database}"
  },
  /*
    "PgRoutiner" configuration section is used to configure PgRoutiner. Use command line to override these values:
    -v --version (switch) to check out current version.
    -info --info (switch) show environment info.
    -settings --settings (switch) to see all currently active settings, including command lines overrides and all configuration files.
    -d --d -dump --dump -console --console -dump-console --dump-console (switch) to redirect all results into console output instead of files or database.
    -wcf --write-config-file to create new configuration file based on current settings.
    -cf --cf -config --config -config-file --config-file to load additional custom configuration file.
    -ow --ow --overwrite (switch) to force overwrite existing files for each generated files.
    -ask --ask --ask-overwrite (switch) to prompt overwrite question of existing files for each generated files.
  */
  "PgRoutiner": {

    /*
      General settings:
      -c -conn --conn -connection --connection to set working connection from the command line.
      -sch --schema-similar-to to set schema similar to expression from the command line.
      -x --x -exec --exec -execute --execute --execute option to execute SQL file or PSQL command on your current connection from the command line.
      -silent -silent --silent to silent not required console texts.
      -l -ls --ls --list to dump or search object list to console. Use switch to dump all objects or parameter value to search.
      -def -ddl --ddl -definition --definition to dump object schema definition in console supplied as value parameter.
      -s --s --search to search object schema definitions and dump highlighted results to console.
      -i -ins -inserts --inserts to dump objects or queries (semicolon separated) insert definitions.
    */
    "Connection": "PostgresConnection",
    "SkipConnectionPrompt": false,
    "DumpPgCommands": true,
    "SchemaSimilarTo": null,
    "SchemaNotSimilarTo": null,
    "Execute": null,
    "List": null,
    "Definition": null,
    "Search": null,
    "Inserts": null,
    "Backup": null,
    "BackupOwner": false,
    "Restore": null,
    "RestoreOwner": false,
    "DumpConsole": false,
    "Silent": false,
    "Verbose": false,
    "SkipIfExists": [ ],
    "SkipUpdateReferences": false,
    "PgDump": "pg_dump",
    "PgDumpFallback": null,
    "PgRestore": "pg_restore",
    "PgRestoreFallback": null,
    "ConfigPath": null,
    "Overwrite": false,
    "AskOverwrite": false,

    /*
      Code generation general settings:
      -modeldir --modeldir --model-dir to set model output directory.
      -ss --skip-sync-methods to skip sync methods generation.
      -sa --skip-async-methods to skip async methods generation.
    */
    "Namespace": null,
    "UseRecords": false,
    "UseRecordsForModels": [ ],
    "UseFileScopedNamespaces": true,
    "UseNullableTypes": true,
    "Mapping": {
      "text": "string",
      "character": "string",
      "xml": "string",
      "inet": "string",
      "daterange": "TimeSpan",
      "double precision": "double",
      "boolean": "bool",
      "smallint": "short",
      "timestamp with time zone": "DateTime",
      "timestamp without time zone": "DateTime",
      "bigint": "long",
      "time with time zone": "DateTime",
      "time without time zone": "DateTime",
      "char": "string",
      "date": "DateTime",
      "numeric": "decimal",
      "character varying": "string",
      "jsonb": "string",
      "real": "float",
      "json": "string",
      "integer": "int",
      "bpchar": "string",
      "float8": "double",
      "bool": "bool",
      "int2": "short",
      "timestamptz": "DateTime",
      "int8": "long",
      "timetz": "DateTime",
      "time": "DateTime",
      "varchar": "string",
      "float4": "float",
      "int4": "int",
      "uuid": "Guid"
    },
    "CustomModels": { },
    "ModelDir": "./Models/{0}/",
    "ModelCustomNamespace": null,
    "EmptyModelDir": false,
    "SkipSyncMethods": false,
    "SkipAsyncMethods": false,
    "SourceHeaderLines": [
      "// pgroutiner auto-generated code",
      "#pragma warning disable CS8632",
      "#pragma warning disable CS8618"
    ],
    "Ident": 4,
    "MethodParameterNames": {
      "string": "@string",
      "int": "@int",
      "bool": "@bool",
      "void": "@void",
      "public": "@public",
      "private": "@private",
      "protected": "@protected",
      "class": "@class",
      "record": "@record",
      "enum": "@enum",
      "namespace": "@namespace",
      "using": "@using"
    },

    /*
      Routines data-access extensions code-generation:
      -r --r -rout --rout -routines --routines (switch) to run routines data-access extensions code-generation from the command line.
      -o --output-dir to set the output dir for the generated code from the command line.
      -rs --rs -r-similar --r-similar -routines-similar --routines-similar --routines-similar-to to set "similar to" expression that matches routines names to be included.
      -rns --rns -r-not-similar --r-not-similar -routines-not-similar --routines-not-similar --routines-not-similar-to to set "not similar to" expression that matches routines names not to be included.
    */
    "Routines": false,
    "RoutinesSimilarTo": null,
    "RoutinesNotSimilarTo": null,
    "RoutinesSchemaSimilarTo": null,
    "RoutinesSchemaNotSimilarTo": null,
    "OutputDir": "./Extensions/{0}/",
    "RoutinesEmptyOutputDir": false,
    "RoutinesReturnMethods": { },
    "RoutinesModelPropertyTypes": { },
    "RoutinesUnknownReturnTypes": [
      "json",
      "jsonb",
      "text",
      "varchar"
    ],
    "RoutinesCallerInfo": false,
    "RoutinesLanguages": [
      "sql",
      "plpgsql"
    ],
    "RoutinesCustomCodeLines": [ ],
    "RoutinesCancellationToken": false,
    "CustomDirs": { },
    "RoutinesIncludeDefintionInComment": false,
    "RoutinesOpenConnectionIfClosed": true,

    /*
      Unit tests code-generation settings:
      -ut --unit-tests to run unit tests templates code-generation from the command line.
      -utd --unit-tests-dir to set the output dir for the generated unit test project from the command line.
    */
    "UnitTests": false,
    "UnitTestProjectTargetFramework": "net7.0",
    "UnitTestProjectLangVersion": null,
    "UnitTestsDir": "../{0}Tests",
    "UnitTestsAskRecreate": false,
    "UnitTestsSkipSyncMethods": false,
    "UnitTestsSkipAsyncMethods": false,

    /*
      Schema dump script settings:
      -sd --schema-dump to run schema script dump from the command line.
      -sdf --schema-dump-file to set generated schema file name from the command line.
    */
    "SchemaDump": false,
    "SchemaDumpFile": null,
    "SchemaDumpOwners": false,
    "SchemaDumpPrivileges": false,
    "SchemaDumpNoDropIfExists": false,
    "SchemaDumpOptions": null,
    "SchemaDumpNoTransaction": true,

    /*
      Schema dump script settings:
      -dd --data-dump to run data script dump from the command line.
      -ddf --data-dump-file to set generated data script file name from the command line.
      -dumplist --data-dump-list set semicolon separated list of tables or queries merged with "DataDumpTables" option and to be dumped.
    */
    "DataDump": false,
    "DataDumpFile": null,
    "DataDumpList": null,
    "DataDumpTables": [ ],
    "DataDumpOptions": null,
    "DataDumpNoTransaction": true,
    "DataDumpRaw": false,

    /*
      Object file tree settings:
      -db --db-objects (switch) to run object files tree dump from the command line.
      -dbd --db-objects-dir to set the root output dir from the command line.
    */
    "DbObjects": false,
    "DbObjectsDir": "./Database/{0}/",
    "DbObjectsDirNames": {
      "Tables": "Tables/{0}",
      "Views": "Views/{0}",
      "Functions": "Functions/{0}",
      "Procedures": "Procedures/{0}",
      "Domains": "Domains/{0}",
      "Types": "Types/{0}",
      "Schemas": "Schemas/{0}",
      "Sequences": "Sequences/{0}",
      "Extensions": "Extensions/{0}"
    },
    "DbObjectsSkipDeleteDir": false,
    "DbObjectsOwners": false,
    "DbObjectsPrivileges": false,
    "DbObjectsCreateOrReplace": false,
    "DbObjectsRaw": false,
    "DbObjectsSchema": null,

    /*
      Markdown (MD) database dictionaries settings:
      -md --markdown (switch) to run markdown (MD) database dictionary file from the command line.
      -mdf --md-file to set generated dictionary file name from the command line.
      -cc --commit-md (switch) to run commit changes in comments from the MD file back to the database from the command line.
    */
    "Markdown": false,
    "MdFile": "./Database/{0}/README.md",
    "MdSchemaSimilarTo": null,
    "MdSchemaNotSimilarTo": null,
    "MdSkipHeader": false,
    "MdSkipToc": false,
    "MdSkipTables": false,
    "MdSkipRoutines": false,
    "MdSkipViews": false,
    "MdSkipEnums": false,
    "MdNotSimilarTo": null,
    "MdSimilarTo": null,
    "MdIncludeSourceLinks": false,
    "MdIncludeExtensionLinks": false,
    "MdIncludeUnitTestsLinks": false,
    "MdSourceLinkRoot": null,
    "MdIncludeTableCountEstimates": false,
    "MdIncludeTableStats": false,
    "MdRoutinesFirst": false,
    "MdIncludeRoutineDefinitions": false,
    "MdAdditionalCommentsSql": null,
    "MdExportToHtml": false,
    "CommitMd": false,

    /*
      PSQL command-line utility settings:
      -sql -psql -psql --sql --psql (switch) to open PSQL command-line utility from the command line.
    */
    "Psql": false,
    "PsqlTerminal": "wt",
    "PsqlCommand": "psql",
    "PsqlFallback": null,
    "PsqlOptions": null,

    /*
      Model output from a query, table, or enum settings:
      -mo --mo -model --model -model-output --model-output to set file name with expressions from which to build models. If file doesn't exists, expressions are literal. It can be one or more queries or table/view/enum names separated by semicolon.
      -mof --mof -model-file --model-file --model-output-file to set a single file name for models output. If this file extensions is "ts" it will generate TypeScript code, otherwise it will generate CSharp code. To generate TypeScript code to console set this value to ".ts".
      -mos --mos -model-save --model-save --model-save-to-model-dir (switch) to enable saving each generated model file to model dir.
    */
    "ModelOutput": null,
    "ModelOutputFile": null,
    "ModelSaveToModelDir": false,

    /*
      CRUD to routines settings:
      -create --crud-create similar to expression of table names to generate create routines.
      -create_returning -create-returning --crud-create-returning similar to expression of table names to generate create returning routines.
      -create_on_conflict_do_nothing -create-on-conflict-do-nothing --crud-create-on-conflict-do-nothing similar to expression of table names to generate create routines that will do nothing on primary key(s) conflicts.
      -create_on_conflict_do_nothing_returning -create-on-conflict-do-nothing-returning --crud-create-on-conflict-do-nothing-returning similar to expression of table names to generate create returning routines that will do nothing on primary key(s) conflicts.
      -create_on_conflict_do_update -create-on-conflict-do-update --crud-create-on-conflict-do-update similar to expression of table names to generate create routines that will update on primary key(s) conflicts.
      -create_on_conflict_do_update_returning -create-on-conflict-do-update-returning --crud-create-on-conflict-do-update-returning similar to expression of table names to generate create returning routines that will update on primary key(s) conflicts.
      -read_by -read-by --crud-read-by similar to expression of table names to generate read by primary key(s) routines.
      -read_all -read-all --crud-read-all similar to expression of table names to generate read all data routines.
      -read_page -read-page --crud-read-page similar to expression of table names to generate search and read page routines.
      -update --crud-update similar to expression of table names to generate update routines.
      -update_returning -update-returning --crud-update-returning similar to expression of table names to generate update returning routines.
      -delete_by -delete-by --crud-delete-by similar to expression of table names to generate delete by primary key(s) routines.
      -delete_by_returning -delete-by-returning --crud-delete-by-returning similar to expression of table names to generate delete returning by primary key(s) routines.
    */
    "CrudFunctionAttributes": null,
    "CrudCoalesceDefaults": false,
    "CrudNamePattern": "{0}\"{1}_{2}\"",
    "CrudCreate": null,
    "CrudCreateReturning": null,
    "CrudCreateOnConflictDoNothing": null,
    "CrudCreateOnConflictDoNothingReturning": null,
    "CrudCreateOnConflictDoUpdate": null,
    "CrudCreateOnConflictDoUpdateReturning": null,
    "CrudReadBy": null,
    "CrudReadAll": null,
    "CrudReadPage": null,
    "CrudUpdate": null,
    "CrudUpdateReturning": null,
    "CrudDeleteBy": null,
    "CrudDeleteByReturning": null
  }
}
```
