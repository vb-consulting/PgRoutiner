# PgRoutiner - A Different Kind of Object-relational Mapping Tool for .NET projects and PostgreSQL

## What is it?


**PgRoutiner** is a set of a **command-line tools** - for the **database-first development** support using **PostgreSQL**.

**PgRoutiner** is .NET Global tool and it uses .NET (.NET Core and .NET5+) configuration and project types to generate:

- Source code in C# (functions and procedures execution and data retreival, CRUD operations and resulting models)
- Unit code templates for all generated code.
- SQL files (schema scripts, data scripts, individual object scripts, difference scripts)
- MD documentation files

It also can be used with any project type to:

- Build schema, data, and any individual object scripts.
- Generate documentation markdown (MD) from comments and commit changes back to the database.
- Generate the difference between two databases and create migration scripts on command.

High level concept info-graphics:

<img src="https://raw.githubusercontent.com/vb-consulting/PgRoutinerDemo/master/pgroutiner%20-%20concept.png" alt="concept" width="640"/>

## Features

### 1. .NET Feature: Build PostgreSQL routines (functions and procedures) data access code

- Builds **C# source-code files** to call and use **PostgreSQL routines** (functions and/or procedures) on command.
- Creates all necessary data-access code for your routines implemented as connection object extensions.
- Creates all necessary model classes or records.

### 2. .NET Feature: Build CRUD (create, read, update, delete) data access code for configured tables

- Create table record code.
- Create table on conflict do nothing code.
- Create table on conflict do update code.
- Create table returning record code.
- Create table returning record on conflict do nothing code.
- Create table returning record on conflict do update code.
- Read by keys returning record code.
- Read all records code.
- Update by record code.
- Update by returning record code.
- Delete by record code.
- Delete by returning record code.
- Creates all necessary model classes or records.

### 3. .NET Feature: Build the unit-test project and unit test template source-code files for each generated method

- Create a **unit-test project** for your database, where each test runs in **isolation** (unique connection inside a rolled-back transaction).
- Create a **unit test template** for each generated data-access method and facilitate **test-driven development** for PostgreSQL database. 
- Run schema, data, or migration script for your testing sessions.

### 4. Build a complete schema script

- Build a **complete schema script** file from `pg_dump` using only your configuration settings to keep it under source-control or use in a test project.

### 5. Build a data script file for the selected tables

- Build a **data script files** from `pg_dump` using only your configuration for selected tables to keep it under source-control or use in a test project.

### 6. Build a compact script for each database object and place them in designated subdirectories

- Build a **formatted script** file from `pg_dump` **for each database object* to keep them under source-control or use in a test project.
- Place object scripts in subdirectories (tables, views, functions, and procedures).

### 7. Create a database dictionary in a documentation markdown (MD) file from database comments and keep them in sync

- Create a **documentation markdown (MD)** from **database comments** on database objects and keep in source-control and share it with a team.
- Edit documentation markdown (MD) comments directly in a file and commit them back to the database with a single command.
- Keep **database dictionary** in sync with database comments.

### 8. Generate difference script between two databases

- Generate a **difference script between two databases** with a single command.
- Automatically generate **schema migration** scripts to keep them under source-control or use in the test project.

### 9. Run `psql` from your configuration

- Use your configuration to ipen new `psql` terminal or issue a command and inspect the output.

## Installation

1) .NET Global tool.

In order to install, you'll have to have .NET Runtime installed first and run the following command from your terminal:

> ```
> $ dotnet tool install --global dotnet-pgroutiner
> ```

You will receive the following message on successful installation:

> ```
> You can invoke the tool using the following command: PgRoutiner
> Tool 'dotnet-pgroutiner' (version '3.5.3') was successfully installed.
> ```

2) Download from executable for your system from [releases page](https://github.com/vb-consulting/PgRoutiner/releases)

## Usage

For first usage, just run PgRoutiner without any arguments:

> ```
> $ pgroutiner
> ```

When there is no available any configuration sections and there is no command line switches - you will see following question:

>```
>You don't seem to be using any available commands and PgRoutiner configuration seems to be missing.
>Would you like to create a custom settings file "appsettings.PgRoutiner.json" with your current values?
>This file can be used to change settings and run tasks without command line arguments
>Create "appsettings.PgRoutiner.json" in this dir [Y/N]?
>```

If you choose yes, the configuration file `appsettings.PgRoutiner.json` will be created in your directory.

This file will contain all possible options and settings with your current values. 

You can update and change those values and run `pgroutiner` command again without arguments.

You can override each setting using command line arguments.

To see help on each argument and settings type `pgroutiner -h` or `pgroutiner -help`.

To see current setting values type `pgroutiner -s` or `pgroutiner -settings`.

[See working with settings](https://github.com/vb-consulting/PgRoutiner/wiki/1.-WORKING-WITH-SETTINGS)

## Support
 
This is open-source software developed and maintained freely without any compensation whatsoever.
 
If you find it useful please consider rewarding me on my effort by [buying me a beer](https://www.paypal.me/vbsoftware/5)🍻 or [buying me a pizza](https://www.paypal.me/vbsoftware/10)🍕
 
Or if you prefer bitcoin:
bitcoincash:qp93skpzyxtvw3l3lqqy7egwv8zrszn3wcfygeg0mv
 
## Licence
 
Copyright (c) Vedran Bilopavlović - VB Consulting and VB Software 2020
This source code is licensed under the [MIT license](https://github.com/vb-consulting/PgRoutiner/blob/master/LICENSE).

