using Microsoft.Extensions.Logging;
using Steeltoe.CircuitBreaker.Hystrix;
using System;
using System.Threading.Tasks;

namespace OrderManagerService.Client
{
    // Lab09 Start
    public class OrderManagerServiceCommand : HystrixCommand<Object>
    {
        IOrderManagerService _orderManagerService;
        ILogger<OrderManagerServiceCommand> _logger;

        public OrderManagerServiceCommand(IHystrixCommandOptions options,
            IOrderManagerService orderService, 
            ILogger<OrderManagerServiceCommand> logger) : base(options)
        {
            _orderManagerService = orderService;
            _logger = logger;
            IsFallbackUserDefined = true;
        }
        public async Task<Object> RandomFortuneAsync()
        {
            return await ExecuteAsync();
        }
        protected override async Task<Object> RunAsync()
        {
            var result = await _orderManagerService.GetOrderAsync();
            _logger.LogInformation("Run: {0}", result);
            return result;
        }

        protected override async Task<Object> RunFallbackAsync()
        {
            _logger.LogInformation("RunFallback");
            return await Task.FromResult<Object>(new Object() { /*Id = 9999, Text = "You will have a happy day!" */});
        }
    }
    // Lab09 End
}
