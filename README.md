# PgRoutiner - Database-First For .NET and PostgreSQL

**`PgRoutiner`** is a set of command-line tools for PostgreSQL databases and PostgreSQL .NET projects.

Using your .NET configuration project connection string (or custom-defined connection) - you can:

- Navigate and search the database with ease.
- Generate C# and TS models and code.
- Generate database scripts and run tools.
- Generate markdown documentation.
- Generate CRUD command functions.

- **See the [presentation slides](https://docs.google.com/presentation/d/1ZXGAqIyyjDc1O2YqzV94uoJ_7stg3IxpOdeCLnr6XIo/edit?usp=sharing)**

- Note: all examples in this readme, as well as in the presentation above use [PostgreSQL Sample Database](https://www.postgresqltutorial.com/postgresql-getting-started/postgresql-sample-database/) from [PostgreSQL Tutorial]

- Quick start: 

> 1. Download native executable files (not dependent on any framework) for the latest version from the [releases page](https://github.com/vb-consulting/PgRoutiner/releases/).
> 2. Set the appropriate path to the downloaded executable file.
> 3. Type `pgroutiner --info`
> 4. Note: working with native executables is many times faster, they very short startup time and they offer many times better user experience.

Table of Contents:

- [PgRoutiner - Database-First For .NET and PostgreSQL](#pgroutiner---database-first-for-net-and-postgresql)
  - [Installation](#installation)
    - [Download Binaries](#download-binaries)
    - [.NET Tool](#net-tool)
      - [Requirements](#requirements)
      - [Global .NET tool](#global-net-tool)
      - [Local .NET tool](#local-net-tool)
  - [Quick Start](#quick-start)
  - [Connection Management](#connection-management)
  - [Configuration Management](#configuration-management)
  - [Working With Database](#working-with-database)
    - [List database objects](#list-database-objects)
    - [View object definitions](#view-object-definitions)
    - [Search definitions by search expression](#search-definitions-by-search-expression)
    - [Dump data from tables or queries as inserts](#dump-data-from-tables-or-queries-as-inserts)
    - [Backup and restore](#backup-and-restore)
    - [Open PSQL tool](#open-psql-tool)
    - [Execute PSQL commands and scripts](#execute-psql-commands-and-scripts)
  - [Generating Scripts](#generating-scripts)
  - [Generating Documentation](#generating-documentation)
  - [Generating Code](#generating-code)
  - [Troubleshooting](#troubleshooting)
  - [Support](#support)
  - [License](#license)
  
## Installation

### Download Binaries

This is the fastest and easiest way to get started with the PgRoutiner tool. The [releases page](https://github.com/vb-consulting/PgRoutiner/releases/) contains downloadable executables that are not dependent on anything. No framework or docker is required, just plain old native executable. 

This is actually, the preferable way of using the PgRoutiner tool. Native executables are very much optimized and have very short startup time and they offer many times better user experience.

Here are the steps:

1. Download native executable files (not dependent on any framework) for the latest version from the [releases page](https://github.com/vb-consulting/PgRoutiner/releases/).
2. Set the appropriate path to the downloaded executable file.
3. Type `pgroutiner --info` to see f it works.

That is it.

### .NET Tool

#### Requirements

- To use as a .NET tool, .NET 8 SDK is required. See the [global .NET tool](#global-net-tool) or the [local .NET tool](#local-net-tool) for installation details.

#### Global .NET tool

To install a global tool (recommended):

```console
$ dotnet tool install --global dotnet-pgroutiner
Tool 'dotnet-pgroutiner' (version '5.4.0') was successfully installed.
```

To update a global tool:

```console
$ dotnet tool update --global dotnet-pgroutiner
Tool 'dotnet-pgroutiner' was successfully updated from version '5.0.7' to version '5.0.8'.
```

This will enable a global command line tool `pgroutiner`. Try typing `pgroutiner --help`.

#### Local .NET tool

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

- Run the tool with **`dotnet tool run pgroutiner [arguments]`**, for example, **`dotnet tool run pgroutiner --help`**

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

- The `pgroutiner` is designed to run from **the .NET project root** - and it will read **any available connections** from standard configuration JSON files (like `appsettings.Development.json` and `appsettings.json`, in that order).
  
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
  
```console
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
 /usr/lib/postgresql/13/bin/pg_restore
```

- If the connection isn't available anywhere in the configuration files - the user is prompted to enter the connection parameters:

```console
~$ pgroutiner

Connection server [localhost]:
Connection port [5432]: 
Connection database [postgres]: 
Connection user [postgres]:
Connection password [environment var.]:
```

- To specify the specific connection name, use `-c` or `--connection` parameter:

```console
~$ pgroutiner -c ConnectionString2 --info
```

```console
~$ pgroutiner --connection ConnectionString2 --info
```

- If the connection name is not found, or the connection is not defined - the user will be prompted to enter valid connection parameters:

```console
Connection server [localhost]:
Connection port [5432]:
Connection database [postgres]:
Connection user [postgres]:
Connection password:
```

- Connection server, port, database, and user have predefined default values (`localhost`, `5432`, `postgres`, `postgres`) - hit Enter to skip and use the default.

- The Command line can use the entire connection string with `-c` or `--connection` - instead of the connection name:

```console
~$ pgroutiner --connection "Server=localhost;Db=test;Port=5432;User Id=postgres;Password=postgres;" --info
```

- Both command-line and configuration files can take advantage of the PostgreSQL URL format `postgresql://{user}:{password}@{server}:{port}/{database}` - instead of [Npgsql connection string](https://www.npgsql.org/doc/connection-string-parameters.html):


```json
{
  "ConnectionStrings": {
    "TestConnection": "postgresql://postgres:postgres@localhost:5432/test"
  }
}
```

- Or, from the command line:

```console
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

- Use the `PgRoutiner` configuration section in the JSON configuration (`appsettings.json` or `appsettings.Development.json`), example:

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

- Use the command line to override any configuration settings.

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

```console
You don't seem to be using any available command-line commands and PgRoutiner configuration seems to be missing.
Would you like to create a custom settings file "appsettings.PgRoutiner.json" with your current values?
This settings configuration file can be used to change settings for this directory without using a command-line.
Create "appsettings.PgRoutiner.json" in this dir [Y/N]?
```

- `appsettings.PgRoutiner.json`, if exists, will always be loaded after the `appsettings.json` and `appsettings.Development.json` and override any possible setting values in those files.

- If you need to load these configuration files from a different location, you can use `--config-path, for example, `pgroutiner -`-config-path ../../dir2/`

- You can also use any of these options to load custom configuration files: `-cf`, `--cf`, `-config`, `--config`, `-config-file`, `--config-file`, for example, `pgroutiner --config ../../dir2/my-config.json`

- You can also write current settings to a custom configuration by using any of these options `-wcf`, `--write-config-file`, for example, `pgroutiner --write-config-file ../../dir2/my-config.json`

- [The full configuration file with all default values for this current version](https://github.com/vb-consulting/PgRoutiner/blob/master/readme-default-config.md)

## Working With Database

### List database objects

- Description: `-l -ls --ls --list to dump or search object list to console. Use the switch to dump all objects or parameter values to search.`

- Examples:

```console
vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$ pgroutiner --list
SCHEMA public
EXTENSION plpgsql
TYPE mpaa_rating
DOMAIN public.year
TABLE public.actor
VIEW public.actor_info
VIEW public.customer_list
...
```

```console
vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$ pgroutiner -l
SCHEMA public
EXTENSION plpgsql
TYPE mpaa_rating
DOMAIN public.year
TABLE public.actor
VIEW public.actor_info
VIEW public.customer_list
...
```

```console
vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$ pgroutiner -ls
SCHEMA public
EXTENSION plpgsql
TYPE mpaa_rating
DOMAIN public.year
TABLE public.actor
VIEW public.actor_info
VIEW public.customer_list
...
```

- Note: when searching through the object list, the match is highlighted.

```console
vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$ pgroutiner --list film
VIEW public.film_list
VIEW public.nicer_but_slower_film_list
VIEW public.sales_by_film_category
TABLE public.film_actor
TABLE public.film_category
TABLE public.film
SEQ public.film_film_id_seq
FUNCTION public.film_in_stock(integer, integer)
FUNCTION public.film_not_in_stock(integer, integer)
vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$
```

```console
vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$ pgroutiner --list actor
TABLE public.actor
VIEW public.actor_info
TABLE public.film_actor
SEQ public.actor_actor_id_seq
vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$
```

### View object definitions

- Description: `-def -ddl --ddl -definition --definition to dump object schema definition in console supplied as a value parameter.`

- Examples:

```console
vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$ pgroutiner -ddl actor
--
-- Table: public.actor
--
CREATE TABLE public.actor (
    actor_id integer DEFAULT nextval('public.actor_actor_id_seq'::regclass) NOT NULL PRIMARY KEY,
    first_name character varying(45) NOT NULL,
    last_name character varying(45) NOT NULL,
    last_update timestamp without time zone DEFAULT now() NOT NULL
);

ALTER TABLE public.actor OWNER TO postgres;
CREATE INDEX idx_actor_last_name ON public.actor USING btree (last_name);
CREATE TRIGGER last_updated BEFORE UPDATE ON public.actor FOR EACH ROW EXECUTE FUNCTION public.last_updated();


vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$
```

```console
vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$ pgroutiner -ddl film
--
-- Table: public.film
--
CREATE TABLE public.film (
    film_id integer DEFAULT nextval('public.film_film_id_seq'::regclass) NOT NULL PRIMARY KEY,
    title character varying(255) NOT NULL,
    description text,
    release_year public.year,
    language_id smallint NOT NULL,
    rental_duration smallint DEFAULT 3 NOT NULL,
    rental_rate numeric(4,2) DEFAULT 4.99 NOT NULL,
    length smallint,
    replacement_cost numeric(5,2) DEFAULT 19.99 NOT NULL,
    rating public.mpaa_rating DEFAULT 'G'::public.mpaa_rating,
    last_update timestamp without time zone DEFAULT now() NOT NULL,
    special_features text[],
    fulltext tsvector NOT NULL,
    FOREIGN KEY (language_id) REFERENCES public.language(language_id) ON UPDATE CASCADE ON DELETE RESTRICT
);

ALTER TABLE public.film OWNER TO postgres;
CREATE INDEX film_fulltext_idx ON public.film USING gist (fulltext);
CREATE INDEX idx_fk_language_id ON public.film USING btree (language_id);
CREATE INDEX idx_title ON public.film USING btree (title);
CREATE TRIGGER film_fulltext_trigger BEFORE INSERT OR UPDATE ON public.film FOR EACH ROW EXECUTE FUNCTION tsvector_update_trigger('fulltext', 'pg_catalog.english', 'title', 'description');
CREATE TRIGGER last_updated BEFORE UPDATE ON public.film FOR EACH ROW EXECUTE FUNCTION public.last_updated();


vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$
```

```console
vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$ pgroutiner --ddl film_in_stock
--
-- Function: public.film_in_stock
--
CREATE FUNCTION public.film_in_stock(
    p_film_id integer,
    p_store_id integer,
    OUT p_film_count integer
)
RETURNS SETOF integer
LANGUAGE sql
AS $_$
     SELECT inventory_id
     FROM inventory
     WHERE film_id = $1
     AND store_id = $2
     AND inventory_in_stock(inventory_id);
$_$;

ALTER FUNCTION public.film_in_stock(p_film_id integer, p_store_id integer, OUT p_film_count integer) OWNER TO postgres;


vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$
```

- Note: to dump multiple definitions use semicolon-separated values:

```console
vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$ pgroutiner --ddl "actor;film_in_stock"
--
-- Table: public.actor
--
CREATE TABLE public.actor (
    actor_id integer DEFAULT nextval('public.actor_actor_id_seq'::regclass) NOT NULL PRIMARY KEY,
    first_name character varying(45) NOT NULL,
    last_name character varying(45) NOT NULL,
    last_update timestamp without time zone DEFAULT now() NOT NULL
);

ALTER TABLE public.actor OWNER TO postgres;
CREATE INDEX idx_actor_last_name ON public.actor USING btree (last_name);
CREATE TRIGGER last_updated BEFORE UPDATE ON public.actor FOR EACH ROW EXECUTE FUNCTION public.last_updated();


--
-- Function: public.film_in_stock
--
CREATE FUNCTION public.film_in_stock(
    p_film_id integer,
    p_store_id integer,
    OUT p_film_count integer
)
RETURNS SETOF integer
LANGUAGE sql
AS $_$
     SELECT inventory_id
     FROM inventory
     WHERE film_id = $1
     AND store_id = $2
     AND inventory_in_stock(inventory_id);
$_$;

ALTER FUNCTION public.film_in_stock(p_film_id integer, p_store_id integer, OUT p_film_count integer) OWNER TO postgres;


vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$
```

### Search definitions by search expression

- Description: `s --s --search to search object schema definitions and dump highlighted results to the console.`

- Searches entire definitions and dumps to console entire definitions containing the expression.

- Note: search expression match is highlighted.

```console
vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$ pgroutiner --search "select public.group_concat"
--
-- View: public.actor_info
--
CREATE VIEW public.actor_info AS
 SELECT a.actor_id,
    a.first_name,
    a.last_name,
    public.group_concat(DISTINCT (((c.name)::text || ': '::text) || ( SELECT public.group_concat((f.title)::text) AS group_concat
           FROM ((public.film f
             JOIN public.film_category fc_1 ON ((f.film_id = fc_1.film_id)))
             JOIN public.film_actor fa_1 ON ((f.film_id = fa_1.film_id)))
          WHERE ((fc_1.category_id = c.category_id) AND (fa_1.actor_id = a.actor_id))
          GROUP BY fa_1.actor_id))) AS film_info
   FROM (((public.actor a
     LEFT JOIN public.film_actor fa ON ((a.actor_id = fa.actor_id)))
     LEFT JOIN public.film_category fc ON ((fa.film_id = fc.film_id)))
     LEFT JOIN public.category c ON ((fc.category_id = c.category_id)))
  GROUP BY a.actor_id, a.first_name, a.last_name;

ALTER TABLE public.actor_info OWNER TO postgres;


vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$
```

```console
vbilopav@DESKTOP-O3A6QK2:~/dev/dvdrental$ pgroutiner --search smallint
--
-- Table: public.store
--
CREATE TABLE public.store (
    store_id integer DEFAULT nextval('public.store_store_id_seq'::regclass) NOT NULL PRIMARY KEY,
    manager_staff_id smallint NOT NULL,
    address_id smallint NOT NULL,
    last_update timestamp without time zone DEFAULT now() NOT NULL,
    FOREIGN KEY (address_id) REFERENCES public.address(address_id) ON UPDATE CASCADE ON DELETE RESTRICT,
    FOREIGN KEY (manager_staff_id) REFERENCES public.staff(staff_id) ON UPDATE CASCADE ON DELETE RESTRICT
);

ALTER TABLE public.store OWNER TO postgres;
CREATE UNIQUE INDEX idx_unq_manager_staff_id ON public.store USING btree (manager_staff_id);
CREATE TRIGGER last_updated BEFORE UPDATE ON public.store FOR EACH ROW EXECUTE FUNCTION public.last_updated();


--
-- Table: public.address
--
CREATE TABLE public.address (
    address_id integer DEFAULT nextval('public.address_address_id_seq'::regclass) NOT NULL PRIMARY KEY,
    address character varying(50) NOT NULL,
    address2 character varying(50),
    district character varying(20) NOT NULL,
    city_id smallint NOT NULL,
    postal_code character varying(10),
    phone character varying(20) NOT NULL,
    last_update timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT fk_address_city FOREIGN KEY (city_id) REFERENCES public.city(city_id)
);

...
```

### Dump data from tables or queries as inserts

### Backup and restore

### Open PSQL tool

### Execute PSQL commands and scripts

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

