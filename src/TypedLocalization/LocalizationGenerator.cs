using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace TypedLocalization
{
    [Generator]
    public class LocalizationGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var resxFiles = context.AdditionalFiles
                .Where(at => at.Path.EndsWith(".resx")
                    && context.AnalyzerConfigOptions.GetOptions(at).TryGetValue("build_metadata.AdditionalFiles.SourceItemType", out var itemType)
                    && itemType == "EmbeddedResource");
            if (!resxFiles.Any())
                return;

            var baseFile = resxFiles.OrderBy(f => f.Path.Length).First();
            context
                .AnalyzerConfigOptions
                .GetOptions(baseFile)
                .TryGetValue("build_metadata.AdditionalFiles.ManifestResourceName", out var manifestResourceName);

            var nameParts = manifestResourceName.Split('.').ToList();
            var resourceName = nameParts.Last();
            var localizerName = resourceName.Replace("Localizations", "Localizer");
            nameParts.RemoveAt(nameParts.Count - 1);
            var nameSpace = string.Join(".", nameParts);

            var entries = LoadDataNodes(baseFile.Path);
            var lastEntry = entries.LastOrDefault();

            var sb = new StringBuilder();
            sb.Append($@"
using System;
using System.ComponentModel; 
using System.Globalization;

using Microsoft.Extensions.Localization;

using TypedLocalization.Abstractions;

namespace {nameSpace}
{{
    public class {resourceName}
    {{
    }}

    public interface I{localizerName} : ILocalizer
    {{
        public static class Keys
        {{");
            // localization keys
            foreach (var entry in entries)
            {
                sb.Append($@"
            public const string {entry.Name} = ""{entry.Name}"";");
                if (entry != lastEntry)
                    sb.Append("\n");
            }

            sb.Append($@"
        }}
");
            // interface properties and methods
            foreach (var entry in entries)
            {
                sb.Append($@"
        /// <summary>
        /// {entry.Value}
        /// </summary>");
                foreach (var param in entry.Parameters)
                {
                    sb.Append($@"
        /// <param name=""{param.Key}"">{param.Key}: {param.Value}</param>");
                }

                if (entry.HasParameters)
                {
                    sb.Append($@"
        string Get{entry.Name}({entry.ToMethodParameters()});");
                }
                else
                {
                    sb.Append($@"
        string {entry.Name} {{ get; }}");
                }

                if (entry != lastEntry)
                    sb.Append("\n");
            }

            sb.Append($@"
    }}

    public class {localizerName} : LocalizerBase, I{localizerName}
    {{
        public {localizerName}(IStringLocalizer<{resourceName}> localizer)
            : base(localizer)
        {{
        }}
");
            // implementations of properties and methods
            foreach (var entry in entries)
            {
                sb.Append($@"
        /// <summary>
        /// {entry.Value}
        /// </summary>");
                foreach (var param in entry.Parameters)
                {
                    sb.Append($@"
        /// <param name=""{param.Key}"">{param.Key}: {param.Value}</param>");
                }

                if (entry.HasParameters)
                {
                    sb.Append($@"
        public string Get{entry.Name}({entry.ToMethodParameters()}) => GetResourceString(I{localizerName}.Keys.{entry.Name}, {entry.ToGetResourceStringParameter()});");
                }
                else
                {
                    sb.Append($@"
        public string {entry.Name} => GetResourceString(I{localizerName}.Keys.{entry.Name});");
                }

                if (entry != lastEntry)
                    sb.Append("\n");
            }

            sb.Append($@"
    }}
}}
");
            context.AddSource(localizerName, SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        private List<LocalizationEntry> LoadDataNodes(string filePath)
        {
            return XDocument.Load(filePath)
                .Root
                .Elements("data")
                .Select(node => new LocalizationEntry(
                    node.Attribute("name").Value,
                    node.Element("value").Value,
                    node.Element("comment")?.Value))
                .ToList();
        }
    }
}
