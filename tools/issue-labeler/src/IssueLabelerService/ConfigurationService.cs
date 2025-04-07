using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace IssueLabelerService
{
    public class ConfigurationService
    {
        private IConfigurationRoot  _config;

        public ConfigurationService(IConfigurationRoot config)
        {
            _config = config;
        }

        public IConfigurationRoot GetConfiguration(string repository)
        {
            var defaultSection = _config.GetSection("defaults");
            var repoSection = _config.GetSection(repository);

            var configurationBuilder = new ConfigurationBuilder();

            // Add the default section first
            configurationBuilder.AddConfiguration(defaultSection);

            // Check if the repository section exists and is not empty
            if (repoSection.Exists() && repoSection.GetChildren().Any())
            {
                // Add the repository section, which will override the default section where keys overlap
                configurationBuilder.AddConfiguration(repoSection);
            }

            var mergedConfig = configurationBuilder.Build();

            // Print the merged configuration for testing
            foreach (var kvp in mergedConfig.AsEnumerable())
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }

            return mergedConfig;
        }
    }
}
