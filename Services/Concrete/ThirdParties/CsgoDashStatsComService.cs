using System.Threading.Tasks;
using Core.Models;
using Services.Interfaces;
using Services.Models.ThirdParties;

namespace Services.Concrete.ThirdParties
{
    class CsgoDashStatsComService : IThirdPartyInterface
    {

        public Task<ThirdPartyData> SendShareCode(Demo demo, string shareCode)
        {
            ThirdPartyData data = new ThirdPartyData { Success = true };
            data.DemoUrl = $"https://csgo-stats.com/match/{shareCode}";

            return Task.FromResult(data);
        }
    }
}
