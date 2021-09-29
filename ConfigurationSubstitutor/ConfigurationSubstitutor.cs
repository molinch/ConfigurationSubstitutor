using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConfigurationSubstitution
{
    public class ConfigurationSubstitutor
    {
        // A shared thread static to avoid allocation on each request.
        [ThreadStatic]
        private static HashSet<string> inprogressCache;

        private readonly string _startsWith;
        private readonly string _endsWith;
        private Regex _findSubstitutions;
        private readonly bool _exceptionOnMissingVariables;

        public ConfigurationSubstitutor(bool exceptionOnMissingVariables = true) : this("{", "}", exceptionOnMissingVariables)
        {
        }

        public ConfigurationSubstitutor(string substitutableStartsWith, string substitutableEndsWith, bool exceptionOnMissingVariables = true)
        {
            _startsWith = substitutableStartsWith;
            _endsWith = substitutableEndsWith;
            var escapedStart = Regex.Escape(_startsWith);
            var escapedEnd = Regex.Escape(_endsWith);
            _findSubstitutions = new Regex("(?<=" + escapedStart + ")(.*?)(?=" + escapedEnd + ")",
                RegexOptions.Compiled);
            _exceptionOnMissingVariables = exceptionOnMissingVariables;
        }

        public string GetSubstituted(IConfiguration configuration, string key)
        {
            if (inprogressCache == null)
            {
                inprogressCache = new HashSet<string>();
            }

            inprogressCache.Clear();
            return GetSubstituted(configuration, key, inprogressCache);
        }

        private string GetSubstituted(IConfiguration configuration, string key, HashSet<string> inprogress)
        {
            var value = configuration[key];
            if (value == null) return value;

            return ApplySubstitution(configuration, value, inprogress);
        }

        private string ApplySubstitution(IConfiguration configuration, string value, HashSet<string> inprogress)
        {
            if (!inprogress.Add(value))
            {
                throw new RecursiveConfigVariableException(value);
            }

            var captures = _findSubstitutions.Matches(value).Cast<Match>().SelectMany(m => m.Captures.Cast<Capture>());
            foreach (var capture in captures)
            {
                var substitutedValue = this.GetSubstituted(configuration, capture.Value, inprogress);

                if (substitutedValue == null && _exceptionOnMissingVariables)
                {
                    throw new UndefinedConfigVariableException($"{_startsWith}{capture.Value}{_endsWith}");
                }

                value = value.Replace(_startsWith + capture.Value + _endsWith, substitutedValue);
            }

            inprogress.Remove(value);

            return value;
        }
    }
}
