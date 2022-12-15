using PgRoutiner.Builder.CodeBuilders.Models;

namespace PgRoutiner.Builder.CodeBuilders;

public class CodeResult
{
    public Code Code { get; set; }
    public string Name { get; set; }
    public Module Module { get; set; }
    public string Schema { get; set; }
    public string ForName { get; set; }
    public string NameSuffix { get; set; }
}

public abstract class CodeBuilder
{
    protected readonly NpgsqlConnection connection;
    protected readonly Current settings;
    protected readonly CodeSettings codeSettings;

    private static HashSet<string> UserDefinedModels { get; } = new();
    private static HashSet<string> DirsEmptied { get; } = new();

    public CodeBuilder(NpgsqlConnection connection, Current settings, CodeSettings codeSettings)
    {
        this.connection = connection;
        this.settings = settings;
        this.codeSettings = codeSettings;
    }

    public void Build()
    {
        if (codeSettings.OutputDir == null)
        {
            return;
        }

        var baseOutputDir = GetOutputDir();
        var baseModelDir = GetModelDir();

        foreach (var codeResult in GetCodes())
        {
            if (codeResult.Code == null)
            {
                continue;
            }
            var fileName = GetFileNames(codeResult, baseOutputDir, codeSettings.EmptyOutputDir);

            var exists = File.Exists(fileName.FullFileName);

            if (exists && codeSettings.Overwrite == false)
            {
                Writer.DumpFormat("File {0} exists, overwrite is set to false, skipping ...", fileName.FullFileName);
                continue;
            }
            if (exists && settings.SkipIfExists != null && (
                settings.SkipIfExists.Contains(codeResult.Name) ||
                settings.SkipIfExists.Contains(fileName.ShortFilename) ||
                settings.SkipIfExists.Contains(fileName.Relative))
                )
            {
                Writer.DumpFormat("Skipping {0}, already exists ... ", fileName.Relative);
                continue;
            }
            if (exists && codeSettings.AskOverwrite &&
                Program.Ask($"File {fileName.Relative} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
            {
                Writer.DumpFormat("Skipping {0} ... ", fileName.Relative);
                continue;
            }

            var models = codeResult.Code.Models.Values.ToArray();
            foreach (var (modelName, modelContent) in codeResult.Code.Models)
            {
                if (modelName == null)
                {
                    continue;
                }

                if (baseModelDir == null && !codeResult.Code.UserDefinedModels.Contains(modelName))
                {
                    codeResult.Module.AddItems(models);
                }
                else
                {
                    var modelModule = new Module(settings);
                    modelModule.AddNamespace(GetModelNamespace(codeResult));
                    modelModule.AddItems(modelContent);
                    if (settings.ModelCustomNamespace != null)
                    {
                        modelModule.Namespace = settings.ModelCustomNamespace;
                    }
                    if (modelModule.Namespace != codeResult.Module.Namespace)
                    {
                        codeResult.Module.AddUsing(modelModule.Namespace);
                        codeResult.Code.ModuleNamespace = modelModule.Namespace;
                    }

                    if (UserDefinedModels.Contains(modelName))
                    {
                        continue;
                    }

                    var modelFileName = GetFileNames(modelName, baseModelDir ?? baseOutputDir, schema: codeResult.Schema, empty: settings.EmptyModelDir);
                    var modelExists = File.Exists(modelFileName.FullFileName);

                    if (modelExists && codeSettings.Overwrite == false)
                    {
                        Writer.DumpFormat("File {0} exists, overwrite is set to false, skipping ...", modelFileName.Relative);
                        continue;
                    }
                    if (modelExists && settings.SkipIfExists != null && (
                        settings.SkipIfExists.Contains(modelName) ||
                        settings.SkipIfExists.Contains(modelFileName.ShortFilename) ||
                        settings.SkipIfExists.Contains(modelFileName.Relative))
                        )
                    {
                        Writer.DumpFormat("Skipping {0}, already exists ...", modelFileName.Relative);
                        continue;
                    }
                    if (modelExists && settings.AskOverwrite &&
                        Program.Ask($"File {modelFileName.Relative} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
                    {
                        Writer.DumpFormat("Skipping {0} ...", modelFileName.Relative);
                        continue;
                    }

                    Writer.DumpRelativePath("Creating file: {0} ...", modelFileName.FullFileName);
                    Writer.WriteFile(modelFileName.FullFileName, modelModule.ToString());
                    modelModule.Flush();

                    if (codeResult.Code.UserDefinedModels.Contains(modelName))
                    {
                        UserDefinedModels.Add(modelName);
                    }
                }
            }

            codeResult.Module.AddItems(codeResult.Code.Class);
            Writer.DumpRelativePath("Creating file: {0} ...", fileName.FullFileName);
            Writer.WriteFile(fileName.FullFileName, codeResult.Module.ToString());
            codeResult.Module.Flush();
        }
    }

    public List<ExtensionMethods> GetMethods()
    {
        var outputDir = GetOutputDir();
        var extensions = new List<ExtensionMethods>();

        foreach (var codeResult in GetCodes())
        {
            if (codeResult.Code == null)
            {
                continue;
            }
            var fileName = GetFileNames(codeResult, outputDir, false);
            if (codeResult.Code.Methods.Count > 0 && File.Exists(fileName.FullFileName))
            {
                string modelNamespace = null;
                if (codeResult.Code.Models.Any())
                {
                    var modelModule = new Module(settings);
                    modelModule.AddNamespace(GetModelNamespace(codeResult));

                    if (settings.ModelCustomNamespace != null)
                    {
                        modelModule.Namespace = settings.ModelCustomNamespace;
                    }
                    if (modelModule.Namespace != codeResult.Module.Namespace)
                    {
                        modelNamespace = modelModule.Namespace;
                    }


                }
                extensions.Add(new ExtensionMethods
                {
                    Methods = codeResult.Code.Methods,
                    Namespace = codeResult.Module.Namespace,
                    Name = codeResult.Code.Methods.First().Name,
                    ModelNamespace = modelNamespace,
                    Schema = codeResult.Schema
                });
            }
        }
        return extensions;
    }

    protected abstract IEnumerable<CodeResult> GetCodes();

    protected (string ShortFilename, string FullFileName, string Relative) GetFileNames(string name, string outputDir,
        string schema = null, bool empty = false)
    {
        var dir = schema == null ? outputDir : string.Format(outputDir, schema == "public" ? "" : schema.ToUpperCamelCase())
            .Replace("//", "/")
            .Replace("\\\\", "\\");

        if (empty)
        {
            EmptyDir(dir);
        }

        var shortFilename = string.Concat(name.ToUpperCamelCase(), ".cs");
        var fullFileName = Path.GetFullPath(Path.Join(dir, shortFilename));
        var relative = fullFileName.GetRelativePath();

        return (shortFilename, fullFileName, relative);
    }

    protected (string ShortFilename, string FullFileName, string Relative) GetFileNames(CodeResult code, string outputDir, bool empty = false)
    {
        var name = code.ForName ?? code.Name;
        return GetFileNames(code.NameSuffix == null ? name : $"{name}_{code.NameSuffix}", outputDir, code.Schema, empty);
    }

    private string GetModelNamespace(CodeResult codeResult)
    {
        return string.Format(
            settings.ModelDir ?? codeSettings.OutputDir,
            codeResult.Schema == "public" ? "" : codeResult.Schema.ToUpperCamelCase()).PathToNamespace().Replace("..", ".");
    }

    private string GetOutputDir()
    {
        return Path.Combine(Program.CurrentDir, codeSettings.OutputDir);
    }

    private string GetModelDir()
    {
        var dir = settings.ModelDir;
        if (dir != null)
        {
            dir = Path.Combine(Program.CurrentDir, dir);
        }
        return dir;
    }

    private void EmptyDir(string dir)
    {
        if (dir.PathEquals(Program.CurrentDir))
        {
            Program.WriteLine(ConsoleColor.Yellow, "",
                $"WARNING: Cannot delete output dir that is project root. Combining RoutinesDeleteOutputDir or CrudOutputDir with CrudDeleteOutputDir or RoutinesDeleteOutputDir is not allowed. Don't ever try to delete your project root. Not a good idea. Ignoring these settings for now...");
        }
        else
        {
            if (!DirsEmptied.Contains(dir))
            {
                if (Directory.Exists(dir))
                {
                    DeleteDirFiles(dir);
                }
                else
                {
                    Directory.CreateDirectory(dir);
                }
                DirsEmptied.Add(dir);
            }
        }
    }

    private static void DeleteDirFiles(string dir)
    {
        Writer.DumpRelativePath("Emptying dir of cs files: {0} ...", dir);
        if (Directory.GetFiles(dir, "*.cs").Length > 0)
        {
            foreach (FileInfo fi in new DirectoryInfo(dir).GetFiles("*.cs"))
            {
                fi.Delete();
            }
        }
    }
}
