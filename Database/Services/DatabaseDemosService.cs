using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Core.Models;
using Services.Interfaces;
using Services.Models.Timelines;

namespace Database.Services
{
    public class DatabaseDemosService : IDemosService
    {
        private readonly IDemosService _wrapped;
        private readonly DatabaseCacheService _cache;
        private readonly IDatabaseProvider _dbProvider;

        public DatabaseDemosService(IDemosService wrapped, DatabaseCacheService cache, IDatabaseProvider dbProvider)
        {
            _wrapped = wrapped;
            _cache = cache;
            _dbProvider = dbProvider;
        }

        public string DownloadFolderPath
        {
            get => _wrapped.DownloadFolderPath;
            set => _wrapped.DownloadFolderPath = value;
        }

        public long SelectedStatsAccountSteamId
        {
            get => _wrapped.SelectedStatsAccountSteamId;
            set => _wrapped.SelectedStatsAccountSteamId = value;
        }

        public bool ShowOnlyAccountDemos
        {
            get => _wrapped.ShowOnlyAccountDemos;
            set => _wrapped.ShowOnlyAccountDemos = value;
        }

        public bool IgnoreLaterBan
        {
            get => _wrapped.IgnoreLaterBan;
            set => _wrapped.IgnoreLaterBan = value;
        }

        public Task<Demo> GetDemoHeaderAsync(string demoFilePath)
        {
            return _wrapped.GetDemoHeaderAsync(demoFilePath);
        }

        public Task<List<Demo>> GetDemosHeader(List<string> folders, List<Demo> currentDemos = null, int size = 0)
        {
            return _wrapped.GetDemosHeader(folders, currentDemos, size);
        }

        public Task<Demo> GetDemoDataAsync(Demo demo)
        {
            return _wrapped.GetDemoDataAsync(demo);
        }

        public Task<Demo> GetDemoDataByIdAsync(string demoId)
        {
            return _wrapped.GetDemoDataByIdAsync(demoId);
        }

        public Task<Demo> AnalyzeDemo(Demo demo, CancellationToken token, Action<string, float> progressCallback = null)
        {
            return _wrapped.AnalyzeDemo(demo, token, progressCallback);
        }

        public Task SaveComment(Demo demo, string comment)
        {
            return _wrapped.SaveComment(demo, comment);
        }

        public Task SaveStatus(Demo demo, string status)
        {
            return _wrapped.SaveStatus(demo, status);
        }

        public Task<ObservableCollection<Demo>> SetSource(ObservableCollection<Demo> demos, string source)
        {
            return _wrapped.SetSource(demos, source);
        }

        public Task<Demo> SetSource(Demo demo, string source)
        {
            return _wrapped.SetSource(demo, source);
        }

        public Task<Demo> AnalyzePlayersPosition(Demo demo, CancellationToken token)
        {
            return _wrapped.AnalyzePlayersPosition(demo, token);
        }

        public Task<List<Demo>> GetDemosFromBackup(string jsonFile)
        {
            return _wrapped.GetDemosFromBackup(jsonFile);
        }

        public Task<Demo> AnalyzeBannedPlayersAsync(Demo demo)
        {
            return _wrapped.AnalyzeBannedPlayersAsync(demo);
        }

        public Task<Rank> GetLastRankAccountStatsAsync(long steamId)
        {
            return _wrapped.GetLastRankAccountStatsAsync(steamId);
        }

        public async Task<List<Demo>> GetDemosPlayer(string steamId)
        {
            if (!_cache.DemosCachedInDb)
            {
                // Init Cache if needed
                await _cache.GetDemoListAsync();
            }

            Debug.Assert(_cache.DemosCachedInDb);
            return _dbProvider.DemoRepository.QueryDemosOfUser(long.Parse(steamId));
            //return await _wrapped.GetDemosPlayer(steamId);
        }

        public Task<bool> DeleteDemo(Demo demo)
        {
            return _wrapped.DeleteDemo(demo);
        }

        public Task<Dictionary<string, string>> GetDemoListUrl()
        {
            return _wrapped.GetDemoListUrl();
        }

        public Task<bool> DownloadDemo(string url, string location)
        {
            return _wrapped.DownloadDemo(url, location);
        }

        public Task<bool> DecompressDemoArchive(string demoName)
        {
            return _wrapped.DecompressDemoArchive(demoName);
        }

        public void WriteChatFile(Demo demo, string filePath)
        {
            _wrapped.WriteChatFile(demo, filePath);
        }

        public Task<string> GetShareCode(Demo demo)
        {
            return _wrapped.GetShareCode(demo);
        }

        public Task<string> GetOldId(Demo demo)
        {
            return _wrapped.GetOldId(demo);
        }

        public Task<List<TimelineEvent>> GetTimeLineEventList(Demo demo)
        {
            return _wrapped.GetTimeLineEventList(demo);
        }
    }
}