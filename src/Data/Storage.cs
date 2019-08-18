
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

namespace Data
{
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
    string mongoDbDir;
    Process process;
    IMongoDatabase database;
    public MongoDbConnection(){
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
        process = Process.Start(start);
        
        
        var client = new MongoClient($"mongodb://localhost:{port}");
        database = client.GetDatabase("csgo-demo-manager");
    }

    public IMongoDatabase GetDemoDatabase()
    {
        return database;
    }

    public void Dispose(){
        process.Kill();
    }
}
}
