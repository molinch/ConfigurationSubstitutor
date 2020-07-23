using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConfigurationSubstitution
{
    public class ConfigurationSubstitutor
    {
        private readonly string _startsWith;
        private readonly string _endsWith;
        private Regex _findSubstitutions;

        public ConfigurationSubstitutor() : this("{", "}")
        {
        }

        public ConfigurationSubstitutor(string substitutableStartsWith, string substitutableEndsWith)
        {
            _startsWith = substitutableStartsWith;
            _endsWith = substitutableEndsWith;
            _findSubstitutions = new Regex(@"(?<=" + Regex.Escape(_startsWith) + @")[^}{]*(?="+ Regex.Escape(_endsWith) + @")", RegexOptions.Compiled);
        }

        public string GetSubstituted(IConfiguration configuration, string key)
        {
            var value = configuration[key];
            if (string.IsNullOrWhiteSpace(value)) return value;

            return ApplySubstitution(configuration, value);
        }

        public string ApplySubstitution(IConfiguration configuration, string value)
        {
            var captures = _findSubstitutions.Matches(value).Cast<Match>().SelectMany(m => m.Captures.Cast<Capture>());
            foreach (var capture in captures)
            {
                value = value.Replace(_startsWith + capture.Value + _endsWith, configuration[capture.Value]);
            }
            return value;
        }
    }
}
