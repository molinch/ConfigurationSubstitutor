using Microsoft.Extensions.Configuration;

namespace ConfigurationSubstitution
{
    using System;

    public static class IConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder EnableSubstitutions(this IConfigurationBuilder builder, bool exceptionOnMissingVariables = false)
        {
            return EnableSubstitutions(builder, new ConfigurationSubstitutor(exceptionOnMissingVariables));
        }

        public static IConfigurationBuilder EnableSubstitutions(this IConfigurationBuilder builder, string substitutableStartsWith, string substitutableEndsWith, bool exceptionOnMissingVariables = false)
        {
            return EnableSubstitutions(builder, new ConfigurationSubstitutor(substitutableStartsWith, substitutableEndsWith, exceptionOnMissingVariables));
        }

        private static IConfigurationBuilder EnableSubstitutions(this IConfigurationBuilder builder, ConfigurationSubstitutor substitutor)
        {
            return builder.Add(new ChainedSubstitutedConfigurationSource(substitutor, builder.Build()));
        }
    }
}