using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using EasyOffer.FrontEnd.Authentication.IdentityServer;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var builder = services.AddIdentityServer(option =>
            {
                //option.UserInteraction.LoginUrl("api/")
            })
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryClients(Config.GetClients())
                .AddProfileService<ProfileService>()
                //.AddCustomTokenRequestValidator<TokenRequestValidator>()
                //.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
                //.AddCustomAuthorizeRequestValidator<AuthorizeRequestValidator>()
                ;
            //.AddTestUsers(Config.GetUsers());

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }

            //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            //services.AddAuthentication(o =>
            //{
            //    o.DefaultScheme = "Application";
            //    o.DefaultSignInScheme = "External";
            //})
            //.AddCookie("Application")
            //.AddCookie("External")
            services.AddAuthentication()
           .AddMicrosoftAccount("Microsoft", options =>
           {
                //options.SignInScheme = JwtBearerDefaults.AuthenticationScheme; ;
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
               options.ClientId = "c5dd363e-57c8-46a3-93fe-854ddf06970f";
               options.ClientSecret = "R?N-xKAi56:bj4M1tO_PJ7Qyd[WENmOg";
               options.CallbackPath = "/account/callback";
           })
           .AddWeibo("Weibo", options =>
           {
               options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
               options.ClientId = "430309827";
               options.ClientSecret = "6c8af89f99ffee9ea59433c248e206c3";
               //options.CallbackPath = "/login";
           })
           .AddFacebook("Facebook", options =>
           {
               options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
               options.ClientId = "2576758835891717";
               options.ClientSecret = "b494862fe42c011ee50d62cb07f6938a";
              // options.CallbackPath = "/account/callback";
                // TestUser 113897956814361 fxpfhruykc_1578621243@tfbnw.net Test@1234
            })
           .AddGoogle("Google", options =>
           {
               options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

               options.ClientId = "296751627088-0irjnelnrr84afg5n523uu3ag2mhjlgu.apps.googleusercontent.com";
               options.ClientSecret = "EmLniXtdeLskcBXqMkKN8aK_";
           })
           .AddOpenIdConnect("oidc", "OpenID Connect", options =>
           {
               options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
               options.SignOutScheme = IdentityServerConstants.SignoutScheme;
               options.SaveTokens = true;

               options.Authority = "https://demo.identityserver.io/";
               options.ClientId = "implicit";

               options.TokenValidationParameters = new TokenValidationParameters
               {
                   NameClaimType = "name",
                   RoleClaimType = "role"
               };
           });


            services.AddCors(options => options.AddPolicy("CorsUrl", p => p.WithOrigins("*").AllowAnyMethod().WithExposedHeaders("Location").AllowAnyHeader()));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

          //  app.UseHttpsRedirection();
            app.UseStaticFiles();
         //   app.UseCookiePolicy();

            app.UseIdentityServer();
            app.UseCors("CorsUrl"); 
            app.UseMvcWithDefaultRoute();
        }
    }
}
