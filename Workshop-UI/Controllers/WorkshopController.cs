
using FortuneService.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.Common.Discovery;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
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

        // Lab09 Start
        private FortuneServiceCommand _fortunes;
        public WorkshopController(
            ILogger<WorkshopController> logger,
            IOptionsSnapshot<FortuneServiceOptions> config,
            FortuneServiceCommand fortunes, 
            IOptions<CloudFoundryApplicationOptions> appOptions,
            IOptions<CloudFoundryServicesOptions> servOptions,
            [FromServices] IDiscoveryClient client
            )
        {
            _logger = logger;
            _fortunes = fortunes;
            CloudFoundryServices = servOptions.Value;
            CloudFoundryApplication = appOptions.Value;
            _fortunesConfig = config;
            discoveryClient = client;
        }
        // Lab09 End
        
        public IActionResult Index()
        {
            _logger?.LogDebug("Index");
            return View();
        }

        public IActionResult Steeltoe()
        {
            _logger?.LogDebug("Steeltoe");
            return View();
        }

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

            HttpContext.Session.SetString("MyFortune", fortune.Text);
            var _idxCount = HttpContext.Session.GetInt32($"Index{CloudFoundryApplication.InstanceIndex}");
            if (_idxCount != null)
                _idxCount += 1;

            HttpContext.Session.SetInt32($"Index{CloudFoundryApplication.InstanceIndex}", _idxCount.GetValueOrDefault(1));
            ViewData["InstanceCount"] = $"{_idxCount.GetValueOrDefault(1)}";
            ViewData["MyFortune"] = fortune.Text;
            ViewData["FortuneIndex"] = $"{fortune.InstanceIndex}";
            ViewData["FortuneDiscoveryUrl"] = discoveryClient.GetInstances("fortuneService")?[fortune.InstanceIndex]?.Uri?.ToString();
            return View(new CloudFoundryViewModel(
                CloudFoundryApplication == null ? new CloudFoundryApplicationOptions() : CloudFoundryApplication,
                CloudFoundryServices == null ? new CloudFoundryServicesOptions() : CloudFoundryServices));

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
