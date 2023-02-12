using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Example.SourceGenerators
{
    [Generator]
    public sealed class JsonSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var globalOptions = context.AnalyzerConfigOptions.GlobalOptions;

            globalOptions.TryGetValue("build_property.projectdir", out var projectDirectory);
            globalOptions.TryGetValue("build_property.rootnamespace", out var rootNamespace);

            var allJsonFiles = Directory.GetFiles(projectDirectory, "*.json", SearchOption.AllDirectories);
            var rgxAllowedClassCharacters = new Regex(@"[A-Za-z0-9]");

            var source = new StringBuilder();
            source.AppendLine($"namespace {rootNamespace}.Json");
            source.AppendLine("{");
            foreach (var file in allJsonFiles)
            {
                var filteredName = rgxAllowedClassCharacters.Replace(file, "_");
                source.AppendLine($"    public static class {filteredName}");
                source.AppendLine("    {");
                source.AppendLine("    }");
            }
            source.AppendLine("}");
        }
    }
}
