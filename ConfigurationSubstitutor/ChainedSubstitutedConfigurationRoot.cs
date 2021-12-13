using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigurationSubstitution
{
    /// <summary>
    /// Proxy configuration root that filters out <see cref="ChainedSubstitutedConfigurationProvider"/>
    /// from the given providers
    /// </summary>
    internal class ChainedSubstitutedConfigurationRoot : IConfigurationRoot
    {
        private readonly Lazy<ConfigurationRoot> _root;

        public ChainedSubstitutedConfigurationRoot(IConfigurationRoot root)
        {
            _root = new Lazy<ConfigurationRoot>(() =>
            {
                var filteredProviders = root.Providers?
                    .Where(p => p.GetType() != typeof(ChainedSubstitutedConfigurationProvider))
                    .ToList();
                return new ConfigurationRoot(filteredProviders);
            });
        }
        

        public IConfigurationSection GetSection(string key) => _root.Value.GetSection(key);

        public IEnumerable<IConfigurationSection> GetChildren() => _root.Value.GetChildren();

        public IChangeToken GetReloadToken() => _root.Value.GetReloadToken();

        public string this[string key]
        {
            get => _root.Value[key];
            set => _root.Value[key] = value;
        }

        public void Reload() => _root.Value.Reload();

        public IEnumerable<IConfigurationProvider> Providers => _root.Value.Providers;
    }
}