
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using Core.Models;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Data
{
    
    internal static class LogEvents
    {
        internal static EventId StartingMongoDb = new EventId(30001);
        internal static EventId ClosingMongoDb = new EventId(30002);
        internal static EventId KillingMongoDb = new EventId(30003);
        internal static EventId ErrorOnMongoDbShutdown = new EventId(30004);
        internal static EventId MultipleSettingsDocuments = new EventId(30005);
    }

    public interface IMongoDbConnection : System.IDisposable
    {
        IMongoDatabase GetDemoDatabase();
    }
    public enum DemoSortingField{
        Date,
        Name,
        HostName,
        Duration
    }

    public interface IMongoDataStore
    {
        Task AddOrUpdateDemo(Data.Demo demo, CancellationToken token);
        Task<Data.Demo> FindDemoByFile(string filePath, CancellationToken token);
        Task<List<Data.Demo>> FindDemoPage(string steamId, int startItem, int maxItems, DemoSortingField sortBy, bool sortDescending, CancellationToken token);
    
        Task<string> AddOrUpdateAccount(Data.Account account);
        Task<ObjectId> AddOrUpdateMultiAccount(Data.MultiAccount account);

        Task<Settings> GetSettings();
        Task UpdateSettings(Settings settings);
    }

    public static class DemoStoreExtensions
    {
            
        public static Data.Source GetSourceData(this Core.Models.Demo demo) {
            
            switch (demo.SourceName ?? demo.Source?.Name ?? "unknown")
			{
				case Core.Models.Source.Valve.NAME:
					return Data.Source.MatchMaking;
				//case Esea.NAME:
				//	return new Esea();
				//case Ebot.NAME:
				//	return new Ebot();
				//case Pov.NAME:
				//	return new Pov();
				//case PugSetup.NAME:
				//	return new PugSetup();
				//case Faceit.NAME:
				//	return new Faceit();
				//case Cevo.NAME:
				//	return new Cevo();
				//case PopFlash.NAME:
				//	return new PopFlash();
				//case Esl.NAME:
				//	return new Esl();
				//case Wanmei.NAME:
				//	return new Wanmei();
				default:
					return Data.Source.Other;
			}
        }

        
        public static Data.DemoTeam ToData(this Core.Models.Team source) {
            return new Data.DemoTeam() {
                Name = source.Name,
                Score = source.Score,
                ScoreFirstHalf = source.ScoreFirstHalf,
                ScoreSecondHalf = source.ScoreSecondHalf
            };
        }
        public static Data.DemoRound ToData(this Core.Models.Round source) {
            return new Data.DemoRound() {
                Number = source.Number
            };
        }
        public static Data.Demo ToData(this Core.Models.Demo demo, ObjectId comment_account){
            return new Data.Demo() {
                DataVersion = 1,
                LocalFilePath = Path.GetFullPath(demo.Path),
                Name = demo.Name,
                Date = demo.Date,
                Source = demo.GetSourceData(),
                Hostname = demo.Hostname,
                DemoTickRate = demo.Tickrate,
                ServerTickRate = demo.ServerTickrate,
                Duration = demo.Duration,
                Ticks = demo.Ticks,
                Map = demo.MapName,
                WinningTeam = demo.Winner == demo.TeamCT ? Team.CT_Starting : Team.T_Starting,
                Surrendered = demo.Surrender != null,
                Comments = new List<DemoComment>() { new DemoComment() { Text = demo.Comment, Account = comment_account }},
                CTStartTeam = demo.TeamCT.ToData(),
                TStartTeam = demo.TeamT.ToData(),
                Rounds = demo.Rounds.Select(r => r.ToData()).ToList()
            };
        }

        public static Task AddOrUpdateDemo(this IMongoDataStore store, Core.Models.Demo demo, ObjectId comment_account, CancellationToken token)
        {
            var dataDemo = demo.ToData(comment_account);
            return store.AddOrUpdateDemo(dataDemo, token);
        }
    }

    /*
    See Architecture.md
    */

    public class MongoDataStore : IMongoDataStore
    {
        IMongoDbConnection _con;
        ILogger<MongoDataStore> _logger;
        bool _indicesOk;
        public MongoDataStore(ILogger<MongoDataStore> logger, IMongoDbConnection con)
        {
            _logger = logger;
            _con = con;
        }

        public async Task InitIndices()
        {
            if (_indicesOk)
            {
                return;
            }

            var db = _con.GetDemoDatabase();
            var col = db.GetCollection<Data.Demo>("demos");
            var indices = await col.Indexes.ListAsync();
            var indicesList = await indices.ToListAsync();
            if (indicesList.Count == 0)
            {
                // Already ok as it is the _id field now
                //await col.Indexes.CreateOneAsync(
                //    new CreateIndexModel<Account>(
                //        Builders<Account>.IndexKeys.Ascending(x => x.SteamId),
                //        new CreateIndexOptions { Unique = true }),
                //    new CreateOneIndexOptions() { });
                var options = new CreateIndexOptions() { Unique = true };
                var indexDefinition = new IndexKeysDefinitionBuilder<Demo>().Ascending(d => d.LocalFilePath);
                var indexModel = new CreateIndexModel<Demo>(indexDefinition, options);
                await col.Indexes.CreateOneAsync(indexModel);
            }

            _indicesOk = true;
        }

        private static UpdateDefinition<T> UpdateIfAny<T, TItem>(UpdateDefinition<T> upds, T obj, System.Linq.Expressions.Expression<Func<T, TItem>> expr)
        {
            var val = expr.Compile().Invoke(obj);
            if (EqualityComparer<TItem>.Default.Equals(val, default(TItem))) upds = upds.Set(expr, val);
            return upds;
        }


        public async Task AddOrUpdateDemo(Data.Demo demo, CancellationToken token)
        {
            await InitIndices();
            var db = _con.GetDemoDatabase();
            var col = db.GetCollection<Data.Demo>("demos");
            //await col.InsertOneAsync(demo, new InsertOneOptions() { }, token);
            //var up = new ObjectUpdateDefinition<Data.Demo>(demo);
            var builder = Builders<Demo>.Update;
            var update = builder.SetOnInsert(d => d.LocalFilePath, demo.LocalFilePath);
            if (demo.Id == ObjectId.Empty) {
                demo.Id = ObjectId.GenerateNewId();
                update = update.SetOnInsert(d => d.Id, demo.Id);
            };

            update = UpdateIfAny(update, demo, d => d.Comments);
            update = UpdateIfAny(update, demo, d => d.CTStartTeam);
            update = UpdateIfAny(update, demo, d => d.DataVersion);
            update = UpdateIfAny(update, demo, d => d.Date);
            update = UpdateIfAny(update, demo, d => d.DemoTickRate);
            update = UpdateIfAny(update, demo, d => d.Duration);
            update = UpdateIfAny(update, demo, d => d.Hostname);
            update = UpdateIfAny(update, demo, d => d.LocalFilePath);
            update = UpdateIfAny(update, demo, d => d.Map);
            update = UpdateIfAny(update, demo, d => d.Name);
            update = UpdateIfAny(update, demo, d => d.Rounds);
            update = UpdateIfAny(update, demo, d => d.ServerTickRate);
            update = UpdateIfAny(update, demo, d => d.Source);
            update = UpdateIfAny(update, demo, d => d.Surrendered);
            update = UpdateIfAny(update, demo, d => d.Ticks);
            update = UpdateIfAny(update, demo, d => d.TStartTeam);
            update = UpdateIfAny(update, demo, d => d.WinningTeam);
            var res = await col.UpdateOneAsync(dem => dem.LocalFilePath == demo.LocalFilePath, update, new UpdateOptions() { IsUpsert = true });

        }

        public async Task<Data.Demo> FindDemoByFile(string filePath, CancellationToken token)
        {
            await InitIndices();
            var db = _con.GetDemoDatabase();
            var col = db.GetCollection<Data.Demo>("demos");
            var findCursor = await col.FindAsync(demo => demo.LocalFilePath == filePath);
            var res = await findCursor.ToListAsync();

            if (res.Count < 1)
            {
                return null;
            }

            if (res.Count > 1)
            {
                _logger.LogWarning(LogEvents.MultipleSettingsDocuments, "Multiple ({count}) demo documents with given path, using the first.", res.Count);
            }

            return res.First();
        }

        public async Task<List<Data.Demo>> FindDemoPage(string steamId, int startItem, int maxItems, DemoSortingField sortBy, bool sortDescending, CancellationToken token)
        {
            await InitIndices();
            var db = _con.GetDemoDatabase();
            var col = db.GetCollection<Data.Demo>("demos");
            
            return null;
            //return col.InsertOneAsync(demo, token);
        }
        
        public async Task<string> AddOrUpdateAccount(Data.Account account)
        {
            if (string.IsNullOrEmpty(account.SteamId))
            {
                throw new ArgumentException("Cannot insert empty steam id");
            }

            await InitIndices();
            var db = _con.GetDemoDatabase();
            var col = db.GetCollection<Data.Account>("accounts");
            //var b = new UpdateDefinitionBuilder<Data.MultiAccount>();
            var up = new ObjectUpdateDefinition<Data.Account>(account);
            //var up = new BsonDocumentUpdateDefinition<Data.MultiAccount>(account);
            var res = await col.UpdateOneAsync(acc => acc.SteamId == account.SteamId, up, new UpdateOptions() { IsUpsert = true });
            return account.SteamId;
        }
        public async Task<ObjectId> AddOrUpdateMultiAccount(Data.MultiAccount account)
        {
            await InitIndices();
            var db = _con.GetDemoDatabase();
            var col = db.GetCollection<Data.MultiAccount>("multi_accounts");
            //var b = new UpdateDefinitionBuilder<Data.MultiAccount>();
            //var up = new BsonDocumentUpdateDefinition<Data.MultiAccount>(account);
            if (account.Id == ObjectId.Empty)
            {
                account.Id = ObjectId.GenerateNewId();
            }

            var b = new FilterDefinitionBuilder<MultiAccount>();
            //var res = await col.UpdateOneAsync(b.Empty, up, new UpdateOptions() { IsUpsert = true });
            var res = await col.ReplaceOneAsync(f => f.Id == account.Id, account, new UpdateOptions() { IsUpsert = true });
            if (res.UpsertedId == null)
            {
                return account.Id;
            }
            return res.UpsertedId.AsObjectId;
        }

        public async Task<Settings> GetSettings()
        {
            await InitIndices();

            var db = _con.GetDemoDatabase();
            var col = db.GetCollection<Data.Settings>("settings");
            var b = new FilterDefinitionBuilder<Settings>();

            var cursor = await col.FindAsync(b.Empty);
            var res = await cursor.ToListAsync();
            if (res.Count < 1)
            {
                return new Settings(); //default
            }

            if (res.Count > 1)
            {
                _logger.LogWarning(LogEvents.MultipleSettingsDocuments, "Multiple ({count}) settings documents, using the first.", res.Count);
            }

            return res.First();
        }

        public async Task UpdateSettings(Settings settings)
        {
            await InitIndices();

            var db = _con.GetDemoDatabase();
            var col = db.GetCollection<Data.Settings>("settings");
            await col.ReplaceOneAsync(set => set.Id == settings.Id, settings, new UpdateOptions() { IsUpsert = true });
        }
    }

    internal static class Helper {
        public static int GetNextFreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        public static (string mongoDir, string mongodExe) FindMongoDb()
        {
            string mongoDir, mongodExe;
            switch (Environment.OSVersion.Platform) {
                case PlatformID.Win32NT:
                    mongoDir = "mongodb-win32-x86_64.light";
                    mongodExe =  "mongod.exe";
                    break;
                default:
                    throw new System.PlatformNotSupportedException();
            }

            var trials = new [] {
                "./mongodb/bin",
                $"../external/mongodb/{mongoDir}/bin",
                $"../../external/mongodb/{mongoDir}/bin",
                $"../../../external/mongodb/{mongoDir}/bin"
            };
            var loc = Path.GetDirectoryName(typeof(IMongoDbConnection).Assembly.Location);
            var all = trials.SelectMany(rel => new [] { rel, Path.Combine(loc, rel) }).ToList();
            
            var result = all
                .Select(rel => Path.GetFullPath(rel))
                .FirstOrDefault(rel => File.Exists(Path.Combine(rel, mongodExe)));
            if(result == null){
                throw new InvalidOperationException($"Could not find mongodb directory, tried {all}");
            }
            return (result, mongodExe);
        }
    }

    public class MongoDbConnection : IMongoDbConnection {
        private readonly string mongoDbDir;
        private readonly Process process;
        private readonly IMongoDatabase database;
        private readonly IMongoDatabase adminDatabase;
        private readonly ILogger<MongoDbConnection> _logger;

        public MongoDbConnection(ILogger<MongoDbConnection> logger){
            // https://mongodb.github.io/mongo-csharp-driver/2.0/reference/driver/connecting/
            mongoDbDir = Path.GetFullPath(Core.AppSettings.GetLocalAppDataPath() + "/mongodb");
            Directory.CreateDirectory(mongoDbDir);
            var port = Helper.GetNextFreeTcpPort();
            var (mongoBinDir, mongodExe) = Helper.FindMongoDb();
            var start = new ProcessStartInfo();  
            start.FileName = mongoBinDir + "/" + mongodExe;
            //start.WindowStyle <- ProcessWindowStyle.Hidden
            start.UseShellExecute= false;
            start.Arguments = $"--port {port} --dbpath \"{mongoDbDir}\"";
            logger.LogInformation(LogEvents.StartingMongoDb, "Starting {mongod} {arguments}", start.FileName, start.Arguments);
            process = Process.Start(start);
            
            
            var client = new MongoClient($"mongodb://localhost:{port}");
            database = client.GetDatabase("csgo-demo-manager");
            adminDatabase = client.GetDatabase("admin");
            _logger = logger;
        }

        public IMongoDatabase GetDemoDatabase()
        {
            return database;
        }

        public void Dispose(){
            _logger.LogInformation(LogEvents.ClosingMongoDb, "Closing mongod");
            // https://docs.mongodb.com/manual/reference/command/shutdown/#dbcmd.shutdown
            var command = new BsonDocumentCommand<BsonDocument>(
                    new BsonDocument() { { "shutdown", 1 }, { "force", true } });
            try {
                var res = adminDatabase.RunCommand<BsonDocument>(command);
            } catch (MongoConnectionException e) {
                // surprisingly we don't get a result...
                _logger.LogWarning(LogEvents.ErrorOnMongoDbShutdown, e, "You can ignore that warning, this is a limitation of the MongoDB C# driver.");
            }

            if (!process.WaitForExit(5000)) {
                _logger.LogWarning(LogEvents.KillingMongoDb, "Killing mongod as it did not exit after timeout.");
                process.Kill();
            }
        }
    }
}
