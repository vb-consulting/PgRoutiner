# VERSION HISTORY

## 3.18.2

- Fix wrong readme URL link.

## 3.18.1

- Fix broken TOC entries in generated Markdown for functions when containing array types

## 3.18.0

- Drop the CRUD generation support. Too many problems with too few benefits if any.

## 3.17.1

- Fix prblem with multiple constraints on CRUD models that caused doubling fields.

## 3.17.0

- Improved routines body DDL output. Better, smarter indentation and multiple lines for returning tables.
- Fixed null-able models for CRUD generation.
- Fixed default values for CRUD generation when using generated database fields.
- All CRUD lists and now strings that can use `;` separator for multiple tables. If value is `*` it will generate CRUD for all tables.

## 3.16.9

- Fix PostgreSqlConfigurationFixture to set deferred constraints

## 3.16.8

- Change default type mapping from `{"uuid", "string"}` to `{"uuid", "Guid"}` (bug due the Norm implementation support for Guid types).

## 3.16.7

- Rename option `UseNullableStrings` to `UseNullableTypes` and fix that it applies to all types, not just string (arrays too).

- Add list option `UseRecordsForModels` which contains a list of model names that will be output as record always.

## 3.16.6

- fir bug with "DbObjectsSkipDeleteDir" option that was prevent it from deleting db objects dir.

## 3.16.5

- For `PostgreSqlTestDatabaseTransactionFixture` set deferred all constraints so we don't have to insert related data.
- For the data exports from queries (inserts generation), also include the select list from the original query into generated insert statement.

## 3.16.4

- Unusable, upgrade to 3.16.5

## 3.16.3

- Improve unit test templates, fix typos, fix auto comments and add base fixture with public connection (for extensions).

## 3.16.2

- Fix missing db tree output directories (schemas and extensions).

- Change help link to README.md

## 3.16.1

- Change duplicate alias for ModelDir (`-md`) to `-modeldir`

## 3.16.0

- Switch `Dump` renamed to `DumpConsole` because it was confusing what it actually does. It just forces content out the file to console. Alias `-d` renamed the same.

- Removed `Options` option that was adding additional command options to execute PSQL commands. Instead, additional command are `;` separated. 

For example: `"\H;select * from countries limit 3"`

This also applies when executing files, for example: `"\H;test.sql"`

- Fix boolean handling in console parser.

- Allow multiple commands.

## 3.15.0

- Added `Options` (alias '-opt') that adds a command option to all psql commands (executed by execute or psql terminal).
- Replaced `help` options with the link to help page.
- Added options for backups and restores:
    
    - `Backup` (alias `-backup`) - crates a backup given (compression 9, 10 jobs) in a dir supplied by this options:

        - Backup dir name can have two format placeholders: {0} for current date time and {1} for current connection name.
        - Additional options for pg_dump are also supplied from this string, after the file name, e.g. `-backup "./backup{0} -additional options"`

    - `Restore` (alias `-restore`) - restores database from given backup (10 jobs):

        - Additional options for pg_restore are also supplied from this string, after the file name, e.g. `-backup "./backup{0} -additional options"`

    - By default backup and restore will ignore owner. Use `BackupOwner` and `RestoreOwner` switches to change that.

## 3.14.3

- Output current dir by default always, but silence if silence is on.

## 3.14.2

- Make Silent switch available from configuration

## 3.14.1

- Fix connection prompt bug

## 3.14.0

- Fix data dump export when running on zero tables.

- Added `Definition` value switch (alias `-def`) that dumps SQL object definition to console.

- Normalize a bit console color outputs to be more consistent.

- Switch `-i˙` that only shows info and prevent any action is changed to `-info`.

- Added `Silent` switch with alias `-silent`:

Use `-silent` or `--silent` to silent console output except for that was instructed (like intentional console dumps).

It replaces `-d` or `--dump` for silencing console output.

`-d` or `--dump` is still used to redirect output from files to console.

- Added `Inserts` switch option with alias `-i`:

It overrides `DataDumpList` and sets `DataDump` to true. 
This is only to replace `-dd -ddl` combination with one simple `-i`.

- Added `List` switch with alias `-l`: lists all objects from database for schema.

## 3.13.0

- Improve unit tests:

Database Fixtures now have four classes: 
    - `PostgreSqlConfigurationFixture` default, uses configuration file, same as before
    - `PostgreSqlTestDatabaseFixture` only test database (created at the start, dropped at the end)
    - `PostgreSqlTestDatabaseTransactionFixture` same as previous but only every test under rolled back transaction
    - `PostgreSqlTestTemplateDatabaseFixture` uses a template database

Now every test can be configured differently. 
Also, moved test hedader comment to class header.

- Improve dumps:

If file (for schema or data dump) is null or empty, dump to console.

Change default `SchemaDumpFile` and `DataDumpFile` to null (dump to console).

- Remove `DbObjectsRemoveExistingDirs` options. It's needed and it caused some funky behavior.

- Added `DataDumpRaw` with default value false. 

When this settings is false, data dump will contain only insert statements, everythiong else is ommited, otherwise it will be raw, untouched.

- Added `DataDumpList`

    - Allows for the semicolon separated list of tables or queries to be dumped.
    - This option has command line alias `-ddl`, for exmple ` -ddl "countries;business_areas"`
    - This semicolon separated values are merged with option `DataDumpTables`
    - Both `DataDumpList` and `DataDumpTables` are now supporting queries. 
    - When dumping query, temp table which later dropped is used for export, but the actaul table name in export dump is replaced with last "FROM" expression.


## 3.12.7

- Added `RoutinesLanguages` settings with default value as hashet array `["sql", "plpgsql"]` which determines which languagues will be included when parsion routines in any section. This enables adding custom languagues like `plpython3u`.

## 3.12.6

- Fix extensions parsing in boject tree.

## 3.12.5

- When routines returns a record with single value, skip creating a model. 

## 3.12.4

- Fix routines generation bug when routines returns record with only single field.

## 3.12.3

- Add support for extensions in object tree files.
- Fix comments new lines in unit test files.
- Add support for default parameters. Parameters with default values with have one more overload without that parameter.

## 3.12.2

- Support for PostgreSQL 14 function syntax

## 3.12.1

- Fix parsing bug on object tree files with new lines.
- Parse default values nicer on object tree files.

## 3.12.0

- `DbObjects` functionality (creating an object tree files) optimized and now it's faster many times. Number of `pg_dump` processes significanlty lowered by caching and smart processing of same dump.
- Removed `DbObjectsDropIfExists` option since it doesn't mean anything with the object tree files and caused problems with this optimization.

## 3.11.5

- Fix unit test generation bug with multiple methods.
- Add summary comment header from routine description plus routine name.
- Add parameters to summary on routine extensions code.

## 3.11.4

- Fix `MdIncludeUnitTestsLinks` bug.
- Add global namespaces support to unit testing modules.
- Add direcotires by schemas to unit testing modules.

## 3.11.3

- Remove source line header from `testsettings.json`.
- Fix typos and grammar in `testsettings.json` comments.
- Added `MdIncludeUnitTestsLinks` that will include unit tests links to markdown document.

## 3.11.2

- Fix default expression in markdown dictionary to include generated table expressions

## 3.11.0

- Added `MdExportToHtml` settings which if set to true rendreds markdown dictionary to html file by using github style. This will produce a single html file without dependencies.

- Fix comments in enum section.

## 3.10.1

Add missing using `System.Runtime.CompilerServices` when `RoutinesCallerInfo` is on.

## 3.10.0

- Moved table stats in markdown bellow table.
- `MdRoutinesFirst` setting, set to true to put routines (functions and procedures) first in markdown.
- `RoutinesCallerInfo` setting, set to true to include caller info (caller member info, source file and line) in routine calls, that may be configured to be logged with Norm.

## 3.9.10

Two new options for Markdown document:
- `MdIncludeTableCountEstimates: false` - set to true to include table count estimates for each table.
- `MdIncludeTableStats": true` - set to true to include detailed statisticts for each table for database administration.

## 3.9.9

- Fixed extensions generation when returning single value.
- Fixed double newlines in markdown.
- Added setting `MdIncludeExtensionLinks` that includes generated C# source code links in markdown.

## 3.9.8

- Added "for tables" in "data file" toc entry of markdown file.

## 3.9.7

- Fixed type artifacts in db tree files.

- Aded `MdSourceLinkRoot` settings to set root when `MdIncludeSourceLinks` is included.

## 3.9.6

Massive improvement on markdown documnetation. Now includes proper user defined type names and enum values. It can also include source links.

New settings:

- `MdSkipEnums` - don't include enums into markdown, default is false (they are included).
- `MdIncludeSourceLinks` - includes links to source file generated by `DbObjects` (`-db` or `--db-objects` switches). Note: it will include links even when files are not generated.

## 3.9.5

Fix markdown comment query on partitioned tables

## 3.9.4

When building schema file, comment out dropping primary key constraints on table partitions because that raises `ERROR:  cannot drop inherited constraint` error.

## 3.9.3

Add support for partitions in markdown dictionary document

## 3.9.2

Add support for partitions in db object tree

## 3.9.1

- Add `.WithCommandBehavior(System.Data.CommandBehavior.SingleResult)` if pg function returns single results
- Add `text` and `varchar` to `RoutinesUnknownReturnTypes` settings so that string types are returned as raw strings

## 3.9.0

- Migrate to Norm 5.2.1

- Use PostgreSQL positional parameters for routines call to disable query rewriting

- Use select for routines to avoid "select *"

- Add pragma to header disable warnings

## 3.8.0

Migrate to Norm 5.0.

## 3.7.6

- Data objects scripts tree creation before schema or data dump scripts.
- Add `DbObjectsRemoveExistingDirs` object that will remove old existing directories when data objects scripts tree creation ends.

## 3.7.5
## 3.7.4
## 3.7.3

- Fix bugs with `PgDumpFallback` and `PsqlFallback` when settings is null.
- Improve ConfigPath settings to use actual path instead of file.

## 3.7.2

Settings `PgDumpFallback` and `PsqlFallback` have null default values now.

When those settings are set to null (default), system defaults are used:

Windows

- `C:\Program Files\PostgreSQL\{0}\bin\pg_dump.exe`
- `C:\Program Files\PostgreSQL\{0}\bin\psql.exe`

Non-Windows

- `/usr/lib/postgresql/{0}/bin/pg_dump`
- `/usr/lib/postgresql/{0}/bin/psql`

## 3.7.1

- Fix types dump files- 
- Improve DbObjects file dumps by skipping unnecessary pg_dump calls. 

## 3.7.0

- Added default values to MethodParameterNames
- Fixed code gen to use Norm 4.0.0
- Upgraded to Norm 4.0.0 and set minimal version to 4.0.0
- Renamed setting 4.0.0 SimilarTo to RoutinesSimilarTo and NotSimilarTo to RoutinesNotSimilarTo

## 3.6.3

- New type mapping {"uuid", "string"}.
- Schema option is renamed to Sche.maSimilarTo
- Added option SchemaNotSimilarTo.
- Added ability to use pgroutiner.json inseatd of appsettings.pgroutiner.json. pgroutiner.json will ovberride appsettings.pgroutiner.json.
- Improved error messages (includes schema and parameter name).
- Limit code generation and markdown only to the sql and plpgsql functions and procedures.
- Added RoutinesSchemaSimilarTo and RoutinesSchemaNotSimilarTo that can override SchemaSimilarTo and SchemaNotSimilarTo.
- Added MdSchemaSimilarTo and MdSchemaNotSimilarTo that can override SchemaSimilarTo and SchemaNotSimilarTo.
- Fix psql process to receive password properly.
- Try to execute files first with psql process, if fails, use standard Npgsql executor.

## 3.6.2

Upgrade to .NET6

## 3.6.1

- Change default source header comment from `// <auto-generated />` to `// pgroutiner auto-generated code` because Visual Studio thinks that he generated that file and complains a bit.

## 3.6.0

- Added `DumpPgCommands` setting with default value true. This will write all `pg_dump` and `psql` system calls to prompt without passwords.

- Added `PsqlFallback` setting for fallback for psql command if version doesn't match. Default is `C:\\Program Files\\PostgreSQL\\{0}\\bin\\psql.exe` for windows and `/usr/lib/postgresql/{0}/bin/psql` for other operationg systems. `{0}` is format placeholder for version number.

- `-psq` command now connects using the same method as `pg_dump` that adds `PGPASSWORD` enviorment variable at the runtime so that it can be used for Azure connections.

- Added `UseFileScopedNamespaces` setting with default value true. This will produce modules with file-scoped namespaces. File-scoped namespaces are only supported with C#10 (default for .NET6).

- Added `UnitTestProjectTargetFramework` setting that will set the target framework when generating unit test project file. Allowed values are `net5.0` and `net6.0` (default).

- Added `UnitTestProjectLangVersion`: setting that will set the languague version when generating unit test project file. Alloed values are `9`, `10` or null (skips this entry, default).

- Bug fix: fix hardcoded namespace for the Unit Test fixtures code file.

- Added `NullableStrings` setting that will use nullable string format `string?` in all parameters or in models where associated field is nullable. Default is true.

## 3.5.7

### Fix diff engine connectivity bug.

### Added `DiffSkipSimilarTo` settings.


This option, if it is not null - will force the diff engine to skip objects that names are [similar](https://www.postgresql.org/docs/9.0/functions-matching.html) to its value.

Default value is "`pg_%`" which will skip all objects that have names that starts with `pg_`.


## 3.5.6

- Fix connection managamanet to accept any additional parameters (like SslMode).
- Fix pg_dmup command to be able to use azure connection string format (user is username@database).

## 3.5.5

### Fix output of using configuration files to not include ConfigPath when not used. Very minor fix.

### Command line switch `--settings` or shorter `-s` can now have value of custom configuration file.

By default program will try to use `appsettings.PgRoutiner.json` is it exists.

You can change this value by supplying a value to this command line switch. For example:

```
$ pgroutiner -s myconfig.json
```

or

```
$ pgroutiner -s=myconfig.json
```

or

```
$ pgroutiner --settings myconfig.json
```

or

```
$ pgroutiner --settings=myconfig.json
```

This will change this default from `appsettings.PgRoutiner.json` to `myconfig.json`.

Program will not raise an exception if the config doesn't exists.

This is useful if you want to separate configurations for code generation and for script generation in a scenarion where code generation is triggered after succeseuful build event for exampple.

## 3.5.4

Settings `Maping` now can contain a custom model name that will replace the result of routine or a crud operation.

For example if Routine result should serialize to an existing class or a record model in your system you can do following:

```json
    "Mapping": {
        // ... existing mappings
        "YourGeneratedModelName": "CustomModelName"
    }
```

This "CustomModelName" is a model that already exists in your system and therefor it will not be generated.

This applies to routines and crud operations.

## 3.5.3

- Fix unit test assert code
- Fix import models in generated unit test
- Fix file creation unknown directory bug
- Fix unit tests not creating crud templates bug
- Fix "async void" on generetad delete by method
- Add comment in generated unit test template in assert section: // todo: adjust assert logic template to match actual logic
- Improve unit test template project configuration comments

## 3.5.2

- If key for settings DbObjectsDirNames (Tables, Views, etc) has null value, that section is skipped.

- Settings DbObjectsDirNames supports subdirectories by object schema. If schema is public, subdirectory is not applied. New defaults are:

```
    "DbObjectsDirNames": {
      "Tables": "Tables/{0}",
      "Views": "Views/{0}",
      "Functions": "Functions/{0}",
      "Procedures": "Procedures/{0}",
      "Domains": "Domains/{0}",
      "Types": "Types/{0}",
      "Schemas": "Schemas",
      "Sequences": "Sequences/{0}"
    }
```

- Format placeholders `{0}` are replaced with schema name if schema is not public.

## 3.5.1

- Fix create schema dump transformer bug
- Add schema name to generated routine name const

## 3.5.0

- Include build script for a single stand-alone executable builds: `build.bat` and `build.sh`.

- Fix schema settings bug that prevented filtering specific schemas.

- Remove CrudDelete and CrudDeleteReturning because they are essentially same as CrudDeleteBy and CrudDeleteByReturning

- For output dir settings (ModelDir, OutputDir and CrudOutputDir) add default {0} formatter that, if present, is replaced with non public schema.
 
- Added MethodParameterNames dictionary settings. You can map generated parameter names here. 

For example, `event` or `var` are generated C# keyword, so if you have parameter with that name, you can map the name to something else:
`"MethodParameterNames": {"event": "@event"},`


-  RoutinesModelPropertyTypes dictionary settings. You can map custom model types here. 

For example `RoutinesModelPropertyTypes: {"ModelClass.Field": "int"}` forces property `Field` or model `ModelClass` to always fallback to type `int`.

## 3.4.1

- Add missing RoutinesReturnMethods settings key when creating settings file.

## 3.4.0

- Remove help url comment from auto generated configuration (until website is build)

- If there is no any command-line argument present and no PgRoutiner file-based configuration, prompt a question:

> You don't seem to be using any available command-line commands and PgRoutiner configuration seems to be missing.
Would you like to create a custom settings file "appsettings.PgRoutiner.json" with your current values?
This settings configuration file can be used to change settings for this directory without using a command-line.
Create "appsettings.PgRoutiner.json" in this dir [Y/N]?

- Previosly, it is was only on some commands, now Pgroutiner check if any valid command-line is issued. And the text sucked, this one is better.

- Now `Connection` settings doesn't have to be a connection name. It can be entire connection string. This is convinient when you want to set connection from the command line:

`$ pgroutiner -c "postgresql://postgres:postgres@localhost:5434/database"`
or
`$ pgroutiner -c "Host=localhost;Database=venture;Port=5434;Username=postgres;Password=postgres"`
or
`$ pgroutiner --connection "postgresql://postgres:postgres@localhost:5434/database"`
or
`$ pgroutiner --connection "Host=localhost;Database=venture;Port=5434;Username=postgres;Password=postgres"`

Is some parts are missing, user will be prompted to enter (unless `SkipConnectionPrompt` is set to true).

- Added warning for tables that don't exists in a database but exists in a CRUD code generation configuration:
> WARNING: Some of the tables in CRUD configuration are not found in the database. Following tables will be skiped for the CRUD code generation: a, b, c

- Unit test fixtures new settings:
  - `TestDatabaseFromTemplate` true or false: if set to true, test database will be created by using database from a TestConnection as database template.
  - `UnitTestsUnderTransaction` true or false: if set to true, each unit test is under new transaction that is rolled back after test is done.
  - `UnitTestsNewDatabaseFromTemplate` true or false: if set to true, each unit test creates a new, unique database by using database from a TestConnection as database template.

Creating database from a template copies all schema and data instantly.

Configuration settings TestDatabaseFromTemplate=true and UnitTestsNewDatabaseFromTemplate=true doesn't make any sense.
There is no point of creating a test database from a template and to do that again for each unit test.

Configuration settings UnitTestsNewDatabaseFromTemplate=true and UnitTestsUnderTransaction=true doesn't make any sense.
There is no point of creating a new test database from a template for each test and then use transaction on a database where only one test runs.

Configuration settings UnitTestsNewDatabaseFromTemplate=true and up or down scripts (UpScripts, DownScripts) doesn't make any sense.
Up or down scripts are only applied on a test database created for all tests.


## 3.3.13

- Add `CrudReturnMethods` dictionary settings. Same settings as `RoutinesReturnMethods`, but only for CRUD methods. Key can be either table name or generated method name. Added for consistency.
- Fix automatic comments on async CRUD methods

## 3.3.12

- Rename setting name `SingleLinqMethod` to `ReturnMethod`
- Add `RoutinesReturnMethods` dictionary settings. This settings overrides `ReturnMethod` for individual routines.
  - Key is either routine method or a generated method name.
  - Value is name of the .NET method (Linq) that yield single result (like Single or First) or NULL to yield an enumeration.

## 3.3.11
## 3.3.10

- Fix unit test fixture for external config files

## 3.3.9

- Fix generation of DeleteBy unit test template
- Fix new line characters in routine comments

## 3.3.8

- Added ConfigPath settings to be able to reference configuration from another dir or project. This is useful in solutions with multiple projects to avoid repeating connection string.
- Added source header to auto generated unit test files (testsettings.json and TestFixtures.cs)
- Added ConfigPath settings in a test configuration along with comments for each key
- Added DeleteBy and DeleteByReturning missing CRUD generators
- Fix command line help abnd add missing settings

## 3.3.7

- Apply custom names to crud extensions and files

## 3.3.6

- Fix snake name parameter names on crud generated code

## 3.3.5

- Add missing Microsoft.Extensions.Configuration.Json and Microsoft.Extensions.Configuration.Binder in tests projects

## 3.3.4

- For new unit tests project add package Microsoft.Extensions.Configuration 
- Add missing custom namespace into generated tests

## 3.3.3

- Fix bug when emtying dirs. Don't remove non cs files.

## 3.3.2

- Add custom names support for crud generation (Settings dictionary `CustomModels` that changes model name to something else).
- New settings key `ModelCustomNamespace` - if not null, sets custom namespace for generated models.

## 3.3.1
 
- Following settings changed the default values:
  - `ModelDir` from null to `./Models`
  - `OutputDir` from `./DataAccess` to `./Extensions`
  - `MdFile` from `./Database/{0}/Dictionary.md` to `./Database/{0}/README.md`
  
- Following settings renamed:
  - From `DbObjectsSkipDelete` to `DbObjectsSkipDeleteDir` (emptys dir before file generation, default is false)
 
- New settings that will enable (disabled by default) to remove previous files in a target dir where source files are generated. For example, if some database objects are dropped, I want them gone from these source files too. These settings will not work when the target dir is project root (the program just yields a warning) so that we don't delete something accidentally.
  - `EmptyModelDir` - empties model dir of previous files
  - `RoutinesEmptyOutputDir` - empties routines dir of previous files
  - `CrudEmptyOutputDir` - empties CRUD dir of previous files
 
## 3.3.0
 
- Fix indexing problem in command line
- Fix current settings output comment coloring
- Show list on currently using settings
- Fix unit test async methods 
- Fix double test methods
- Build unit test templates only on units that have existing generated file
- Model classes from routines generated from record result gets new name "RoutineNameResult"
- User-defined model results from a routines (table result) are always in separate model file
- UseStatementBody replaced UseExpressionBody
- Fix char type mapping in the parameter type
- Fix and uncomment unit tests assert part
- Added SingleLinqMethod setting to be able to change Linq method that returns a single value and set it to SingleOrDefault
- Added UnitTestsSkipSyncMethods and UnitTestsSkipAsyncMethods settings to be able to skip async or sync test methods generation
 
- Add CRUD support settings and move shared code generation settings to general code generation settings section.
- New CRUD support generator settings:
 
  - `"Crud": true,` eneables/disables crud generartion
  - `"CrudOutputDir": "./Test",` - generated source files output dir
  - `"CrudOverwrite": true,` - should generated source files be overwitten or not
  - `"CrudAskOverwrite": false,` - should program ask shout it overwrite generated source files or not
  - `"CrudNoPrepare": false,` - don't prepare generated statements
  - `"CrudCreate": [ "test" ],` - generate a create extension for tables in list
  - `"CrudCreateReturning": [ "test" ],` - generate a create returning extension for tables in list
  - `"CrudCreateOnConflictDoNothing": [ "test" ],` - generate a create on conflict do nothing extension for tables in list
  - `"CrudCreateOnConflictDoNothingReturning": [ "test" ],` - generate a  on conflict do nothing returning extension for tables in list
  - `"CrudCreateOnConflictDoUpdate": [ "test" ],` - generate a create on conflict do update extension for tables in list
  - `"CrudCreateOnConflictDoUpdateReturning": [ "test" ],` - generate a create on conflict do update returning  extension for tables in list
  - `"CrudReadBy": [ "test" ],` - generate a ready by extension for tables in list
  - `"CrudReadAll": [ "test" ],` - generate a read all extension for tables in list
  - `"CrudUpdate": [ "test" ],` - generate a update extension for tables in list
  - `"CrudUpdateReturning": [ "test" ],` - generate a update returning extension for tables in list
  - `"CrudDelete": [ "test" ],` - generate a delee extension for tables in list
  - `"CrudDeleteReturning": [ "test" ]` - generate a delete returning extension for tables in list
 
## 3.2.0
 
- Fix method body methods when on void functions
- Fix same model repetition for function overloads
- Add sequences support for object files (only sequences not used in identites) tree and diff generator
 
## 3.1.16
 
- If "Execute" (exlusive -x switch) contains a file pattern that yields valid files, or valid directory - all files will be executed, only in top level directory.
- If there is no valid file, command will execute PSQL command as before.
- Connection now writes notices to console that connection might yield.
 
## 3.1.15
 
- Added "SkipConnectionPrompt" option.
 
If some value from the connection string (host, port, database, user or password) is missing (empty), program will automatically prompt for input.
 
When system enviorment variable is defined for that part (PGHOST, PGSERVER, PGPORT, PGDATABASE, PGDB, PGUSER, PGPASSWORD, PGPASS) program will take that as a default value. Simply leave value empty in prompt (hit enter) to use enviorment variable.
 
If "SkipConnectionPrompt" is true and enviorment variable is defined, enviorment variable will be used without propmpting the user.
 
## 3.1.14
 
- Fix connection string parsing bug
 
## 3.1.13
 
- "wait for exit" on psql shell execute only if not on windows
 
## 3.1.12
 
- Add "wait for exit" on psql shell execute to fix early exit on linux
 
## 3.1.11
 
- Fix connection prompt in partial connection strings.
 
## 3.1.10
 
- Added `-i` or `--info` command-line switch that just display current info (dir, config files, used settings and connection). and exits without any file file generation.
- If following switches are on: `Execute`, `Psql` or `CommitMd`, all other file generations switches are off (`DbObjects`, `SchemaDump`, `DataDump`, `Diff`, `Routines`, `UnitTests` and `Markdown`)
- If `PsqlTerminal` program is not supported by the operations system, fallback to the default shell execute of the operating system.
 
## 3.1.9
## 3.1.8
 
- Fix typo in settings comment hint
- 
## 3.1.7
 
- Fix postgresql connection string format
 
## 3.1.6
 
- Change connection string formatting and add asterisk to password prompt
 
## 3.1.3
 
- when entering password, use blank (hit enter key) to use envirment variable
 
## 3.1.2
 
- fix connection prompt text
- add schema support to database objects tree and diff generator
- further improve generated settings file comments
- include check defintion expressions into generated dictionary file
 
## 3.1.1
 
- fix duplication of foreign keys in a markupd dictionary document
- improve generated settings file comments
- add "-v", "--version" switch to dump current version
- rename exe to lowercase "pgroutiner" for case-sensitive terminals
 
## 3.1.0
 
- Fixed issues with comman line parser
- Fixed issues with recreating directories on database objects tree
- Added Domains and Types to database objects tree and diff engine
