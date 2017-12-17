using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderManagerService.Client
{
    public interface IOrderManagerService
    {
        Task<List<Object>> AllOrdersAsync();
        Task<Object> GetOrderAsync();
    }
}
