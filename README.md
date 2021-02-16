# PgRoutiner - Database-First Development with PostgreSQL

**PgRoutiner** is a set of a **command-line tools** - for the **database-first development** support using **PostgreSQL**.

**PgRoutiner** is .NET Global tool and it uses .NET (.NET Core and .NET5+) configuration and project types to generate source code and SQL files.

It also can be used with any project type to:

- Build schema, data, and object scripts.
- Generate documentation markdown (MD) from comments and commit changes back to the database.
- Generate the difference between two databases and create migration scripts on command.

## Features

### 1. .NET Feature: Build PostgreSQL routines (functions and procedures) data access code

- Builds **C# source-code files** to call and use **PostgreSQL routines** (functions and/or procedures) on command.
- Creates all necessary data-access code for your routines implemented as connection object extensions.
- Creates all necessary model classes or records.

### 2. .NET Feature: Build the unit-test project and unit test template source-code files for each generated method

- Create a **unit-test project** for your database, where each test runs in **isolation** (unique connection inside a rolled-back transaction).
- Create a **unit test template** for each generated data-access method and facilitaty **test-driven development** for PostgreSQL database. 
- Run schema, data, or migration script for your testing sessions.

### 3. Build a complete schema script

- Build a **complete schema script** file from `pg_dump` using only your configuration settings to keep it under source-control or use in a test project.

### 4. Build a complete schema script

- Build a **data script files** from `pg_dump` using only your configuration for selected tables to keep it under source-control or use in a test project.

### 5. Build a script for each database object

- Build a **formatted script** file from `pg_dump` **for each database object* to keep them under source-control or use in a test project.
- Place object scripts in subdirectories (tables, views, functions, and procedures).

### 6. Create a database dictionary in a documentation markdown (MD) file from database comments and keep them in sync

- Create a **documentation markdown (MD)** from **database comments** on database objects and keep in source-control and share it with a team.
- Edit documentation markdown (MD) comments directly in a file and commit them back to the database with a single command.
- Keep **database dictionary** in sync with database comments.

### 6. Run `psql` command-line tool easily

- Run `psql` command-line tool easily and open a new terminal on a single command using your project configuration.

### 7. Generate difference script between two databases

- Generate a **difference script between two databases** on a single command.
- Automatically generate **schema migration** scripts to keep them under source-control or use in the test project.


## Installation and usage

This version is distributed as a .NET Global tool.

> ```
> $ dotnet tool install --global dotnet-pgroutiner
> You can invoke the tool using the following command: PgRoutiner
> Tool 'dotnet-pgroutiner' (version '3.0.0') was successfully installed.
> ```
