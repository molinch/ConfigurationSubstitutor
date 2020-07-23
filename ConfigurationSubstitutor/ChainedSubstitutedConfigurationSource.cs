using Microsoft.Extensions.Configuration;

namespace ConfigurationSubstitution
{
    /// <summary>
    /// Represents a chained <see cref="IConfiguration"/> as an <see cref="IConfigurationSource"/>.
    /// </summary>
    public class ChainedSubstitutedConfigurationSource : IConfigurationSource
    {
        private readonly ConfigurationSubstitutor _substitutor;
        private readonly IConfiguration _configuration;

        public ChainedSubstitutedConfigurationSource(ConfigurationSubstitutor substitutor, IConfiguration configuration)
        {
            _substitutor = substitutor;
            _configuration = configuration;
        }

        /// <summary>
        /// Builds the <see cref="ChainedSubstitutedConfigurationSource"/> using IConfigurationBuilder as the source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="ChainedSubstitutedConfigurationProvider"/></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
            => new ChainedSubstitutedConfigurationProvider(_configuration, _substitutor);
    }
}
