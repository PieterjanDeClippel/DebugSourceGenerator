using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Example.SourceGenerators;

[Generator]
public sealed class JsonSourceGenerator : ISourceGenerator
{
    static string[] excludedFolders = new string[] { "/bin", "/obj", "/Properties" };

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var globalOptions = context.AnalyzerConfigOptions.GlobalOptions;

        globalOptions.TryGetValue("build_property.projectdir", out var projectDirectory);
        globalOptions.TryGetValue("build_property.rootnamespace", out var rootNamespace);
        projectDirectory = projectDirectory.TrimEnd('/', '\\');

#pragma warning disable RS1035
        var allJsonFiles = Directory.GetFiles(projectDirectory, "*.json", SearchOption.AllDirectories);

        var mustNotStartWith = excludedFolders
            .Select(folder => folder.Replace('/', Path.DirectorySeparatorChar))
            .Select(folder => Path.Combine(projectDirectory, folder.TrimStart(Path.DirectorySeparatorChar)));

        allJsonFiles = allJsonFiles
            .Where(file => !mustNotStartWith.Any(folder => file.StartsWith(folder)))
            .ToArray();

        var rgxAllowedClassCharacters = new Regex(@"[^A-Za-z0-9]");

        foreach (var file in allJsonFiles)
        {
            var shortFile = Path.GetFileNameWithoutExtension(file);
            var className = rgxAllowedClassCharacters.Replace(shortFile, "_");
            // ClassName cannot start with numbers
            if (Regex.IsMatch(className, @"^[0-9]"))
            {
                className = '_' + className;
            }

            var extensionToTrim = Path.GetExtension(file);
            var relativePath = file.Substring(projectDirectory.Length + 1).Trim('/', '\\');
            var namespaceName = Path.GetDirectoryName(relativePath).Replace(Path.DirectorySeparatorChar, '.');

            var source = new StringBuilder();
            source.AppendLine($"namespace {rootNamespace}.{namespaceName}.Json");
            source.AppendLine("{");

            //    var text = File.ReadAllText(file);
            //    var json = System.Text.Json.JsonDocument.Parse(text);

            //    //source.AppendLine($"    public static class {className}");
            //    //source.AppendLine("    {");
            //    //ProcessJsonElement(json.RootElement);
            //    //source.AppendLine("    }");
            source.AppendLine("}");

            //    //var result = ProcessJsonElement(json.RootElement, className, true);

            context.AddSource($"{shortFile}.g.cs", source.ToString());
        }
#pragma warning restore RS1035
    }

    //private StringBuilder ProcessJsonElement(System.Text.Json.JsonElement element, string className, bool isRoot = false)
    //{
    //    var classTypeBuilder = new StringBuilder();
    //    var propertTypeBuilder = new StringBuilder();

    //    switch (element.ValueKind)
    //    {
    //        case System.Text.Json.JsonValueKind.Object:
    //            // Generate class
    //            classTypeBuilder.Append("    public");
    //            classTypeBuilder.Append(isRoot ? " static" : string.Empty);
    //            classTypeBuilder.Append($" class {className}");
    //            classTypeBuilder.AppendLine();
    //            classTypeBuilder.AppendLine("{");

    //            foreach (var property in element.EnumerateObject())
    //            {
    //                var propertyClassName = $"{className}_{property.Name}";

    //                //classTypeBuilder.AppendLine("    {");
    //                //var props = ProcessJsonElement(property.Value, propertyClassName);
    //                //classTypeBuilder.Append(props.propertyDefinitions);
    //                //classTypeBuilder.AppendLine("    }");
    //                //classTypeBuilder.AppendLine("}");

    //                //classTypeBuilder.Append(props.classDefinitions);

    //                propertTypeBuilder.Append(ProcessJsonElement(property.Value, propertyClassName));
    //                propertTypeBuilder.AppendLine();

    //                // Generate property
    //                classTypeBuilder.AppendLine($"public {propertyClassName} {property.Name} {{ get; set; }}");
    //            }
    //            return (classTypeBuilder);
    //        case System.Text.Json.JsonValueKind.Array:
    //            throw new NotImplementedException();
    //        case System.Text.Json.JsonValueKind.Undefined:
    //        case System.Text.Json.JsonValueKind.String:
    //        case System.Text.Json.JsonValueKind.Number:
    //        case System.Text.Json.JsonValueKind.True:
    //        case System.Text.Json.JsonValueKind.False:
    //        case System.Text.Json.JsonValueKind.Null:
    //        default:
    //            throw new NotImplementedException();
    //    }
    //}
}
