using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using TCSlackbot.Logic;
using TCSlackbot.Logic.Utils;

namespace TCSlackbot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var clientId = Configuration["TimeCockpit-ClientId"];
            var clientSecret = Configuration["TimeCockpit-ClientSecret"];

            Debug.Assert(!string.IsNullOrEmpty(clientId));
            Debug.Assert(!string.IsNullOrEmpty(clientSecret));

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = "/auth/login";
                    options.AccessDeniedPath = "/error";
                })
                .AddOpenIdConnect(options =>
                {
                    options.Authority = "https://auth.timecockpit.com/";
                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;

                    options.UsePkce = true;
                    options.SaveTokens = true;
                    options.RequireHttpsMetadata = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    // Use the authorization code flow.
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;

                    options.Scope.Add("openid");
                    options.Scope.Add("offline_access");

                    options.SecurityTokenValidator = new JwtSecurityTokenHandler
                    {
                        InboundClaimTypeMap = new Dictionary<string, string>()
                    };
                });

            services.AddHttpClient("APIClient", client =>
            {
                client.BaseAddress = new Uri("https://api.timecockpit.com");
            });

            services.AddHttpClient("BotClient", client =>
            {
                client.BaseAddress = new Uri("https://slack.com/api");
            });

            services.AddTransient<ISecretManager, SecretManager>();
            services.AddTransient<ICosmosManager, CosmosManager>();
            services.AddTransient<ITokenManager, TokenManager>();

            services.AddControllers();
            services.AddDataProtection();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
