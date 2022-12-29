# PgRoutiner - Database-First For .NET and PostgreSQL

**`PgRoutiner`** is a set of command-line tools for PostgreSQL databases and PostgreSQL .NET projects.

Using your .NET configuration project connection string (or custom-defined connection) - you can:

- Navigate and search the database with ease.
- Generate C# and TS models and code.
- Generate database scripts and run tools.
- Generate markdown documentation.
- Generate CRUD command functions.

- **See the [presentations slides](https://docs.google.com/presentation/d/1ZXGAqIyyjDc1O2YqzV94uoJ_7stg3IxpOdeCLnr6XIo/edit?usp=sharing)**

- **Successfully used by the [opennovations.eu](https://opennovations.eu/) in a highly dockerized environment.**

Table of Contents:

- [PgRoutiner - Database-First For .NET and PostgreSQL](#pgroutiner---database-first-for-net-and-postgresql)
  - [Installation](#installation)
    - [Requirements](#requirements)
    - [Global .NET tool](#global-net-tool)
    - [Local .NET tool](#local-net-tool)
    - [Docker image](#docker-image)
  - [Quick Start](#quick-start)
  - [Connection Management](#connection-management)
  - [Configuration Management](#configuration-management)
  - [Working With Database](#working-with-database)
  - [Generating Scripts](#generating-scripts)
  - [Generating Documentation](#generating-documentation)
  - [Generating Code](#generating-code)
  - [Troubleshooting](#troubleshooting)
  - [Support](#support)
  - [License](#license)
  
## Installation

### Requirements

- To use as a .NET tool, .NET 7 SDK is required. See the [global .NET tool](#global-net-tool) or the [local .NET tool](#local-net-tool) for installation details.
- To use as a Docker tool, Docker is required. See the [Docker image](#docker-image) for installation details.

### Global .NET tool

To install a global tool (recommended):

```
$ dotnet tool install --global dotnet-pgroutiner
Tool 'dotnet-pgroutiner' (version '5.0.7') was successfully installed.
```

To update a global tool:

```
$ dotnet tool update --global dotnet-pgroutiner
Tool 'dotnet-pgroutiner' was successfully updated from version '5.0.6' to version '5.0.7'.
```

This will enable a global command line tool `pgroutiner`. Try typing `pgroutiner --help`.

### Local .NET tool

To add a local tool to your project only you need to create a manifest file and add a tool without `--global` switch as described in this [tutorial](https://learn.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use).

TL-DR:

1) Add **`.config`** directory to your project or solution.

2) Add **`dotnet-tools.json`** file to this directory with the following content:

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "dotnet-pgroutiner": {
      "version": "5.0.6",
      "commands": [
        "pgroutiner"
      ]
    }
  }
}
```

- From your command line type **`dotnet tool restore`**

- Run the tool with **`dotnet tool run pgroutiner [arguments]`**, for example **`dotnet tool run pgroutiner --help`**

### Docker image

There is a [Dockerfile](https://github.com/vb-consulting/PgRoutiner/blob/master/Dockerfile) that you can use.

- Download or copy this file: `wget https://raw.githubusercontent.com/vb-consulting/PgRoutiner/master/Dockerfile`

- Run **`docker build -t pgroutiner .`** to build the image.

- To run pgroutiner type **`docker run -it --rm pgroutiner --help`** for help or **`docker run -it --rm pgroutiner --info`** to display current info or **`docker run -it --rm pgroutiner --list`** to list database objects.

- Tip: if you are using Linux, you can create an **alias** for this command.

- To mount a current directory when running (where presumably your configuration may be located) use the following switches:
    - `docker run --rm -it -v $(pwd):/home/ pgroutiner` on Linux
    - `docker run --rm -it -v ${PWD}:/home/ pgroutiner` on PowerShell
    - `docker run --rm -it -v $%cd%:/home/ pgroutiner` on Win Command-Line

- Note: to be able to access your local database, use **`host.docker.internal`** as your hostname instead of `localhost for example **`docker run` --rm -it -v ${PWD}:/home/ pgroutiner -c "Server=host.docker.internal;Db=database;Port=5432;User Id=user;Password=password;"`** - or set the network parameter to connect to netwrok. See the Docker manual for more details.

- Note2: This Dockerfile supports PostgreSQL clients 9.6, 10, 11, 12, 13, 14 and 15. 
- You can narrow it down to your version by editing line 45 and make the image smaller by omitting unnecessary client versions.

## Quick Start

1) Install `pgroutiner` (see the instructions above)
2) Open the terminal in your .NET project configured to use the PostgreSQL database.
3) Type `pgroutiner --help` to see available commands and switches (the list is long).
4) Type `pgroutiner` to define a new connection if you don't have one - and to create the default configuration file for this dir. See more on [Connection management](#connection-management)
5) Type `pgroutiner --info` to see if can you connect to the database and to see other environment info.
6) Type `pgroutiner --list` to see list of all objects.
7) Type `pgroutiner --ddl [object_name]` to see the data definition language for the object from the second parameter.
8) Type `pgroutiner --search [search expression]` to search data definitions with search expression.

## Connection Management

- `pgroutiner` is designed to run from **the .NET project root** - and it will read **any available connections** from standard configuration JSON files (like `appsettings.Development.json` and `appsettings.json`, in that order).
  
- It will use the **first available connection string** from the `ConnectionStrings` section:

appsettings.json
```json
{
  "ConnectionStrings": {
    "DvdRental": "Server=localhost;Db=dvdrental;Port=5432;User Id=postgres;Password=postgres;"
  }
}
```

- Note: this is the [Npgsql connection string format](https://www.npgsql.org/doc/connection-string-parameters.html), but, it can also use standard PostgreSQL URL connection format `postgresql://{user}:{password}@{server}:{port}/{database}`.

- Running simple info command to test the connection to see the environment:
  
```
~$ pgroutiner --info

Version:
 5.0.3.0

Executable dir:
 /home/vbilopav/.dotnet/tools

OS:
 Unix 5.10.102.1

Using configuration files:
 appsettings.Development.json
 appsettings.json

Using dir:
 /home/vbilopav/dev/dvdrental

Using settings:
 --info

Using connection DvdRental:
 Host=localhost;Database=dvdrental;Port=5432;Username=postgres  (PostgreSQL 13.8)

Using project file:
 dvdrental.csproj

pg_dump:
 pg_dump

pg_restore:
 pg_restore
```

- If the connection isn't available anywhere in the configuration files - the user is prompted to enter the connection parameters:

```
~$ pgroutiner

Connection server [localhost]:
Connection port [5432]: 
Connection database [postgres]: 
Connection user [postgres]:
Connection password [environment var.]:
```

- To specify the specific connection name, use `-c` or `--connection` parameter:

```
~$ pgroutiner -c ConnectionString2 --info
```

```
~$ pgroutiner --connection ConnectionString2 --info
```

- If the connection name is not found, or the connection is not defined - the user will be prompted to enter valid connection parameters:

```
Connection server [localhost]:
Connection port [5432]:
Connection database [postgres]:
Connection user [postgres]:
Connection password:
```

- Connection server, port, database, and user have predefined default values (`localhost`, `5432`, `postgres`, `postgres`) - hit Enter to skip and use the default.

- Command line can use the entire connection string with `-c` or `--connection` - instead of the connection name:

```
~$ pgroutiner --connection "Server=localhost;Db=test;Port=5432;User Id=postgres;Password=postgres;" --info
```

- Both, command-line and configuration files can take advantage of the PostgreSQL URL format `postgresql://{user}:{password}@{server}:{port}/{database}` - instead of [Npgsql connection string](https://www.npgsql.org/doc/connection-string-parameters.html):


```json
{
  "ConnectionStrings": {
    "TestConnection": "postgresql://postgres:postgres@localhost:5432/test"
  }
}
```

- Or, from the command line:

```
~$ pgroutiner --connection "postgresql://postgres:postgres@localhost:5432/test" --info
```

- Every part of the connection (server, port, database, user, and password) can be omitted from the connection string or connection URL and it will be replaced with the following environment variables:

  - `PGHOST` or `PGSERVER` to replace the missing server parameter.
  - `PGPORT` to replace the missing port parameter.
  - `PGDATABASE` or `PGDB` to replace the missing database parameter.
  - `PGHOST` or `PGSERVER` to replace the missing server parameter.
  - `PGUSER` to replace the missing user parameter.
  - `PGPASSWORD` or `PGPASS` to replace the missing password parameter.

## Configuration Management

- Every possible command-line option and switch can be configured in the configuration file.

- Use `PgRoutiner` configuration section in the JSON configuration (`appsettings.json` or `appsettings.Development.json`), example:

```json
{
  "ConnectionStrings": {
    "TestConnection": "postgresql://postgres:postgres@localhost:5432/test"
  },
  "PgRoutiner": {
    // pgroutiner settings
  }
}
```

- `pgroutiner` will read and apply configuration settings as the default value if the configuration section `PgRoutiner` exists. 

- Use the command-line to override any configuration settings.

- As a general rule, any configuration setting has its equivalent in the command line as the kebab-cased sitch starting with two dashes. Examples:
  - Setting: `SkipConnectionPrompt`, command line: `--skip-connection-prompt`
  - Setting: `SchemaSimilarTo`, command line: `--schema-similar-to`
  - etc

- Many settings have also shortcut alias, for example, the `Connection` setting can be either: `-c` `-conn` `--conn` `-connection` `--connection`.

- Boolean settings are switches, it is sufficient to include a switch without value to turn it on: `pgroutiner --skip-connection-prompt`
- Add `false` or `0` to turn it off: `pgroutiner --skip-connection-prompt false`

- You can inspect currently applied settings with `--settings` switch. `pgroutiner --settings` will force to skip any operation and it will only display all current settings. It will also display command-line alias in comment headers.

- You can also run `pgroutiner --info` which will, among other things, display currently applied settings that differ from default values.

- If there is no configuration section `PgRoutiner` is present anywhere, and if you run `pgroutiner` without any parameters, you will be offered to create the default configuration file `appsettings.PgRoutiner.json`:

```
You don't seem to be using any available command-line commands and PgRoutiner configuration seems to be missing.
Would you like to create a custom settings file "appsettings.PgRoutiner.json" with your current values?
This settings configuration file can be used to change settings for this directory without using a command-line.
Create "appsettings.PgRoutiner.json" in this dir [Y/N]?
```

- `appsettings.PgRoutiner.json`, if exists, will always be loaded after the `appsettings.json` and `appsettings.Development.json` and override any possible setting values in those files.

- If you need to load these configuration files from a different location, you can use `--config-path, for example, `pgroutiner -`-config-path ../../dir2/`

- You can also use any of these options to load custom configuration files: `-cf`, `--cf`, `-config`, `--config`, `-config-file`, `--config-file`, for example, `pgroutiner --config ../../dir2/my-config.json`

- You can also write current settings to a custom configuration by using any of these options `-wcf`, `--write-config-file`, for example, `pgroutiner --write-config-file ../../dir2/my-config.json`

- Here is the full configuration file with all default values for this current version (5.0.7):

```jsonc
/* PgRoutiner (5.0.7.0) settings */
{
  "ConnectionStrings": {
    "PostgresConnection": "postgresql://postgres:postgres@localhost:5432/postgres"
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
      "#pragma warning disable CS8632",
      "// pgroutiner auto-generated code"
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
      -mo --mo -model --model -model-output --model-output to set file name with expressions from which to build models. If file doesn't exists, expressions are literal. It can be one or more queries or table/view names separated by semicolon.
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

## Working With Database

## Generating Scripts

## Generating Documentation

## Generating Code

## Troubleshooting

Depending on the command, this tool will start external processes with PostgreSQL client tools like `psql`, `pg_dump`, or `pg_restore`.

That means, that PostgreSQL client tools must be installed on the system. PostgreSQL client tools will be installed by default with every PostgreSQL installation.

If you don't want a server, but only client tools:
- For Windows systems, there is option "client tools only" option in the installer.
- For Linux systems, installing the package `postgresql-client` would be enough, something like `$ sudo apt-get install -y postgresql-client`, but depends on the system.

When `pgroutiner` calls an external tool, it will first try to call the default alias `psql` or `pg_dump`. Then, if the version of the tool doesn't match the version from the connection it will try to locate the executable on the default location:

- `C:\Program Files\PostgreSQL\{0}\bin\pg_dump.exe` and `C:\Program Files\PostgreSQL\{0}\bin\psql.exe` for windows systems.
- `/usr/lib/postgresql/{0}/bin/pg_dump` and `/usr/lib/postgresql/{0}/bin/psql` for Linux systems.
- Note: format placeholder `{0}` is the major version number.

Those paths are PostgreSQL installs binaries by default. 

When PgRoutiner sees the version mismatch it will prompt a warning and fallback to that path with an appropriate version number.

This behavior can be avoided by settings `PgDumpFallback` and `PsqlFallback` settings values respectively.

## Support
 
This is open-source software developed and maintained freely without any compensation whatsoever.
  
## License
 
Copyright (c) Vedran Bilopavlović - VB Consulting and VB Software 2020
This source code is licensed under the [MIT license](https://github.com/vb-consulting/PgRoutiner/blob/master/LICENSE).

