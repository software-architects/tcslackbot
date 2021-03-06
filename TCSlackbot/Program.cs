using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;

namespace TCSlackbot
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, builder) =>
                    {
                        var keyVaultEndpoint = GetKeyVaultEndpoint();
                        if (!string.IsNullOrEmpty(keyVaultEndpoint))
                        {
                            var azureServiceTokenProvider = new AzureServiceTokenProvider();
                            var authenticationCallback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);
                            var keyVaultClient = new KeyVaultClient(authenticationCallback);

                            builder.AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
                        }
                    })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static string GetKeyVaultEndpoint() => "https://tcslackbot-key-vault.vault.azure.net/";
    }
}
