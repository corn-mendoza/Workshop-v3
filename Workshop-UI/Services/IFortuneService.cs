using System.Collections.Generic;
using System.Threading.Tasks;

namespace Workshop_UI.Services
{
    public interface IFortuneService
    {
        Task<List<Fortune>> AllFortunesAsync();
        Task<Fortune> RandomFortuneAsync();
    }
}
