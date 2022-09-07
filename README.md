# PgRoutiner - A Different Kind of Object-relational Mapping Tool for .NET projects and PostgreSQL

  - [Installation](#installation)
  - [Quick Start](#quick-start)
  
## Installation

```
$ dotnet tool install --global dotnet-pgroutiner
Tool 'dotnet-pgroutiner' (version '3.16.2') was successfully installed.
```

To update:

```
$ dotnet tool update --global dotnet-pgroutiner
Tool 'dotnet-pgroutiner' was successfully updated from version '3.16.0' to version '3.16.2'.
```

## Quick Start

1) Install `pgroutiner`
2) Open the terminal in your .NET project configured to use the PostgreSQL database.
3) Type `pgroutiner --list`

If your connection string is configured for the user with the admin privileges - you will see the **list of all database objects.**

See also

- [PgRoutiner - A Different Kind of Object-relational Mapping Tool for .NET projects and PostgreSQL](#pgroutiner---a-different-kind-of-object-relational-mapping-tool-for-net-projects-and-postgresql)
  - [Installation](#installation)
  - [Quick Start](#quick-start)
  - [Connection Management](#connection-management)
  - [Basic configuration](#basic-configuration)
  - [Create the default configuration file](#create-the-default-configuration-file)
  - [List database objects](#list-database-objects)
  - [Objects definitions (DDL)](#objects-definitions-ddl)
  - [Generate insert statements](#generate-insert-statements)
  - [Executing SQL scripts and PSQL commands](#executing-sql-scripts-and-psql-commands)
  - [Open PSQL command-line tool](#open-psql-command-line-tool)
  - [Dump schema](#dump-schema)
  - [Backup and restore](#backup-and-restore)
  - [Create object tree files](#create-object-tree-files)
  - [Build markdown database dictionary](#build-markdown-database-dictionary)
  - [Database difference script](#database-difference-script)
  - [Code generation](#code-generation)
    - [Routines (functions and procedures) data-access code generation](#routines-functions-and-procedures-data-access-code-generation)
    - [CRUD data-access code generation](#crud-data-access-code-generation)
  - [Troubleshooting](#troubleshooting)
  - [Support](#support)
  - [License](#license)

## Connection Management

- `pgroutiner` is designed to run from the .NET project root, and it will read any available connections from standard configuration files (`appsettings.Development.json`, `appsettings.json`, in that order, or the custom configuration file `appsettings.PgRoutiner.json`).

- It will use the first available connection string from the `ConnectionStrings` section:

appsettings.json
```json
{
  "ConnectionStrings": {
    "TestConnection": "Server=localhost;Db=test;Port=5432;User Id=postgres;Password=postgres;"
  }
}
```

- Note: this is the [Npgsql connection string format](https://www.npgsql.org/doc/connection-string-parameters.html).

- Running simple info command to test the connection:
```
~$ pgroutiner --info
PgRoutiner: 3.16.2.0
Type pgroutiner -h or pgroutiner --help to see help on available commands and settings.
Type pgroutiner -s or pgroutiner --settings to see the currently selected settings.
Issues   https://github.com/vb-consulting/PgRoutiner/issues
Donate   bitcoincash:qp93skpzyxtvw3l3lqqy7egwv8zrszn3wcfygeg0mv   https://www.paypal.com/paypalme/vbsoftware/
Copyright (c) VB Consulting and VB Software 2022.
This program and source code is licensed under the MIT license.
https://github.com/vb-consulting/PgRoutiner/blob/master/LICENSE

Using dir:
 /home/vbilopav

Using configuration files:
 appsettings.json

Using connection TestConnection:
 Host=localhost;Database=pdd;Port=5433;Username=postgres
```

- Or silently test the connection:

```
~$ pgroutiner --info --silent
```

- To specify the connection name, use `-c` or `--connection` parameter:


```
~$ pgroutiner -c testconnection --info

...

Using connection testconnection:
 Host=localhost;Database=pdd;Port=5433;Username=postgres
```

```
~$ pgroutiner --connection testconnection --info
...

Using connection testconnection:
 Host=localhost;Database=pdd;Port=5433;Username=postgres
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
...

Using connection Server=localhost;Db=pdd;Port=5433;User Id=postgres;Password=postgres;:
 Host=localhost;Database=pdd;Port=5433;Username=postgres
```

- Both, command-line and configuration files can take advantage of the PostgreSQL URL format `postgresql://{user}:{password}@{server}:{port}/{database}` - instead of [Npgsql connection string](https://www.npgsql.org/doc/connection-string-parameters.html):


```json
{
  "ConnectionStrings": {
    "TestConnection": "postgresql://postgres:postgres@localhost:5432/test"
  }
}
```
```
~$ pgroutiner --connection "postgresql://postgres:postgres@localhost:5432/test" --info
...

Using connection postgresql://postgres:postgres@localhost:5432/test:
 Host=localhost;Database=pdd;Port=5433;Username=postgres
```

- Every part of the connection (server, port, database, user, and password) can be omitted from the connection string or connection URL and it will be replaced with the following environment variables:

  - `PGHOST` or `PGSERVER` to replace the missing server parameter.
  - `PGPORT` to replace the missing port parameter.
  - `PGDATABASE` or `PGDB` to replace the missing database parameter.
  - `PGHOST` or `PGSERVER` to replace the missing server parameter.
  - `PGUSER` to replace the missing user parameter.
  - `PGPASSWORD` or `PGPASS` to replace the missing password parameter.

## Basic configuration

- By default PgRoutiner will output a lot of information in the console like:

  - Copyright and version info
  - Basic help
  - Current options
  - Current psql/pg_dump/pg_restore commands
  - Configuration files being used
  - Current dir
  - Current database connection

- To silence all of that output that is not explicitly requested - you can include `-silent` or `--silent`:

```
~$ pgroutiner --list --silent

... list output

```

- To **permanently turn this option on, use the following configuration**:

`appsettings.json` or `appsettings.Development.json` or `appsettings.PgRoutiner.json`:
```json
{
  "ConnectionStrings": {
    "TestConnection": "postgresql://postgres:postgres@localhost:5432/test"
  },
  "PgRoutiner": {
    "Silent": true
  }
}
```

- Every command and switch can be set in a configuration like this. 

- The Command line will always override the configuration setting.

- For the boolean values (like this `Silent` for example), the default value is `false`, and you can set it to `true` by simply including that option in console (like `-silent` or `--silent`).

- Fox boolean values `false` or `0` and `true` or `1` are acceptable.

- Any configuration value can be overridden with a command line with the same name, where multiple words are separated by the minus sign `-`. For example `Connection` settings will be `--connection` in console, but `SkipConnectionPrompt` will be `--skip-connection-prompt`.


## Create the default configuration file

- You can create a default configuration file that contains all available options and settings.

- Simply run `pgroutiner` without any command line parameters and if `pgroutiner` configuration is not found you will be prompted with the question:

```
You don't seem to be using any available command-line commands and `pgroutiner` configuration seems to be missing.
Would you like to create a custom settings file "appsettings.PgRoutiner.json" with your current values?
This settings configuration file can be used to change settings for this directory without using a command-line.
Create "appsettings.PgRoutiner.json" in this dir [Y/N]?
y

Settings file appsettings.PgRoutiner.json successfully created!
```

- Note: this `appsettings.PgRoutiner.json` will contain all default configuration values. You can move this section to `appsettings.json` or `appsettings.Development.json`, whichever suits the best.

## List database objects

- Use `-l` or `--list` to list all objects for the connection:

```
~$ pgroutiner -l
SCHEMA public
SCHEMA reporting
EXTENSION plpgsql
EXTENSION pg_trgm
TYPE valid_genders
TABLE public.people
TABLE public.company_reviews
TABLE public.countries
TABLE public.person_roles
TABLE public.users
TABLE public.companies
TABLE public.business_role_types
TABLE public.business_roles
TABLE public.employee_records
TABLE public.employee_status
TABLE public.business_areas
TABLE public.company_areas
FUNCTION reporting.chart_companies_by_country(integer)
FUNCTION reporting.chart_employee_counts_by_area(integer)
FUNCTION reporting.chart_employee_counts_by_year(integer)
```

- To view objects from the specific schema, use `-sch` or `--schema-similar-to` option:

```
~$ pgroutiner --list -sch reporting
SCHEMA reporting
EXTENSION plpgsql
EXTENSION pg_trgm
FUNCTION reporting.chart_companies_by_country(integer)
FUNCTION reporting.chart_employee_counts_by_area(integer)
FUNCTION reporting.chart_employee_counts_by_year(integer)
```

- Option `--schema-similar-to` or `-sch` uses [`similar` expression from PostgreSQL](https://www.postgresql.org/docs/current/functions-matching.html#FUNCTIONS-SIMILARTO-REGEXP) which means that you can use `|` to specify multiple schemas:

```
~$ pgroutiner --list --schema-similar-to "reporting|public"
SCHEMA public
SCHEMA reporting
EXTENSION plpgsql
EXTENSION pg_trgm
TYPE valid_genders
TABLE public.people
TABLE public.company_reviews
TABLE public.countries
TABLE public.person_roles
TABLE public.users
TABLE public.companies
TABLE public.business_role_types
TABLE public.business_roles
TABLE public.employee_records
TABLE public.employee_status
TABLE public.business_areas
TABLE public.company_areas
FUNCTION reporting.chart_companies_by_country(integer)
FUNCTION reporting.chart_employee_counts_by_area(integer)
FUNCTION reporting.chart_employee_counts_by_year(integer)
```

- You can also use `--schema--not-similar-to` to exclude schemas NOT similar to the expression:

```
~$ pgroutiner --list --schema-not-similar-to public
SCHEMA reporting
EXTENSION plpgsql
EXTENSION pg_trgm
FUNCTION reporting.chart_companies_by_country(integer)
FUNCTION reporting.chart_employee_counts_by_area(integer)
FUNCTION reporting.chart_employee_counts_by_year(integer)
```

## Objects definitions (DDL)

- To view object definition use `-def` or `--definition` option:

```
$ pgroutiner -def users
--
-- Table: users
--
CREATE TABLE public.users (
    id bigint NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    email character varying NOT NULL,
    name character varying,
    data json DEFAULT '{}'::json NOT NULL,
    providers character varying[] DEFAULT '{}'::character varying[] NOT NULL,
    timezone character varying NOT NULL,
    culture character varying NOT NULL,
    person_id bigint,
    lockout_end timestamp with time zone,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    CONSTRAINT fk_person_id FOREIGN KEY (person_id) REFERENCES public.people(id) DEFERRABLE
);

ALTER TABLE public.users OWNER TO postgres;
COMMENT ON TABLE public.users IS 'System users. May or may not be a person (in people records).';
COMMENT ON COLUMN public.users.email IS 'lowercased';
COMMENT ON COLUMN public.users.data IS 'json data received from external auth provider';
COMMENT ON COLUMN public.users.providers IS 'list of external auth providers autorized this user';
COMMENT ON COLUMN public.users.timezone IS 'timezone from browser';
COMMENT ON COLUMN public.users.culture IS 'matching culture by browser timezone';
CREATE UNIQUE INDEX idx_users_email ON public.users USING btree (email)
```

- You can view DDL object definition of any type:

```
$ pgroutiner --definition chart_companies_by_country
--
-- Function: chart_companies_by_country
--
CREATE FUNCTION reporting.chart_companies_by_country(_limit integer DEFAULT 10) RETURNS json
    LANGUAGE sql
    AS $$
with cte as (
    select
        b.name, count(*), row_number () over (order by count(*) desc, b.name)
    from
        companies a
        inner join countries b on a.country = b.code
    group by
        b.name
    order by
        count(*) desc, b.name
)
select
    json_build_object(
        'labels', json_agg(sub.name),
        'series', array[
            json_build_object('data', json_agg(coalesce(sub.count, 0)))
        ]
    )
from (
    select name, count, row_number
    from cte
    where row_number < coalesce(_limit, 10)
    union all
    select 'Other' as name, sum(count) as count, 10 as row_number
    from cte
    where row_number >= coalesce(_limit, 10)
    order by row_number
) sub
$$;
ALTER FUNCTION reporting.chart_companies_by_country(_limit integer) OWNER TO postgres;
COMMENT ON FUNCTION reporting.chart_companies_by_country(_limit integer) IS 'Number of companies by country.
Json object where lables are country names and it only have one series with the number of companies for each country.
It show only first 9 conutries and 10th is summed together as other.
- Returns JSON schema: `{"labels": [string], "series: [{"data": [number]}]"}`
';
```

```
$ pgroutiner -def pg_trgm
--
-- Extension: pg_trgm
--
CREATE EXTENSION IF NOT EXISTS pg_trgm WITH SCHEMA public;

COMMENT ON EXTENSION pg_trgm IS 'text similarity measurement and index searching based on trigrams';
```

```
$ pgroutiner -def valid_genders
--
-- Type: valid_genders
--
CREATE TYPE public.valid_genders AS ENUM (
    'M',
    'F'
);

ALTER TYPE public.valid_genders OWNER TO postgres;
COMMENT ON TYPE public.valid_genders IS 'There are only two genders.';
```

- If you want to view the DDL definition for multiple objects you can separate names by `;`, and use quotes, for example:

```
$ pgroutiner -def "reporting;valid_genders"
--
-- Schema: reporting
--
CREATE SCHEMA reporting;

ALTER SCHEMA reporting OWNER TO postgres;

--
-- Type: valid_genders
--
CREATE TYPE public.valid_genders AS ENUM (
    'M',
    'F'
);

ALTER TYPE public.valid_genders OWNER TO postgres;
COMMENT ON TYPE public.valid_genders IS 'There are only two genders.';
```

## Generate insert statements

- You can generate insert statements from multiple tables or queries by using `-i`, or `--inserts` option:

```
$ pgroutiner --inserts business_areas
DO $testconnection_data$
BEGIN
-- Data for Name: business_areas; Type: TABLE DATA; Schema: public; Owner: postgres
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (1, 'General', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (2, 'Enterprise', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (3, 'Fintech', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (4, 'Mobility', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (5, 'Insurtech', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (6, 'Big Data', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (7, 'Healthcare', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (8, 'Manufacturing', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (9, 'Hardware', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (10, 'Proptech', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (11, 'AI', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (12, 'Edtech', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (13, 'Consumer', DEFAULT);
END $testconnection_data$
LANGUAGE plpgsql;
```

- To omit transaction wrapper from resulting script include `--data-dump-no-transaction` switch:

```
$ pgroutiner -i business_areas --data-dump-no-transaction
-- Data for Name: business_areas; Type: TABLE DATA; Schema: public; Owner: postgres
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (1, 'General', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (2, 'Enterprise', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (3, 'Fintech', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (4, 'Mobility', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (5, 'Insurtech', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (6, 'Big Data', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (7, 'Healthcare', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (8, 'Manufacturing', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (9, 'Hardware', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (10, 'Proptech', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (11, 'AI', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (12, 'Edtech', DEFAULT);
INSERT INTO public.business_areas OVERRIDING SYSTEM VALUE VALUES (13, 'Consumer', DEFAULT);
```

- To specify query instead of table name:

```
$ pgroutiner -i "select name from business_areas limit 3" --data-dump-no-transaction
-- Data for Name: business_areas; Type: TABLE DATA; Schema: public; Owner: postgres
INSERT INTO public.business_areas VALUES ('General');
INSERT INTO public.business_areas VALUES ('Enterprise');
INSERT INTO public.business_areas VALUES ('Fintech');
```

- To specify multiple queries or tables use the semicolon (`;`) separated list:

```
$ pgroutiner -i "select name from business_areas limit 3; select * from countries limit 3" --data-dump-no-transaction
-- Data for Name: business_areas; Type: TABLE DATA; Schema: public; Owner: postgres
INSERT INTO public.business_areas VALUES ('General');
INSERT INTO public.business_areas VALUES ('Enterprise');
INSERT INTO public.business_areas VALUES ('Fintech');
-- Data for Name: countries; Type: TABLE DATA; Schema: public; Owner: postgres
INSERT INTO public.countries VALUES (474, 'MQ', 'MTQ', 'Martinique', 'martinique', NULL);
INSERT INTO public.countries VALUES (478, 'MR', 'MRT', 'Mauritania', 'mauritania', NULL);
INSERT INTO public.countries VALUES (480, 'MU', 'MUS', 'Mauritius', 'mauritius', NULL);
```

## Executing SQL scripts and PSQL commands

- Use `-x` or `--execute` option to execute the SQL file or PSQL command:

- This will execute the PSQL command and show the results:

```
$ pgroutiner -x "select * from countries limit 3"
 code | iso2 | iso3 |    name    | name_normalized | culture
------+------+------+------------+-----------------+---------
  474 | MQ   | MTQ  | Martinique | martinique      |
  478 | MR   | MRT  | Mauritania | mauritania      |
  480 | MU   | MUS  | Mauritius  | mauritius       |
(3 rows)
```

- If the supplied parameter is an existing file, that file will be executed instead:

test.sql:
```
do 
$$
begin
raise info 'hello world';
end
$$
```
```
$ pgroutiner -x test.sql
psql:/home/vbilopav/pgroutiner-test/test.sql:7: INFO:  hello world
DO
```

or

test.sql:
```
select * from business_areas
```
```
$ pgroutiner -x test.sql
 id |     name      | name_normalized
----+---------------+-----------------
  1 | General       | general
  2 | Enterprise    | enterprise
  3 | Fintech       | fintech
  4 | Mobility      | mobility
  5 | Insurtech     | insurtech
  6 | Big Data      | big data
  7 | Healthcare    | healthcare
  8 | Manufacturing | manufacturing
  9 | Hardware      | hardware
 10 | Proptech      | proptech
 11 | AI            | ai
 12 | Edtech        | edtech
 13 | Consumer      | consumer
(13 rows)
```

- You can also specify a file search mask to execute multiple files:

```
$ pgroutiner -x *.sql

... 
```

- You can use specific PSQL commands to run, for example, to list all tables:

```
$ pgroutiner -x "\dt"
                List of relations
 Schema |        Name         | Type  |  Owner
--------+---------------------+-------+----------
 public | business_areas      | table | postgres
 public | business_role_types | table | postgres
 public | business_roles      | table | postgres
 public | companies           | table | postgres
 public | company_areas       | table | postgres
 public | company_reviews     | table | postgres
 public | countries           | table | postgres
 public | employee_records    | table | postgres
 public | employee_status     | table | postgres
 public | people              | table | postgres
 public | person_roles        | table | postgres
 public | users               | table | postgres
(12 rows)
```

- You can combine multiple commands separated by a semicolon `;`, For example, the following command will set the output format to HTML with `\H` command and then execute the query:

```
$ pgroutiner -x "\H;select * from countries limit 1"
Output format is html.
<table border="1">
  <tr>
    <th align="center">code</th>
    <th align="center">iso2</th>
    <th align="center">iso3</th>
    <th align="center">name</th>
    <th align="center">name_normalized</th>
    <th align="center">culture</th>
  </tr>
  <tr valign="top">
    <td align="right">474</td>
    <td align="left">MQ</td>
    <td align="left">MTQ</td>
    <td align="left">Martinique</td>
    <td align="left">martinique</td>
    <td align="left">&nbsp; </td>
  </tr>
</table>
<p>(1 row)<br />
</p>
```

- You can combine multiple commands and files separated by a semicolon `;`.
  
## Open PSQL command-line tool

```
$ pgroutiner -psql
psql (14.5 (Ubuntu 14.5-1.pgdg20.04+1))
SSL connection (protocol: TLSv1.3, cipher: TLS_AES_256_GCM_SHA384, bits: 256, compression: off)
Type "help" for help.

test=#
```

- Change terminal on windows:

Windows systems will open a new window with a windows terminal and start PSQL by default. To use a different terminal, use `--psql-terminal` option:

```
❯ dotnet run -- -psql --psql-terminal cmd
```

## Dump schema

To dump schema from the current connection use `-sd` or `--schema-dump` option:

```
$ pgroutiner -sd
...
ALTER TABLE ONLY public.users
    ADD CONSTRAINT fk_person_id FOREIGN KEY (person_id) REFERENCES public.people(id) DEFERRABLE;
--
-- Name: person_roles fk_role_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--
ALTER TABLE ONLY public.person_roles
    ADD CONSTRAINT fk_role_id FOREIGN KEY (role_id) REFERENCES public.business_roles(id) DEFERRABLE;
--
-- Name: business_roles fk_type; Type: FK CONSTRAINT; Schema: public; Owner: -
--
ALTER TABLE ONLY public.business_roles
    ADD CONSTRAINT fk_type FOREIGN KEY (type) REFERENCES public.business_role_types(id) DEFERRABLE;
--
-- PostgreSQL database dump complete
--
END $pdd_schema$
LANGUAGE plpgsql;
```

- To export to specific file use `-sdf` or `--schema-dump-file`:

```
$ pgroutiner --schema-dump --schema-dump-file dump.sql
```

- You can also use the file redirect option:

```
$ pgroutiner --schema-dump > dump.sql
```

- Dumping to a file with `--schema-dump-file` has more options, see the configuration file for all available options.

## Backup and restore

- Use `-backup` or `--backup` to backup database.

```
$ pgroutiner -backup ./mybackup
```

- This will create a directory `mybackup` (only if it does not exist) with the backup of your database.

- Backup is in a directory format, with maximum compression, and by using 10 parallel jobs (fastest).

- By default, the object owner is not included in the backup. To include owner, include `--backup-owner`:

```
$ pgroutiner -backup ./mybackup --backup-owner
```

- You can use optional format placeholders to add automatic values to the backup name:
  - `{0}` - current date and time. You can format this by using [a custom format specifier](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings), for example `{0:yyyy-MM-dd}`
  - `{1}` - current connection name

- Use `-restore` or `--restore` to restore the database.

```
$ pgroutiner -restore ./mybackup
```

- By default, database owner is not restored (if present). Use `--restore-owner` to restore owner if present:

```
$ pgroutiner -restore ./mybackup --restore-owner
```

- Restore will read compressed directory format by using 10 parallel jobs (fastest).

## Create object tree files

- `pgroutiner` can create one file for each database object containing object DDL definition, in respective directories (tables, views, functions, etc) - by using `-db` or `--db-objects` switch

When you run `$ pgroutiner -db` or `$ pgroutiner --db-objects`, and not in silent mode, you will see output like this:

```
** DATA OBJECTS SCRIPTS TREE GENERATION **
/usr/lib/postgresql/14/bin/pg_dump -h localhost -p 5433 -U postgres --encoding=UTF8 --schema-only --no-owner --no-acl pdd
Creating dump file Database/TestConnection/Tables/people.sql ...
Creating dump file Database/TestConnection/Tables/company_reviews.sql ...
Creating dump file Database/TestConnection/Tables/countries.sql ...
Creating dump file Database/TestConnection/Tables/person_roles.sql ...
Creating dump file Database/TestConnection/Tables/users.sql ...
Creating dump file Database/TestConnection/Tables/companies.sql ...
Creating dump file Database/TestConnection/Tables/business_role_types.sql ...
Creating dump file Database/TestConnection/Tables/business_roles.sql ...
Creating dump file Database/TestConnection/Tables/employee_records.sql ...
Creating dump file Database/TestConnection/Tables/employee_status.sql ...
Creating dump file Database/TestConnection/Tables/business_areas.sql ...
Creating dump file Database/TestConnection/Tables/company_areas.sql ...
/usr/lib/postgresql/14/bin/pg_dump -h localhost -p 5433 -U postgres --encoding=UTF8 --schema-only --no-owner --no-acl --exclude-table=* pdd
Creating dump file Database/TestConnection/Functions/reporting/reporting.chart_employee_counts_by_area.sql ...
Creating dump file Database/TestConnection/Functions/reporting/reporting.chart_employee_counts_by_year.sql ...
Creating dump file Database/TestConnection/Functions/reporting/reporting.chart_companies_by_country.sql ...
Creating dump file Database/TestConnection/Types/valid_genders.sql ...
Creating dump file Database/TestConnection/Schemas/reporting.sql ...

**** FINISHED ****
```

- Use `-dbd` or ` --db-objects-dir` to set the target root directory name. Use `{0}` format placeholder to put the connection name.

## Build markdown database dictionary

- To create a database dictionary "readme" markdown file use `-md` or `--markdown` command:

```
$ pgroutiner -md

** MARKDOWN (MD) GENERATION **
Creating markdown file Database/TestConnection/README.md ...
```

- See a live example of output here: [PDD.Database dictionary example](https://github.com/vb-consulting/postgresql-driven-development-demo/blob/master/PDD.Database/README.md)

- Default markdown file name is `./Database/{0}/README.md`, where `{0}` placeholder is the connection name. Use `-mdf` or `--md-file` to change this file name.

- Use `-cc` or `--commit-md` to commit edited comments back to the database.

  - You can edit generated markdown in between comments for example:

```
<!-- comment on table "public"."business_areas" is @until-end-tag; -->
Business areas that companies may be invloved.
<!-- end -->
```

- Edit this comment and use `-cc` or `--commit-md` to commit edited comments back to the database.

- Use `--md-export-to-html` to create HTML version as well.

- Use `--md-include-source-links` to add links to the generated object tree files

- Use `--md-include-table-stats` to include statistics for every table.

## Database difference script

- `pgroutiner` can generate a schema difference script between two connections to automatically generate schema migrations.

## Code generation

- `pgroutiner` can generate C# 10 for .NET6 data-access code automatically by using your PostgreSQL connection for:
  - Functions and procedures (routines).
  - CRUD operations of configured tables.
  - All appropriate models.
  - Unit test templates.

- It will look for a .NET project file in the current directory, and, if found, it will add the following Nuget libraries (only if missing):
  - [`Npgsql` - .NET Access to PostgreSQL](https://www.npgsql.org/)
  - [`System.Linq.Async` - Linq over IAsyncEnumerable sequence](https://www.nuget.org/packages/System.Linq.Async)
  - [`Norm.net` - extendible, high-performance micro-ORM ](https://github.com/vb-consulting/Norm.net)

- These references will be required for the generated code, however, they can be skipped with `--skip-update-references` switch.
  
- There is a number of switches and settings that affect the code generation:
  
  - `--namespace` - the name of the namespace for generated code. Default is not set (null) and it will use project default from the project file with respect to the directory. Use this to set a fixed namespace for the generated code.
  - `--use-records` - force generating C# records instead of classes (default) for all generated models.
  - `--use-expression-body` - use expression bodies instead of block bodies (default) for all generated functions.
  - `--use-file-scoped-namespaces` - use file-scoped namespaces (default), instead of block-scoped for all generated source code files
  - `--use-nullable-strings` - use nullable string types `string?` (default), instead of standard strings.
  - `--mapping` - type mapping between PostgreSQL types and .NET. This is a dictionary setting, that can be set either from configuration or command line. To set mapping from the command line use `--mapping:text mystring` to set `text` type to point to `mystring`. Use these settings to change existing mappings or add new ones.
  - `--custom-models` - this a dictionary setting, which is empty by default, where keys are generated model names and values are custom names we wish to override.
  - `--model-dir` - models code output directory name. Default is `./Models/`.
  - `--model-custom-namespace` - the name of the custom namespace for models. Default is not set (null) and it will use project default from the project file with respect to the directory. Use this to set a fixed namespace for the generated models.
  - `--empty-model-dir` - force emptying model directory before files are generated. Set to true to delete all files before generation. The default is false.
  - `--skip-sync-methods` - don't generate synchronous methods and functions. The default is false - always generates synchronous methods and functions.
  - `--skip-async-methods` - don't generate asynchronous methods and functions. The default is false - always generates asynchronous methods and functions.
  - `--source-header-lines` - list of text
  - `--ident`
  - `--return-method`
  - `--method-parameter-names`

### Routines (functions and procedures) data-access code generation

- `pgroutiner` can generate C# 10 for .NET6 data-access code for PostgreSQL routines (functions and procedures) from your connection, along with the appropriate data model.

### CRUD data-access code generation

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

Those paths where PostgreSQL installs binaries by default. 

When PgRoutiner sees the version mismatch it will prompt a warning and fallback to that path with an appropriate version number.

This behavior can be avoided by settings `PgDumpFallback` and `PsqlFallback` settings values respectively.

## Support
 
This is open-source software developed and maintained freely without any compensation whatsoever.
 
If you find it useful please consider rewarding me on my effort by [buying me a beer](https://www.paypal.me/vbsoftware/5)🍻 or [buying me a pizza](https://www.paypal.me/vbsoftware/10)🍕
 
Or if you prefer bitcoin:
bitcoincash:qp93skpzyxtvw3l3lqqy7egwv8zrszn3wcfygeg0mv
 
## License
 
Copyright (c) Vedran Bilopavlović - VB Consulting and VB Software 2020
This source code is licensed under the [MIT license](https://github.com/vb-consulting/PgRoutiner/blob/master/LICENSE).

