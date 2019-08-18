
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

    public interface IMongoDataStore {
        Task AddDemo (Data.Demo demo, CancellationToken token);
        Task<List<Data.Demo>> FindDemoPage(string steamId, int startItem, int maxItems, DemoSortingField sortBy, bool sortDescending, CancellationToken token);
    }

    public static class DemoStoreExtensions{
        public static Task AddDemo(this IMongoDataStore store, Core.Models.Demo demo)
        {
            throw new System.NotImplementedException();
            //return store.AddDemo(demo);
        }
    }

    /*
    See Architecture.md
    */

    public class MongoDataStore : IMongoDataStore {
        IMongoDbConnection _con;
        public MongoDataStore(IMongoDbConnection con)
        {
            _con = con;
        }

        public Task AddDemo(Data.Demo demo, CancellationToken token)
        {
            var db = _con.GetDemoDatabase();
            var col = db.GetCollection<Data.Demo>("demos");
            return col.InsertOneAsync(demo, token);
        }

        public Task<List<Data.Demo>> FindDemoPage(string steamId, int startItem, int maxItems, DemoSortingField sortBy, bool sortDescending, CancellationToken token)
        {
            
            var db = _con.GetDemoDatabase();
            var col = db.GetCollection<Data.Demo>("demos");
            
            return null;
            //return col.InsertOneAsync(demo, token);
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
