# VERSION HISTORY

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
