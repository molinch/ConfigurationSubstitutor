using System;
using Microsoft.Extensions.Configuration;

namespace ConfigurationSubstitution
{
    public static class IConfigurationBuilderExtensions
    {
        [Obsolete]
        public static IConfigurationBuilder EnableSubstitutions(this IConfigurationBuilder builder, bool exceptionOnMissingVariables)
        {
            return EnableSubstitutions(builder, new ConfigurationSubstitutor(exceptionOnMissingVariables));
        }

        public static IConfigurationBuilder EnableSubstitutions(this IConfigurationBuilder builder, UnresolvedVariableBehaviour unresolvedVariableBehaviour = UnresolvedVariableBehaviour.Throw)
        {
            return EnableSubstitutions(builder, new ConfigurationSubstitutor(unresolvedVariableBehaviour));
        }

        [Obsolete]
        public static IConfigurationBuilder EnableSubstitutions(this IConfigurationBuilder builder, string substitutableStartsWith, string substitutableEndsWith, bool exceptionOnMissingVariables)
        {
            return EnableSubstitutions(builder, new ConfigurationSubstitutor(substitutableStartsWith, substitutableEndsWith, exceptionOnMissingVariables));
        }

        public static IConfigurationBuilder EnableSubstitutions(this IConfigurationBuilder builder, string substitutableStartsWith, string substitutableEndsWith, UnresolvedVariableBehaviour unresolvedVariableBehaviour = UnresolvedVariableBehaviour.Throw)
        {
            return EnableSubstitutions(builder, new ConfigurationSubstitutor(substitutableStartsWith, substitutableEndsWith, unresolvedVariableBehaviour));
        }

        [Obsolete]
        public static IConfigurationBuilder EnableSubstitutionsWithDelimitedFallbackDefaults(this IConfigurationBuilder builder, string substitutableStartsWith, string substitutableEndsWith, string fallbackDefaultValueDelimiter, bool exceptionOnMissingVariables)
        {
            return EnableSubstitutions(builder, new ConfigurationSubstitutor(substitutableStartsWith, substitutableEndsWith, exceptionOnMissingVariables, fallbackDefaultValueDelimiter));
        }

        public static IConfigurationBuilder EnableSubstitutionsWithDelimitedFallbackDefaults(this IConfigurationBuilder builder, string substitutableStartsWith, string substitutableEndsWith, string fallbackDefaultValueDelimiter, UnresolvedVariableBehaviour unresolvedVariableBehaviour = UnresolvedVariableBehaviour.Throw)
        {
            return EnableSubstitutions(builder, new ConfigurationSubstitutor(substitutableStartsWith, substitutableEndsWith, unresolvedVariableBehaviour, fallbackDefaultValueDelimiter));
        }

        private static IConfigurationBuilder EnableSubstitutions(this IConfigurationBuilder builder, ConfigurationSubstitutor substitutor)
        {
            return builder.Add(new ChainedSubstitutedConfigurationSource(substitutor, builder.Build()));
        }
    }
}