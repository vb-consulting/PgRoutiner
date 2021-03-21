# VERSION HISTORY

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
