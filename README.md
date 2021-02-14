# PgRoutiner - Database-First Development with PostgreSQL

**`PgRoutiner`** is a set of command-line tools for the database-first development support using **`PostgreSQL`**.

It primarily targets .NET (.NET Core and .NET5+) project types but, depending on the feature - it also can be used with different project types.

## Features at a Glance

- .NET Feature: Automatically creates all the necessary **C# source-code files to call and use PostgreSQL routines** (functions and/or procedures) - implemented as connection extension methods and return results (if any).

- .NET Feature: Automatically creates a **unit test projects and unit test template source-code files** for each generated PostgreSQL routines call with each test isolated in a rolled-back transaction.

- Create a **complete schema script** file from a connection in your configuration file to include in your project source control or to be used with database unit tests.

- Create a **data dump script file**, for configured tables, from a connection in your configuration file to include in your project source control or to be used with database unit tests.

- Create a nicely formatted **script file for each, individual database object** and distribute them in directories by type (tables, views, functions, etc) to include in your project source control or use in unit tests.

- Create and maintain a database **documentation markdown (MD)** file from database object comments to include database dictionary in your source code repository. Edit markdown database dictionary comments in a markdown file and commit them back to the database to keep documentation and database comments in sync.

- Run a `psql` command-line tool easily, using your configured project connection.

- Create an **update diff database script** with the difference between two configured connections and generate migration scripts automatically and keep them in source-control.

## Installation and usage

This version is distributed as a .NET Global tool.

> ```
> $ dotnet tool install --global dotnet-pgroutiner
> You can invoke the tool using the following command: PgRoutiner
> Tool 'dotnet-pgroutiner' (version '3.0.0') was successfully installed.
> ```
