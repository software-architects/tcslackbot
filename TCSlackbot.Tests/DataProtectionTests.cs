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
        private readonly ServiceProvider _serviceProvider;

        public DataProtectionTests(DependencySetupFixture fixture)
        {
            if (fixture == null)
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentNullException("DependencySetupFixture");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

            _serviceProvider = fixture.ServiceProvider;
        }

        [Fact]
        public void DataDecrypt()
        {
            var _protectionProvider = _serviceProvider.GetDataProtectionProvider();
            var _protector = _protectionProvider.CreateProtector("UUIDProtector");

            string password = "password123";


            Assert.NotEqual(password, _protector.Protect(password));
        }

        [Fact]
        public void DataEncrypt()
        {
            var _protectionProvider = _serviceProvider.GetDataProtectionProvider();
            var _protector = _protectionProvider.CreateProtector("UUIDProtector");

            string password = "password123";
            var encryptedPassword = _protector.Protect(password);

            Assert.NotNull(_protector.Unprotect(encryptedPassword));
            Assert.Equal(password, _protector.Unprotect(encryptedPassword));
        }
    }
}
