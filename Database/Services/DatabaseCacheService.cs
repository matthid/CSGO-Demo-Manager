using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Core.Models;
using Core.Models.Events;
using Services.Interfaces;
using Services.Models;

namespace Database.Services
{
    public class DatabaseCacheService : ICacheService
    {
        private readonly ICacheService _wrapped;

        private readonly IDatabaseProvider _dbProvider;
        private bool _demosCachedInDb;

        public DatabaseCacheService(ICacheService wrapped, IDatabaseProvider dbProvider)
        {
            _wrapped = wrapped;
            _dbProvider = dbProvider;
        }

        public DemoFilter Filter
        {
            get => _wrapped.Filter;
            set => _wrapped.Filter = value;
        }

        internal bool DemosCachedInDb => _demosCachedInDb;

        public bool HasDemoInCache(string demoId)
        {
            return _wrapped.HasDemoInCache(demoId);
        }

        public Task<Demo> GetDemoDataFromCache(string demoId)
        {
            return _wrapped.GetDemoDataFromCache(demoId);
        }

        public Task WriteDemoDataCache(Demo demo)
        {
            return _wrapped.WriteDemoDataCache(demo);
        }

        public Task<bool> AddSuspectToCache(string suspectSteamCommunityId)
        {
            return _wrapped.AddSuspectToCache(suspectSteamCommunityId);
        }

        public Task<List<string>> GetSuspectsListFromCache()
        {
            return _wrapped.GetSuspectsListFromCache();
        }

        public Task<bool> RemoveSuspectFromCache(string steamId)
        {
            return _wrapped.RemoveSuspectFromCache(steamId);
        }

        public Task ClearDemosFile()
        {
            return _wrapped.ClearDemosFile();
        }

        public Task CreateBackupCustomDataFile(string filePath)
        {
            return _wrapped.CreateBackupCustomDataFile(filePath);
        }

        public bool ContainsDemos()
        {
            return _wrapped.ContainsDemos();
        }

        public Task<bool> AddSteamIdToBannedList(string steamId)
        {
            return _wrapped.AddSteamIdToBannedList(steamId);
        }

        public Task<List<string>> GetSuspectsBannedList()
        {
            return _wrapped.GetSuspectsBannedList();
        }

        public Task<bool> AddAccountAsync(Account account)
        {
            return _wrapped.AddAccountAsync(account);
        }

        public Task<bool> RemoveAccountAsync(Account account)
        {
            return _wrapped.RemoveAccountAsync(account);
        }

        public Task<bool> UpdateAccountAsync(Account account)
        {
            return _wrapped.UpdateAccountAsync(account);
        }

        public Task<List<Account>> GetAccountListAsync()
        {
            return _wrapped.GetAccountListAsync();
        }

        public Task<Account> GetAccountAsync(long steamId)
        {
            return _wrapped.GetAccountAsync(steamId);
        }

        public Task<List<string>> GetFoldersAsync()
        {
            return _wrapped.GetFoldersAsync();
        }

        public Task<bool> AddFolderAsync(string path)
        {
            return _wrapped.AddFolderAsync(path);
        }

        public Task<bool> RemoveFolderAsync(string path)
        {
            return _wrapped.RemoveFolderAsync(path);
        }

        public async Task<List<Demo>> GetDemoListAsync()
        {
            var result = await _wrapped.GetDemoListAsync();
            if (!_demosCachedInDb)
            {
                _demosCachedInDb = true;
                await Task.Run(() =>
                {
                    foreach (var demo in result)
                    {
                        _dbProvider.DemoRepository.SaveDemo(demo);
                    }
                });
            }

            return result;
        }

        public Task<List<Demo>> GetFilteredDemoListAsync()
        {
            return _wrapped.GetFilteredDemoListAsync();
        }

        public Task<List<string>> GetPlayersWhitelist()
        {
            return _wrapped.GetPlayersWhitelist();
        }

        public Task<bool> AddPlayerToWhitelist(string suspectSteamCommunityId)
        {
            return _wrapped.AddPlayerToWhitelist(suspectSteamCommunityId);
        }

        public Task<bool> RemovePlayerFromWhitelist(string steamId)
        {
            return _wrapped.RemovePlayerFromWhitelist(steamId);
        }

        public Task<bool> GenerateJsonAsync(Demo demo, string folderPath)
        {
            return _wrapped.GenerateJsonAsync(demo, folderPath);
        }

        public Task<long> GetCacheSizeAsync()
        {
            return _wrapped.GetCacheSizeAsync();
        }

        public Task<bool> RemoveDemo(string demoId)
        {
            return _wrapped.RemoveDemo(demoId);
        }

        public Task<ObservableCollection<WeaponFireEvent>> GetDemoWeaponFiredAsync(Demo demo)
        {
            return _wrapped.GetDemoWeaponFiredAsync(demo);
        }

        public Task<ObservableCollection<PlayerBlindedEvent>> GetDemoPlayerBlindedAsync(Demo demo)
        {
            return _wrapped.GetDemoPlayerBlindedAsync(demo);
        }

        public Task<RankInfo> GetLastRankInfoAsync(long steamId)
        {
            return _wrapped.GetLastRankInfoAsync(steamId);
        }

        public Task<Rank> GetLastRankAsync(long steamId)
        {
            return _wrapped.GetLastRankAsync(steamId);
        }

        public Task<bool> SaveLastRankInfoAsync(RankInfo rankInfo)
        {
            return _wrapped.SaveLastRankInfoAsync(rankInfo);
        }

        public Task<List<RankInfo>> GetRankInfoListAsync()
        {
            return _wrapped.GetRankInfoListAsync();
        }

        public Task<bool> UpdateRankInfoAsync(Demo demo, long steamId)
        {
            return _wrapped.UpdateRankInfoAsync(demo, steamId);
        }

        public Task ClearRankInfoAsync()
        {
            return _wrapped.ClearRankInfoAsync();
        }

        public Task<bool> RemoveRankInfoAsync(long steamId)
        {
            return _wrapped.RemoveRankInfoAsync(steamId);
        }

        public Task<bool> DeleteVdmFiles()
        {
            return _wrapped.DeleteVdmFiles();
        }

        public bool HasDummyCacheFile()
        {
            return _wrapped.HasDummyCacheFile();
        }

        public void DeleteDummyCacheFile()
        {
            _wrapped.DeleteDummyCacheFile();
        }

        public Task<DemoBasicData> AddDemoBasicDataAsync(Demo demo)
        {
            return _wrapped.AddDemoBasicDataAsync(demo);
        }

        public Task<List<DemoBasicData>> GetDemoBasicDataAsync()
        {
            return _wrapped.GetDemoBasicDataAsync();
        }

        public Task<bool> InitDemoBasicDataList()
        {
            return _wrapped.InitDemoBasicDataList();
        }
    }
}