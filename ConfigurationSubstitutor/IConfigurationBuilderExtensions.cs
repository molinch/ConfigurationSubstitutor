using Microsoft.Extensions.Configuration;

namespace ConfigurationSubstitution
{
    public static class IConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder EnableSubstitutions(this IConfigurationBuilder builder, bool exceptionOnMissingVariables = true)
        {
            return EnableSubstitutions(builder, new ConfigurationSubstitutor(exceptionOnMissingVariables));
        }

        public static IConfigurationBuilder EnableSubstitutions(this IConfigurationBuilder builder, string substitutableStartsWith, string substitutableEndsWith, bool exceptionOnMissingVariables = true)
        {
            return EnableSubstitutions(builder, new ConfigurationSubstitutor(substitutableStartsWith, substitutableEndsWith, exceptionOnMissingVariables));
        }

        private static IConfigurationBuilder EnableSubstitutions(this IConfigurationBuilder builder, ConfigurationSubstitutor substitutor)
        {
            return builder.Add(new ChainedSubstitutedConfigurationSource(substitutor, builder.Build()));
        }

        public static IConfigurationBuilder EnableSubstitutionsWithDelimitedFallbackDefaults(this IConfigurationBuilder builder, string substitutableStartsWith, string substitutableEndsWith, string fallbackDefaultValueDelimiter, bool exceptionOnMissingVariables = true)
        {
            return EnableSubstitutions(builder, new ConfigurationSubstitutor(substitutableStartsWith, substitutableEndsWith, exceptionOnMissingVariables, fallbackDefaultValueDelimiter));
        }
    }
}