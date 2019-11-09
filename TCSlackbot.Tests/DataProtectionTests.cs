using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TCSlackbot.Tests
{
    public class DependencySetupFixture
    {
        public DependencySetupFixture()
        {
            var services = new ServiceCollection();
            services.AddDataProtection();
            ServiceProvider = services.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; private set; }
    }
    public class DataProtectionTests : IClassFixture<DependencySetupFixture>
    {
        private ServiceProvider _serviceProvider;

        public DataProtectionTests(DependencySetupFixture fixture)
        {
            _serviceProvider = fixture.ServiceProvider;
        }

        [Fact]
        public void DataDecrypt()
        {
            var _protectionProvider = _serviceProvider.GetDataProtectionProvider();

            string password = "password123";

            var _protector = _protectionProvider.CreateProtector("UUIDProtector");

            Assert.Equal(password, _protector.Protect(password)); // False
        }
    }
}
