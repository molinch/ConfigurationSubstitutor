using Microsoft.Extensions.Configuration;

namespace ConfigurationSubstitution
{
    public static class IConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder EnableSubstitutions(this IConfigurationBuilder builder)
        {
            return EnableSubstitutions(builder, new ConfigurationSubstitutor());
        }

        public static IConfigurationBuilder EnableSubstitutions(this IConfigurationBuilder builder, string substitutableStartsWith, string substitutableEndsWith)
        {
            return EnableSubstitutions(builder, new ConfigurationSubstitutor(substitutableStartsWith, substitutableEndsWith));
        }

        private static IConfigurationBuilder EnableSubstitutions(this IConfigurationBuilder builder, ConfigurationSubstitutor substitutor)
        {
            return builder.Add(new ChainedSubstitutedConfigurationSource(substitutor, builder.Build()));
        }
    }
}