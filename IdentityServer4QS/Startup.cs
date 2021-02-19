//===============================================================================
// Microsoft FastTrack for Azure
// IdentityServer4 Custom Claims Samples
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================
using IdentityServer4;
using IdentityServer4QS.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4QS
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<SqlHelper>(sp => new SqlHelper(Config.GetRolesConnectionString()));
            services.AddMvc();
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients())
                .AddTestUsers(Config.GetUsers())
                .AddProfileService<ProfileService>();

            services.AddAuthentication()
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, "Azure AD",options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                    options.ClientId = "{IdentityServer4 client ID in your Azure AD tenant}";
                    options.Authority = "https://login.microsoftonline.com/{your Azure AD tenant ID}";
                    options.SignedOutRedirectUri = "http://localhost:49814/";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenValidated = ctx =>
                        {
                            IEnumerable<Claim> claims = ctx.Principal.Claims;
                            ClaimsIdentity identity = (ClaimsIdentity)ctx.Principal.Identity;
                            var claim = (from c in identity.Claims
                                         where c.Type == "name"
                                         select c).Single();
                            identity.RemoveClaim(claim);
                            return Task.FromResult(0);
                        }
                    };
                    options.GetClaimsFromUserInfoEndpoint = true;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();

            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
