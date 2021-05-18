# VERSION HISTORY

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
- If `PsqlTerminal` program is not supported by the operations system, fallback to the default shell execute of the operationg system.
 
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
