﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Almirex.Contracts.Interfaces;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Integration.Wcf;
using Autofac.Integration.Web;
using Autofac.Integration.WebApi;
using ExchangeLegacy.Config;
using ExchangeLegacy.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pivotal.Discovery.Client;
using Pivotal.Extensions.Configuration;
using Pivotal.Extensions.Configuration.ConfigServer;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.CloudFoundry.Connector.MySql;
using Steeltoe.CloudFoundry.Connector.Rabbit;

namespace ExchangeLegacy
{
    public class WebApiApplication : System.Web.HttpApplication, IContainerProviderAccessor
    {
        private static IContainerProvider _containerProvider;

        public IContainerProvider ContainerProvider => _containerProvider;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var services = new ServiceCollection();


            ILoggerFactory logFactory = new LoggerFactory();
            logFactory.AddConsole(minLevel: LogLevel.Error);
            string env = "Production";
            ServerConfig.RegisterConfig(configBuilder =>
                configBuilder
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{env}.json", optional: true)
                    .AddConfigServer(env, logFactory)
                    .AddEnvironmentVariables());
//            services.AddDiscoveryClient(ServerConfig.Configuration);
            services.AddOptions();
            services.AddMySqlConnection(ServerConfig.Configuration);
            services.AddSingleton<OrderbookService>();
            services.Configure<Steeltoe.Discovery.Client.SpringConfig>(ServerConfig.Configuration.GetSection("spring"));
            services.Configure<ExchangeConfig>(ServerConfig.Configuration.GetSection("config"));
            services.AddSingleton<IFeeSchedule, FlatFeeScheduler>();
            services.AddRabbitConnection(ServerConfig.Configuration, ServiceLifetime.Singleton);
//            services.AddHystrixMetricsStream(ServerConfig.Configuration);

            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.RegisterType<Exchange>();

            builder.Register(x => x.Resolve<MySqlConnection>()).As<IDbConnection>();
            builder.Register(ctx => logFactory).As<ILoggerFactory>().SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();
            builder.RegisterDiscoveryClient(ServerConfig.Configuration);
            builder.RegisterHystrixMetricsStream(ServerConfig.Configuration);
            var config = GlobalConfiguration.Configuration;
            // Register your Web API controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // Register the Autofac filter provider.
            builder.RegisterWebApiFilterProvider(config);

            // Register the Autofac model binder provider.
            builder.RegisterWebApiModelBinderProvider();
            var container = builder.Build();
            container.StartDiscoveryClient();
            container.StartHystrixMetricsStream();
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            AutofacHostFactory.Container = container;
            _containerProvider = new ContainerProvider(container);
            container.Resolve<OrderbookService>().Recover();
            
            Console.WriteLine(">> Exchange Started<<");
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

            // Get the exception object.
            Exception exc = Server.GetLastError();
            Console.Error.WriteLine(exc);
        }
        private JsonSerializerSettings ConfigureSerializer(JsonSerializerSettings serializer)
        {
            serializer.Formatting = Formatting.Indented;
            //            serializer.NullValueHandling = NullValueHandling.Ignore;
            //            serializer.DefaultValueHandling = DefaultValueHandling.Ignore;
            serializer.MissingMemberHandling = MissingMemberHandling.Ignore;
            serializer.Converters = new List<JsonConverter> { new StringEnumConverter() };
            return serializer;

        }
    }
}
