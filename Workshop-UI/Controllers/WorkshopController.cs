
using FortuneService.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Steeltoe.Common.Discovery;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop_UI.ViewModels.Workshop;

namespace Workshop_UI.Controllers
{
    public class WorkshopController : Controller
    {
        ILogger<WorkshopController> _logger;
        public CloudFoundryServicesOptions CloudFoundryServices { get; set; }
        public CloudFoundryApplicationOptions CloudFoundryApplication { get; set; }
        IOptionsSnapshot<FortuneServiceOptions> _fortunesConfig;
        IDiscoveryClient discoveryClient;
        IDistributedCache RedisCacheStore { get; set; }

        // Lab09 Start
        private FortuneServiceCommand _fortunes;
        public WorkshopController(
            ILogger<WorkshopController> logger,
            IOptionsSnapshot<FortuneServiceOptions> config,
            FortuneServiceCommand fortunes,
            IOptions<CloudFoundryApplicationOptions> appOptions,
            IOptions<CloudFoundryServicesOptions> servOptions,
            IDistributedCache cache,
            [FromServices] IDiscoveryClient client
            )
        {
            _logger = logger;
            _fortunes = fortunes;
            CloudFoundryServices = servOptions.Value;
            CloudFoundryApplication = appOptions.Value;
            _fortunesConfig = config;
            discoveryClient = client;
            RedisCacheStore = cache;
        }
        // Lab09 End
        
        public IActionResult Index()
        {
            _logger?.LogDebug("Index");
            return View();
        }

        [HttpGet]
        public IActionResult Steeltoe()
        {
            _logger?.LogDebug("Steeltoe");
            return View();
        }

        [HttpGet]
        // Lab10 Start
        //[Authorize(Policy = "read.fortunes")] 
        // Lab10 End
        public async Task<IActionResult> Services()
        {
            _logger?.LogDebug("RandomFortune");

            ViewData["FortuneUrl"] = _fortunesConfig.Value.RandomFortuneURL;

            // Lab05 Start
            var fortune = await _fortunes.RandomFortuneAsync();
            // Lab05 End

            SortedList<int, int> appInstCount = new SortedList<int, int>();
            SortedList<int, int> srvInstCount = new SortedList<int, int>();
            List<string> fortunes = new List<string>();

            var _fortuneHistory = RedisCacheStore?.GetString("FortuneHistory");
            if (!string.IsNullOrEmpty(_fortuneHistory))
                fortunes = JsonConvert.DeserializeObject<List<string>>(_fortuneHistory);

            fortunes.Insert(0, fortune.Text);

            if (fortunes.Count > 10)
            {
                fortunes.RemoveAt(10);
            }

            string fortuneoutput = JsonConvert.SerializeObject(fortunes);
            RedisCacheStore?.SetString("FortuneHistory", fortuneoutput);
            
            HttpContext.Session.SetString("MyFortune", fortune.Text);
            
            var _appInstCount = RedisCacheStore?.GetString("AppInstance");
            if (!string.IsNullOrEmpty(_appInstCount))
            {
                _logger?.LogInformation($"App Session Data: {_appInstCount}");
                appInstCount = JsonConvert.DeserializeObject<SortedList<int, int>>(_appInstCount);
            }

            var _srvInstCount = RedisCacheStore?.GetString("SrvInstance");
            if (!string.IsNullOrEmpty(_srvInstCount))
            {
                _logger?.LogInformation($"Servlet Session Data: {_srvInstCount}");
                srvInstCount = JsonConvert.DeserializeObject<SortedList<int, int>>(_srvInstCount);
            }

            var _count = appInstCount.GetValueOrDefault(CloudFoundryApplication.Instance_Index, 0);
            appInstCount[CloudFoundryApplication.Instance_Index] = ++_count;

            var _count2 = srvInstCount.GetValueOrDefault(fortune.InstanceIndex, 0);
            srvInstCount[fortune.InstanceIndex] = ++_count2;
            
            string output = JsonConvert.SerializeObject(appInstCount);
            RedisCacheStore?.SetString("AppInstance", output);
            string output2 = JsonConvert.SerializeObject(srvInstCount);
            RedisCacheStore?.SetString("SrvInstance", output2);

            ViewData["MyFortune"] = fortune.Text;
            ViewData["FortuneIndex"] = $"{fortune.InstanceIndex}";
            ViewData["FortuneDiscoveryUrl"] = discoveryClient.GetInstances("fortuneService")?[fortune.InstanceIndex]?.Uri?.ToString();
            return View(new CloudFoundryViewModel(
                CloudFoundryApplication == null ? new CloudFoundryApplicationOptions() : CloudFoundryApplication,
                CloudFoundryServices == null ? new CloudFoundryServicesOptions() : CloudFoundryServices,
                discoveryClient,
                appInstCount,
                srvInstCount,
                fortunes));
        }

        [HttpPost]
        public async Task<IActionResult> LogOff()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            await HttpContext.Session.CommitAsync();
            return RedirectToAction(nameof(WorkshopController.Index), "Workshop");
        }

        [HttpGet]
        // Lab10 Start
        [Authorize]
        // Lab10 Start
        public IActionResult Login()
        {
            return RedirectToAction(nameof(WorkshopController.Index), "Workshop");
        }

        [Authorize]
        public IActionResult Kill()
        {
            Environment.Exit(-99);
            return View();
        }


        public IActionResult Manage()
        {
            ViewData["Message"] = "Manage accounts using UAA or CF command line.";
            return View();
        }

        public IActionResult AccessDenied()
        {
            ViewData["Message"] = "Insufficient permissions.";
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

    }
}
