# VERSION HISTORY

## 5.2.0

* New settings `RoutinesIncludeDefintionInComment` (default false) - this setting, if set to true will include routine definition in summary comment in generated code surrounded by `<code>` tags.

This creates a nice effect when using Visual Studio intellisense autocomplete where we can see the entire routine in the tooltip nicely formatted.

* Settings `RoutinesCustomDirs` renamed to `CustomDirs` and now it applies also to DB objects (tables, views, enums, etc.) and not only to routines.

## 5.1.2

- Fix: fix extension link when using RoutinesCustomDirs in markdown output.

## 5.1.1

- Fix: fix a bug caused by the last fix (primary keys in markdown document).

## 5.1.0

- Fix: fix repeated column names (PK and FK) in markdown document.
- Fix: in markdown document, when showing table output from function, write "RECORD" when function output is a single record.

### New Feature - new `RoutinesCustomDirs` setting

This is dictionary settings where key is "similar to" expression that matches routine names and value is directory name. 
When routine name matches key, then directory name is used as directory for routine output. 
This is useful when you have many routines and you want to split them into multiple directories.

For example, if we have these settings:

```
    "OutputDir": "./Test/Extensions/{0}/",
    "RoutinesCustomDirs": {
      "%upload%": "Upload/Dir1"
    },
```

Routine with following names will be created in following directories with following names:

- `public.test1` -> `./Test/Extensions/Test1.cs` (namespace `Test.Extensions`)
- `my_schema.test2` -> `./Test/Extensions/MySchema/Test1.cs` (namespace `Test.Extensions.MySchema`)
- `public.test_upload` -> `./Test/Extensions/Upload/Dir1/TestUpload.cs` (namespace `Test.Extensions.Upload.Dir1`) - matches `"%upload%": "Upload/Dir1"`
- `my_schema.test_upload` -> `./Test/Extensions/MySchema/Upload/Dir1/TestUpload.cs` (namespace `Test.Extensions.MySchema.Upload.Dir1`) - matches `"%upload%": "Upload/Dir1"`

## 5.0.11

- Fix: fix problem with nullable models

## 5.0.10

- Fix: fix method generation when returning a single record.

## 5.0.9

- Fix: include `"#pragma warning disable CS8618"` suppression in generated code, default `SourceHeaderLines` option. Necessary for generation NON NULLABLE model fields.
- Fix: when returning USER-DEFINED type (a table), include all fields from that table in a routine query.
- Fix: when returning USER-DEFINED type (a table), set custom model as nullable type to avoid warnings.

## 5.0.8

- Fix: better error messages for custom mappings errors with suggestion to add mapping in configuration.
- Fix: enum types models generation

## 5.0.7

* Fix: When creating inserts list from a query, parse column names properly first.

## 5.0.6

* Fix: Settings `SchemaDumpNoTransaction` and `DataDumpNoTransaction` set to default true.

## 5.0.5
## 5.0.4

* Fix: Fix update returning shortcut

## 5.0.3

* Fix: Fix search bug when searching trough functions.

## 5.0.2

* Fix: Fixed bug with console parameters when working without configuration.

* Fix: returned missing settings command to show all current settings.

## 5.0.1

*Fix: Fix issue with proc.proretset

## 5.0.0

* New: Having a `cspoj` project file is no longer necessary when generating a C# code. Default name-space will be assumed from current directory name.

* New: Creation of configuration file is possible even when connection could not be established.

* New: More details on error connection.

* New: Creation of configuration file on command `--write-config-file [file name]` or `--wcf [file name]` (only when it was not created automatically)

* New: Removed entire `diff` section from the configuration file. "Diff" is still possible, but only from command line. It will remain hidden until throughly tested and stabilized.

* Fix: Improved configuration file help comments very much.

* Fix: model outputs from `ModelOutputQuery` doesn't have to contain any data for model to be generated.

* Fix: `ModelOutputQuery` renamed `ModelOutput` (and shortcuts `moq` and `-mo` or `-model`) and now contains a table or view name instead of a query.

* New: Added enums support to model generation.

* New: Added `ModelSaveToModelDir` (command line `-mos`, `--model-save-to-model-dir`, `--mos`, `--model-save`, `-model-save`) that will save each model or enum in specific file in the model directory set by `ModelDir` setting.

* New: Added `ConfigFile` setting (-cf --cf -config --config -config-file --config-file) to load custom configuration file.

* New: Returned functional `--help`.

* Fix: Fix bug not displaying routines on Definition command.

* Fix: Fix proper showing of user defined routine parameters on list command.

* Fix: bug on markdown creation on views returning enum field type.

* Fix: disable mutually exclusive settings.

* New: List setting (`-l -ls --ls --list`) is converted from bool to string. If parameter value is not included it acts like a switch. If parameter value is included it will be used as a filter for list command. Search matches are highlighted.

* New: Definition setting (`-def -ddl --ddl -definition --definition`) now can use `*` to dump all object definitions.

* New: Search setting (`-s --s --search`) will search and highlight all matching objects definitions that definition contains searched string.

* New: Info setting (`--info`) includes lot more information like exe dir, OS, pg_dump and pg_restore versions. Also, connection info includes server version.

* Fix: error trying to enumerate directory with invalid characters on execute command.

* Fix: fix routine data access code-gen when returning single complex type or set of complex types.

### New feature - CRUD generator

From version 5, `pgroutiner` can generate CRUD routines for your database tables. 

Example of generated CRUD routine (create on conflict do update) for the `test_table` table to console (`-console` switch):


```
$ pgroutiner -console -create-on-conflict-do-update-returning test_table

drop function if exists "test_table_create_on_conflict_do_update_returning"(int2, varchar, int2[], numeric);

--
-- function "test_table_create_on_conflict_do_update_returning"(int2, varchar, int2[], numeric)
--
create function "test_table_create_on_conflict_do_update_returning"(
    _id int2,
    _name varchar,
    _types int2[],
    _height_cm numeric
)
returns "test_table"
language sql
volatile
as $$
insert into "test_table"
(
    "id",
    "name",
    "types",
    "height_cm"
)
overriding system value values
(
    _id,
    _name,
    _types,
    _height_cm
)
on conflict ("id") do update set
    "name" = EXCLUDED."name",
    "types" = EXCLUDED."types",
    "height_cm" = EXCLUDED."height_cm"
returning
    "id",
    "name",
    "default",
    "default_name",
    "types",
    "height_cm",
    "height_in",
    "created_at";
$$;

comment on function "test_table_create_on_conflict_do_update_returning"(int2, varchar, int2[], numeric) is 'Create and return a new record in table "test_table" and update a row with new data on key violation (id).';

$
```

New settings:

#### Setting `CrudFunctionAttributes` (override command `--crud-function-attributes`)

- Type: string
- Default value: null (not used)

Applies additional attributes to generated CRUD functions. For example, if you want to add `security definer` attribute to all generated CRUD functions, you can set this setting to `security definer`.

#### Setting `CrudCoalesceDefaults` (override command `--crud-coalesce-defaults`)

- Type: boolean
- Default value: false

If set to true, generated CRUD functions will use `coalesce` function to set default values for columns that have default values defined in database.

This applies only to `insert` and `update` CRUD functions. When used, parameters that create or update default values will be included and will fallback to default when set to NULL.

For example: `coalesce(_parameter_with_default_value, 'default value eyxpression'::character varying),`

#### Setting `CrudNamePattern` (override command `--crud-name-pattern`)

- Type: string
- Default value: `{0}\"{1}_{2}\"`

Sets the pattern for generated CRUD function names using format placeholders: 

- `{0}` is replaced with schema name. If schema is public it is skipped, otherwise quoted schema name.
- `{1}` is replaced with table name (unquoted).
- `{2}` is replaced with applied CRUD setting suffix in lower underline case. For example, if setting `CrudCreateOnConflictDoNothingReturning` is used, this will be `create_on_conflict_do_nothing_returning`.

#### Setting `CrudCreate` (override commands `-create`, `--crud-create`)

- Type: string
- Default value: null (not used)

Similar to expression of table names to generate function for this CRUD type.

#### Setting `CrudCreateReturning` (override commands `-create_returning`, `-create-returning`, `--crud-create-returning`)

- Type: string
- Default value: null (not used)

Similar to expression of table names to generate function for this CRUD type.

#### Setting `CrudCreateOnConflictDoNothing` (override commands `-create_on_conflict_do_nothing`, `-create-on-conflict-do-nothing`, `--crud-create-on-conflict-do-nothing`)

- Type: string
- Default value: null (not used)

Similar to expression of table names to generate function for this CRUD type.

#### Setting `CrudCreateOnConflictDoNothingReturning` (override commands `-create_on_conflict_do_nothing_returning`, `-create-on-conflict-do-nothing-returning`, `--crud-create-on-conflict-do-nothing-returning`)

- Type: string
- Default value: null (not used)

Similar to expression of table names to generate function for this CRUD type.

#### Setting `CrudCreateOnConflictDoUpdate` (override commands `-create_on_conflict_do_update`, `-create-on-conflict-do-update`, `--crud-create-on-conflict-do-update`)

- Type: string
- Default value: null (not used)

Similar to expression of table names to generate function for this CRUD type.

#### Setting `CrudCreateOnConflictDoUpdateReturning` (override commands `-create_on_conflict_do_update_returning`, `-create-on-conflict-do-update-returning`, `--crud-create-on-conflict-do-update-returning`)

- Type: string
- Default value: null (not used)

Similar to expression of table names to generate function for this CRUD type.

#### Setting `CrudReadBy` (override commands `-read_by`, `-read-by`, `--crud-read-by`)

- Type: string
- Default value: null (not used)

Similar to expression of table names to generate function for this CRUD type.

#### Setting `CrudReadAll` (override commands `-read_all`, `-read-all`, `--crud-read-all`)

- Type: string
- Default value: null (not used)

Similar to expression of table names to generate function for this CRUD type.

#### Setting `CrudReadPage` (override commands `-read_page`, `-read-page`, `--crud-read-page`)

- Type: string
- Default value: null (not used)

Similar to expression of table names to generate function for this CRUD type.

#### Setting `CrudUpdate` (override commands `-update`, `--crud-update`)

- Type: string
- Default value: null (not used)

Similar to expression of table names to generate function for this CRUD type.

#### Setting `CrudUpdateReturning ` (override commands `-crud_update_returning`, `-crud-update-returning`, `--crud-update-returning`)

- Type: string
- Default value: null (not used)

Similar to expression of table names to generate function for this CRUD type.

#### Setting `CrudDeleteBy` (override commands  `-delete_by`, `-delete-by`, `--crud-delete-by`)

- Type: string
- Default value: null (not used)

Similar to expression of table names to generate function for this CRUD type.

#### Setting `CrudDeleteByReturning` (override commands `-delete_by_returning`, `-delete-by-returning`, `--crud-delete-by-returning`)

- Type: string
- Default value: null (not used)

Similar to expression of table names to generate function for this CRUD type.


