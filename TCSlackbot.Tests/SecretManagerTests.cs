using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TCSlackbot.Logic;

namespace TCSlackbot.Tests
{
    [TestClass]
    public class SecretManagerTests
    {
        [TestMethod]
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

            Assert.AreEqual("value1", secretManager.GetSecret("key1"));
            Assert.AreEqual("value2", secretManager.GetSecret("key2"));
        }

        [TestMethod]
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

            Assert.IsNull(secretManager.GetSecret("wrong_key"));
        }

        [TestMethod]
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

            Assert.AreEqual("mynewvalue", secretManager.GetSecret("key1"));
        }

        [TestMethod]
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

            Assert.AreEqual("value3", secretManager.GetSecret("key3"));
        }
    }
}
