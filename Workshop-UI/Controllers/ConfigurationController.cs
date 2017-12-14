using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pivotal.Extensions.Configuration.ConfigServer;
using Pivotal.Helper;
using Steeltoe.Extensions.Configuration.CloudFoundry;

namespace Workshop_UI.Controllers
{
    public class ConfigurationController : Controller
    {
        ILogger<ConfigurationController> _logger;
        private IConfiguration Config { get; set; }

        public ConfigurationController(ILogger<ConfigurationController> logger, IConfiguration config)
        {
            _logger = logger;
            Config = config;
        }

        public IActionResult Index()
        {
            _logger?.LogDebug("Index");

            var _index = Environment.GetEnvironmentVariable("INSTANCE_INDEX");
            if (_index == null)
            {
                _index = "Running Local";
            }

            var _prodmode = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (_prodmode == null)
            {
                _prodmode = "Production";
            }

            var _port = Environment.GetEnvironmentVariable("PORT");
            if (_port == null)
            {
                _port = "localhost";
            }

            ViewData["Index"] = $"Application Instance: {_index}";
            ViewData["ProdMode"] = $"ASPNETCORE Environment: {_prodmode}";
            ViewData["Port"] = $"Port: {_port}";
            ViewData["Uptime"] = $"Uptime: {DateTime.Now.TimeOfDay.Subtract(TimeSpan.FromMilliseconds(Environment.TickCount))}";

            ViewData["appId"] = Config["vcap:application:application_id"];
            ViewData["appName"] = Config["vcap:application:application_name"];
            ViewData["uri0"] = Config["vcap:application:application_uris:0"];
            ViewData["disk"] = Config["vcap:application:limits:disk"];
            ViewData["sourceString"] = "appsettings.json";
            IConfigurationSection configurationSection = Config.GetSection("ConnectionStrings");
            if (configurationSection != null)
            {
                if (configurationSection.GetValue<string>("AttendeeContext") != null)
                {
                    ViewData["sourceString"] = "Config Server";
                }
            }
                
            ViewData["jsonDBString"] = Config.GetConnectionString("AttendeeContext").Replace("PCF!Password", "*****");
            var cfe = new CFEnvironmentVariables();
            var _connect = cfe.getConnectionStringForDbService("user-provided", "AttendeeContext").Replace("PCF!Password", "*****");
            ViewData["boundDBString"] = _connect;

            //if (Services.Value != null)
            //{
            //    foreach (var service in Services.Value.ServicesList)
            //    {
            //        ViewData[service.Name] = service.Name;
            //        ViewData[service.Plan] = service.Plan;
            //    }
            //}


            if (Config.GetSection("spring") != null)
            {
                ViewData["AccessTokenUri"] = Config["spring:cloud:config:access_token_uri"]; 
                ViewData["ClientId"] = Config["spring:cloud:config:client_id"];
                ViewData["ClientSecret"] = Config["spring:cloud:config:client_secret"];
                ViewData["Enabled"] = Config["spring:cloud:config:enabled"];
                ViewData["Environment"] = Config["spring:cloud:config:env"];
                ViewData["FailFast"] = Config["spring:cloud:config:failFast"];
                ViewData["Label"] = Config["spring:cloud:config:label"];
                ViewData["Name"] = Config["spring:cloud:config:name"];
                ViewData["Password"] = Config["spring:cloud:config:password"];
                ViewData["Uri"] = Config["spring:cloud:config:uri"];
                ViewData["Username"] = Config["spring:cloud:config:username"];
                ViewData["ValidateCertificates"] = Config["spring:cloud:config:validate_certificates"];
            }
            else
            {
                ViewData["AccessTokenUri"] = "Not Available";
                ViewData["ClientId"] = "Not Available";
                ViewData["ClientSecret"] = "Not Available";
                ViewData["Enabled"] = "Not Available";
                ViewData["Environment"] = "Not Available";
                ViewData["FailFast"] = "Not Available";
                ViewData["Label"] = "Not Available";
                ViewData["Name"] = "Not Available";
                ViewData["Password"] = "Not Available";
                ViewData["Uri"] = "Not Available";
                ViewData["Username"] = "Not Available";
                ViewData["ValidateCertificates"] = "Not Available";
            }
            return View();
        }
    }
}