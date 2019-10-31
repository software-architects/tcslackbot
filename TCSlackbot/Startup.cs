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
using System.IdentityModel.Tokens.Jwt;
using TCSlackbot.Logic;

namespace TCSlackbot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var authenticationConfig = Configuration.GetSection("AuthenticationConfig").Get<AuthenticationConfig>();

            // https://github.com/onelogin/openid-connect-dotnet-core-sample
            // https://github.com/KevinDockx/OpenIDConnectInDepth/blob/master/src/Sample.WebClient/Startup.cs
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
                    options.ClientId = authenticationConfig.ClientId;
                    options.ClientSecret = authenticationConfig.ClientSecret;

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

            services.AddTransient<ISecretManager, SecretManager>();

            services.Configure<SlackConfig>(Configuration.GetSection("SlackConfig"));
            services.Configure<AuthenticationConfig>(Configuration.GetSection("AuthenticationConfig"));

            services.AddControllers();

            services.AddHttpClient("APIClient", client =>
            {
                client.BaseAddress = new Uri("https://api.timecockpit.com");
            });
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
