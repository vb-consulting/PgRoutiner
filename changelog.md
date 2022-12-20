# VERSION HISTORY

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

