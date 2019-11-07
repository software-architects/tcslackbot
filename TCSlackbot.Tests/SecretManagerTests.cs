using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using TCSlackbot.Logic;
using Xunit;

namespace TCSlackbot.Tests
{
    public class SecretManagerTests
    {
        [Fact]
        public void ValidSecret()
        {
            var dict = new Dictionary<string, string>
            {
                {"key1", "value1"},
                {"key2", "value2"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(dict)
                .Build();

            SecretManager secretManager = new SecretManager(configuration);

            Assert.Equal("value1", secretManager.GetSecret("key1"));
            Assert.Equal("value2", secretManager.GetSecret("key2"));
        }

        [Fact]
        public void InvalidSecret()
        {
            var dict = new Dictionary<string, string>
            {
                {"key1", "value1"},
                {"key2", "value2"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(dict)
                .Build();

            SecretManager secretManager = new SecretManager(configuration);

            Assert.Null(secretManager.GetSecret("wrong_key"));
        }

        [Fact]
        public void ChangedSecret()
        {
            var dict = new Dictionary<string, string>
            {
                {"key1", "value1"},
                {"key2", "value2"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(dict)
                .Build();

            SecretManager secretManager = new SecretManager(configuration);

            secretManager.SetSecret("key1", "mynewvalue");

            Assert.Equal("mynewvalue", secretManager.GetSecret("key1"));
        }

        [Fact]
        public void NewSecret()
        {
            var dict = new Dictionary<string, string>
            {
                {"key1", "value1"},
                {"key2", "value2"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(dict)
                .Build();

            SecretManager secretManager = new SecretManager(configuration);

            secretManager.SetSecret("key3", "value3");

            Assert.Equal("value3", secretManager.GetSecret("key3"));
        }
    }
}
