using Microsoft.Extensions.Configuration;

namespace ConfigurationSubstitution
{
    /// <summary>
    /// Represents a chained <see cref="IConfiguration"/> as an <see cref="IConfigurationSource"/>.
    /// </summary>
    public class ChainedSubstitutedConfigurationSource : IConfigurationSource
    {
        private readonly ConfigurationSubstitutor _substitutor;
        private readonly IConfigurationRoot _root;

        public ChainedSubstitutedConfigurationSource(ConfigurationSubstitutor substitutor, IConfigurationRoot root)
        {
            _substitutor = substitutor;
            _root = root;
        }

        /// <summary>
        /// Builds the <see cref="ChainedSubstitutedConfigurationSource"/> using IConfigurationBuilder as the source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="ChainedSubstitutedConfigurationProvider"/></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
            => new ChainedSubstitutedConfigurationProvider(_root, _substitutor);
    }
}