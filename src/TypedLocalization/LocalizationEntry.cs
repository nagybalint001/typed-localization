using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TypedLocalization
{
    internal class LocalizationEntry
    {
        public string Name { get; }

        public string Value { get; }

        /// <summary>
        /// key: name of the param
        /// value: type of the param
        /// </summary>
        public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();

        public bool HasParameters => Parameters.Count > 0;

        public LocalizationEntry(string name, string value, string parameters)
        {
            Name = name;
            Value = value;

            InitializeParameters(parameters);
        }

        public string ToMethodParameters()
        {
            return string.Join(", ", Parameters.Select(x => $"{x.Value} {x.Key}"));
        }

        public string ToGetResourceStringParameter()
        {
            return string.Join(", ", Parameters.Select(x => x.Value == "string"
                ? $"(Token: \"{{{x.Key}}}\", Value: {x.Key})"
                : $"(Token: \"{{{x.Key}}}\", Value: {x.Key}.ToString())"));
        }

        private void InitializeParameters(string parameters)
        {
            if (string.IsNullOrEmpty(parameters))
                return;

            // param string: {name|type} or {name}
            var paramMatches = Regex.Matches(parameters, "{.*?}", RegexOptions.Singleline);
            foreach (Match paramMatch in paramMatches)
            {
                // param: name|type or name
                var param = paramMatch.Value.TrimStart('{').TrimEnd('}');
                if (!param.Contains("|"))
                {
                    Parameters.Add(param, "string");
                }
                else
                {
                    var paramAndType = param.Split('|');
                    Parameters.Add(paramAndType[0], paramAndType[1]);
                }
            }
        }
    }
}
