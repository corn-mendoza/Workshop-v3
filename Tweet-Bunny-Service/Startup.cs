﻿
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pivotal.Helper;
using Steeltoe.CloudFoundry.Connector.Rabbit;
// Lab07 Start
// Lab07 End

// Lab08 Start
using Steeltoe.CloudFoundry.Connector.Redis;
using Steeltoe.Extensions.Configuration.CloudFoundry;
// Lab10 End

// Lab09 Start
// Lab09 End

// Lab11 Start
using Steeltoe.Management.CloudFoundry;
// Lab08 End

// Lab10 Start
using Steeltoe.Security.Authentication.CloudFoundry;
using Steeltoe.Security.DataProtection;
using System;
// Lab11 End

namespace TweetBunnyService
{
    public class Startup
    {
        public Startup(Microsoft.Extensions.Configuration.IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            // Lab08 Start
            if (!Environment.IsDevelopment())
            {
                // Use Redis cache on CloudFoundry to DataProtection Keys
                services.AddRedisConnectionMultiplexer(Configuration);
                services.AddDataProtection()
                    .PersistKeysToRedis()
                    .SetApplicationName("tweetbunnyservice");
            }
            //Lab08 End

            //// Lab05 Start
            //services.AddScoped<IFortuneService, FortuneServiceClient>();
            //// Lab05 End

            //// Lab05 Start
            //services.Configure<FortuneServiceOptions>(Configuration.GetSection("fortuneService"));
            //// Lab05 End

            // Add for Service Options
            services.ConfigureCloudFoundryOptions(Configuration);
            //

            // Lab07 Start
            //services.AddDiscoveryClient(Configuration);

            // Lab07 End

            // Lab08 Start
            if (Environment.IsDevelopment())
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                // Use Redis cache on CloudFoundry to store session data
                services.AddDistributedRedisCache(Configuration);
            }
            // Lab08 End

            // Lab10 Start
            services.AddAuthentication((options) =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CloudFoundryDefaults.AuthenticationScheme;

            })
            .AddCookie((options) =>
            {
                options.AccessDeniedPath = new PathString("/Workshop/AccessDenied");

            })
            .AddCloudFoundryOAuth(Configuration);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("read.fortunes", policy => policy.RequireClaim("scope", "read.fortunes"));

            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // Lab10 End

            //// Lab09 Start
            //services.AddHystrixCommand<FortuneServiceCommand>("FortuneService", Configuration);
            //services.AddHystrixMetricsStream(Configuration);
            //// Lab09 End

            services.AddRabbitConnection(Configuration);
            services.AddSession();

            //services.AddSingleton<IHealthContributor, SqlServerHealthContributor>();
            
            // Lab11 Start
            services.AddCloudFoundryActuators(Configuration);
            // Lab11 End

            // Use the Bound Service for connection string if it is found in a User Provided Service
            string sourceString = "appsettings.json";
            string dbString = Configuration.GetConnectionString("SqlConnectionString");
            IConfigurationSection configurationSection = Configuration.GetSection("ConnectionStrings");
            if (configurationSection != null)
            {
                if (configurationSection.GetValue<string>("SqlConnectionString") != null)
                {
                    dbString = configurationSection.GetValue<string>("SqlConnectionString");
                    sourceString = "Config Server";
                }
            }
            else
            {
                var cfe = new CFEnvironmentVariables();
                var _connect = cfe.getConnectionStringForDbService("user-provided", "SqlConnectionString");
                if (!string.IsNullOrEmpty(_connect))
                {
                    sourceString = "User Provided Service";
                }
            }

            //services.AddDbContext<AttendeeContext>(options =>
            //        options.UseSqlServer(dbString));

            Console.WriteLine($"Using connection string from the {sourceString}");

            services.AddTransient<Application>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //    app.UseBrowserLink();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Workshop/Error");
            //}

            //app.UseStaticFiles();

            // Lab11 Start
            app.UseCloudFoundryActuators();
            // Lab11 End

            // Lab09 Start
            //app.UseHystrixRequestContext();
            // Lab09 End

            app.UseSession();

            // Lab10 Start
            app.UseAuthentication();
            // Lab10 End

            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Workshop}/{action=Index}/{id?}");
            //});


            //// Lab07 Start
            //app.UseDiscoveryClient();
            //// Lab07 End

            //// Lab09 Start
            //if (!Environment.IsDevelopment())
            //{
            //    app.UseHystrixMetricsStream();
            //}
            // Lab09 End
        }
    }
}
