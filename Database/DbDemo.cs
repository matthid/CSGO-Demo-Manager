using System;
using Dapper;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Core.Models.Source;

namespace Database
{
    public class DbDemo
    {
        public int Id { get; set; }
    }

    public interface IDemoRepository
    {
        Demo GetDemo(string id);
        void SaveDemo(Demo customer);
        List<Demo> QueryDemosOfUser(long steamId);
    }

    public class SqLiteDemoRepository : SqLiteBaseRepository, IDemoRepository
    {
        public string SHA256CheckSum(string filePath)
        {
            using (SHA256 SHA256 = SHA256Managed.Create())
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                    return Convert.ToBase64String(SHA256.ComputeHash(fileStream));
            }
        }

        private long FromStatus(string status)
        {
            switch (status.ToLowerInvariant())
            {
                case "none":
                    return 0;
                case "error":
                    return 1;
                default:
                    throw new InvalidOperationException($"Unknown status '{status}'");
            }
        }

        private string ToStatus(long status)
        {
            switch (status)
            {
                case 0:
                    return "None";
                case 1:
                    return "error";
                default:
                    throw new InvalidOperationException($"Unknown watch status '{status}' in database.");
            }
        }

        public void SaveDemo(Demo demo)
        {
            var cnn = GetConnection();
            DateTime lastAccess = DateTime.MinValue;
            string sha256 = null;
            //if (File.Exists(demo.Path))
            //{
            //    lastAccess = File.GetLastWriteTimeUtc(demo.Path);
            //    sha256 = SHA256CheckSum(demo.Path);
            //}

            var fileId = cnn.Query<long>(
                @"INSERT INTO FileRef
        ( Path, LastChange, Sha256Hash ) VALUES
        ( @Path, @LastChange, @Sha256 );
         select last_insert_rowid()",
                new
                {
                    Path = demo.Path,
                    LastChange = lastAccess,
                    Sha256 = sha256
                });


            var ret = cnn.Execute(
                @"INSERT INTO Demo
        ( ID, Name, Date, Source, ClientName, HostName, IsGOTV, DemoTickrate, HostTickrate, Duration, Ticks, MapName,
          FileRef, Comment, WatchStatus, CTStartingTeamFirstHalfScore, TStartingTeamFirstHalfScore, CTStartingTeamSecondHalfScore, 
          TStartingTeamSecondHalfScore, CTStartingTeamName, TStartingTeamName, SurrendingTeam, WinningTeam ) VALUES
        ( @Id, @Name, @Date, @Source, @ClientName, @HostName, @IsGOTV, @DemoTickrate, @HostTickrate, @Duration, @Ticks,
          @MapName, @FileRef, @Comment, @WatchStatus, @CTStartingTeamFirstHalfScore, @TStartingTeamFirstHalfScore,
          @CTStartingTeamSecondHalfScore, @TStartingTeamSecondHalfScore, @CTStartingTeamName, @TStartingTeamName, 
          @SurrendingTeam, @WinningTeam)",
                new
                {
                    Id = demo.Id, Name = demo.Name, Date = demo.Date, Source = demo.SourceName, ClientName = demo.ClientName, HostName = demo.Hostname,
                    IsGOTV = demo.Type == "GOTV" ? 1 : 0, DemoTickrate = demo.Tickrate, HostTickrate = demo.ServerTickrate, Duration = demo.Duration,
                    Ticks = demo.Ticks, MapName = demo.MapName, FileRef = fileId, Comment = demo.Comment,
                    WatchStatus = FromStatus(demo.Status),
                    CTStartingTeamFirstHalfScore = demo.TeamCT.ScoreFirstHalf,
                    TStartingTeamFirstHalfScore = demo.TeamT.ScoreFirstHalf,
                    CTStartingTeamSecondHalfScore = demo.TeamCT.ScoreSecondHalf,
                    TStartingTeamSecondHalfScore = demo.TeamT.ScoreSecondHalf,
                    CTStartingTeamName = demo.TeamCT.Name,
                    TStartingTeamName = demo.TeamT.Name,
                    SurrendingTeam = demo.Surrender == null ? null : (demo.Surrender == demo.TeamCT ? (int?)0 : 1),
                    WinningTeam = demo.Winner == null ? null : (demo.Winner == demo.TeamCT ? (int?)0 : 1)
                });
            Debug.Assert(ret == 1);

            foreach (var (team, demoPlayer) in new []{ demo.TeamCT.Players.Select(p => (demo.TeamCT, p)), demo.TeamT.Players.Select(p => (demo.TeamT, p)) }.SelectMany(t => t))
            {
                cnn.Execute(
                    @"INSERT INTO PlayerInDemo
                (SteamId, Demo, StartingTeam, Name, KillCount, FlashbangThrownCount, SmokeThrownCount, HeThrownCount, MolotovThrownCount, IncendiaryThrownCount, DecoyThrownCount,
                 TradeKillCount, BombPlantedCount,BombDefusedCount, BombExplodedCount, CrouchKillCount, JumpKillCount, HitCount,
                 HeadshotCount, DeathCount, AssistCount, EntryKillCount, MvpCount, TeamKillCount, RoundPlayedCount,
                 AverageHltvRating, AverageEseaRws, OneKillCount, TwoKillCount, ThreeKillCount, FourKillCount, FiveKillCount) VALUES
                (@SteamId, @Demo, @StartingTeam, @Name, @KillCount,@FlashbangThrownCount, @SmokeThrownCount, @HeThrownCount, @MolotovThrownCount, @IncendiaryThrownCount, @DecoyThrownCount,
                 @TradeKillCount, @BombPlantedCount, @BombDefusedCount, @BombExplodedCount, @CrouchKillCount, @JumpKillCount, @HitCount,
                 @HeadshotCount, @DeathCount, @AssistCount, @EntryKillCount, @MvpCount, @TeamKillCount, @RoundPlayedCount,
                 @AverageHltvRating, @AverageEseaRws, @OneKillCount, @TwoKillCount, @ThreeKillCount, @FourKillCount, @FiveKillCount)", new
                    {
                        SteamId               = demoPlayer.SteamId,
                        Demo                  = demo.Id,
                        StartingTeam          = team == demo.TeamCT ? 0 : 1,
                        Name                  = demoPlayer.Name,
                        KillCount             = demoPlayer.KillCount,
                        FlashbangThrownCount  = demoPlayer.FlashbangThrownCount,
                        SmokeThrownCount      = demoPlayer.SmokeThrownCount,
                        HeThrownCount         = demoPlayer.HeGrenadeThrownCount,
                        MolotovThrownCount    = demoPlayer.MolotovThrownCount,
                        IncendiaryThrownCount = demoPlayer.IncendiaryThrownCount,
                        DecoyThrownCount      = demoPlayer.DecoyThrownCount,
                        TradeKillCount        = demoPlayer.TradeKillCount,
                        BombPlantedCount      = demoPlayer.BombPlantedCount,
                        BombDefusedCount      = demoPlayer.BombDefusedCount,
                        BombExplodedCount     = demoPlayer.BombExplodedCount,
                        CrouchKillCount       = demoPlayer.CrouchKillCount,
                        JumpKillCount         = demoPlayer.JumpKillCount,
                        //WeaponFiredCount      = demoPlayer,
                        HitCount              = demoPlayer.HitCount,
                        HeadshotCount         = demoPlayer.HeadshotCount,
                        DeathCount            = demoPlayer.DeathCount,
                        AssistCount           = demoPlayer.AssistCount,
                        EntryKillCount        = demoPlayer.EntryKills.Count,
                        MvpCount              = demoPlayer.RoundMvpCount,
                        //KnifeKillCount        = demoPlayer.Kill,
                        TeamKillCount         = demoPlayer.TeamKillCount,
                        //DamageHealthCount     = demoPlayer.DamageHealthCount,
                        //DamageArmorCount      = demoPlayer.DamageArmorCount,
                        RoundPlayedCount      = demoPlayer.RoundPlayedCount,
                        AverageHltvRating     = demoPlayer.RatingHltv,
                        AverageEseaRws        = demoPlayer.EseaRws,
                        OneKillCount          = demoPlayer.OneKillCount,
                        TwoKillCount          = demoPlayer.TwoKillCount,
                        ThreeKillCount        = demoPlayer.ThreeKillCount,
                        FourKillCount         = demoPlayer.FourKillCount,
                        FiveKillCount         = demoPlayer.FiveKillCount,
                        //ClutchCount           = demoPlayer.Clu,
                        //ClutchWonCount        = ,
                        //ClutchLostCount       = ,
                    });
            }
        }

        public List<Demo> QueryDemosOfUser(long steamId)
        {
            var cnn = GetConnection();
            var result = cnn.Query(
                @"SELECT Demo
        FROM PlayerInDemo
        WHERE SteamId = @steamId", new {steamId});

            var ret = new List<Demo>();
            foreach (var demo in result)
            {
                ret.Add(GetDemo(demo.Demo));
            }

            return ret;
        }

        public Demo GetDemo(string id)
        {
            var cnn = GetConnection();
            var result = cnn.Query(
                @"SELECT Name, Date, Source, ClientName, HostName, IsGOTV, DemoTickrate, HostTickrate, Duration, Ticks, MapName,
          FileRef, Comment, WatchStatus, CTStartingTeamFirstHalfScore, TStartingTeamFirstHalfScore, CTStartingTeamSecondHalfScore, 
          TStartingTeamSecondHalfScore, CTStartingTeamName, TStartingTeamName, SurrendingTeam, WinningTeam
        FROM Demo
        WHERE Id = @id", new { id }).FirstOrDefault();

            if (result == null)
            {
                return null;
            }
            

            var fileRefResult = cnn.Query(
                @"SELECT Path
        FROM FileRef
        WHERE Id = @id", new { id = result.FileRef }).FirstOrDefault();

            var demo = new Demo
            {
                Id = id,
                Name = result.Name,
                Date = result.Date,
                SourceName = result.Source,
                ClientName = result.ClientName,
                Hostname = result.HostName,
                Type = result.IsGOTV == 1 ? "GOTV" : "POV",
                Tickrate = (float)result.DemoTickrate,
                ServerTickrate = (float)result.HostTickrate,
                Duration = (float)result.Duration,
                Ticks = (int)result.Ticks,
                MapName = result.MapName,
                Path = fileRefResult.Path,
                Comment = result.Comment,
                Status = ToStatus(result.WatchStatus),
            };

            var ctTeam = demo.TeamCT;
            ctTeam.ScoreFirstHalf = (int)result.CTStartingTeamFirstHalfScore;
            ctTeam.ScoreSecondHalf = (int)result.CTStartingTeamSecondHalfScore;
            ctTeam.Name = result.CTStartingTeamName;
            var tTeam = demo.TeamT;
            tTeam.ScoreFirstHalf = (int)result.TStartingTeamFirstHalfScore;
            tTeam.ScoreSecondHalf = (int)result.TStartingTeamSecondHalfScore;
            tTeam.Name = result.TStartingTeamName;

            demo.Surrender =
                result.SurrendingTeam == null ? null : (result.SurrendingTeam == 0 ? ctTeam : tTeam);
            demo.Winner =
                result.WinningTeam == null ? null : (result.WinningTeam == 0 ? ctTeam : tTeam);


            if (demo.SourceName != null)
            {
                demo.Source = Source.Factory(demo.SourceName);
            }

            var demoPlayers = cnn.Query(
                @"SELECT SteamId, Demo, StartingTeam, Name, KillCount, FlashbangThrownCount, SmokeThrownCount, HeThrownCount, MolotovThrownCount, IncendiaryThrownCount, DecoyThrownCount,
                 TradeKillCount, BombPlantedCount,BombDefusedCount, BombExplodedCount, CrouchKillCount, JumpKillCount, HitCount,
                 HeadshotCount, DeathCount, AssistCount, EntryKillCount, MvpCount, TeamKillCount, RoundPlayedCount,
                 AverageHltvRating, AverageEseaRws, OneKillCount, TwoKillCount, ThreeKillCount, FourKillCount, FiveKillCount
        FROM PlayerInDemo
        WHERE Demo = @id", new { id });
            foreach (var demoPlayer in demoPlayers)
            {
                var team = demoPlayer.StartingTeam == 0 ? ctTeam : tTeam;
                var player = new Player()
                {
                    SteamId = demoPlayer.SteamId,
                    Name = demoPlayer.Name,
                    TeamName = team.Name,
                    KillCount = (int)demoPlayer.KillCount,
                    FlashbangThrownCount = (int)demoPlayer.FlashbangThrownCount,
                    SmokeThrownCount = (int)demoPlayer.SmokeThrownCount,
                    HeGrenadeThrownCount = (int)demoPlayer.HeThrownCount,
                    MolotovThrownCount = (int)demoPlayer.MolotovThrownCount,
                    IncendiaryThrownCount = (int)demoPlayer.IncendiaryThrownCount,
                    DecoyThrownCount = (int)demoPlayer.DecoyThrownCount,
                    TradeKillCount = (int)demoPlayer.TradeKillCount,
                    BombPlantedCount = (int)demoPlayer.BombPlantedCount,
                    BombDefusedCount = (int)demoPlayer.BombDefusedCount,
                    BombExplodedCount = (int)demoPlayer.BombExplodedCount,
                    CrouchKillCount = (int)demoPlayer.CrouchKillCount,
                    JumpKillCount = (int)demoPlayer.JumpKillCount,
                    //WeaponFiredCount      = demoPlayer,
                    HitCount = (int)demoPlayer.HitCount,
                    HeadshotCount = (int)demoPlayer.HeadshotCount,
                    DeathCount = (int)demoPlayer.DeathCount,
                    AssistCount = (int)demoPlayer.AssistCount,
                    //EntryKillCount = demoPlayer.EntryKills.Count,
                    RoundMvpCount = (int)demoPlayer.MvpCount,
                    //KnifeKillCount        = demoPlayer.Kill,
                    TeamKillCount = (int)demoPlayer.TeamKillCount,
                    //TotalDamageHealthCount = demoPlayer.DamageHealthCount,
                    //TotalDamageArmorCount = demoPlayer.DamageArmorCount,

                    RoundPlayedCount = (int)demoPlayer.RoundPlayedCount,
                    //AverageHealthDamage = demoPlayer.AverageHealthDamage,
                    RatingHltv = (float)demoPlayer.AverageHltvRating,
                    EseaRws = (decimal)demoPlayer.AverageEseaRws,
                    OneKillCount = (int) demoPlayer.OneKillCount,
                    TwoKillCount = (int)demoPlayer.TwoKillCount,
                    ThreeKillCount = (int)demoPlayer.ThreeKillCount,
                    FourKillCount = (int)demoPlayer.FourKillCount,
                    FiveKillCount = (int)demoPlayer.FiveKillCount,
                };
                team.Players.Add(player);

            }

            return demo;
        }

        protected override void InitDatabase()
        {
            base.InitDatabase();
            var cnn = GetConnection();
            cnn.Execute(
            @"create table IF NOT EXISTS FileRef
            (
                ID                                  INTEGER primary key AUTOINCREMENT,
                Path                                TEXT not null,
                LastChange                          datetime not null,
                Sha256Hash                          BLOB
            );
            CREATE UNIQUE INDEX idx_fileref_path
            ON FileRef (Path);");
            cnn.Execute(
                @"create table IF NOT EXISTS Player
            (
                Id                                  INTEGER primary key AUTOINCREMENT,
                SteamId                             INTEGER,
                PrimaryAccount                      INTEGER,
                Note                                TEXT,
                LastChange                          datetime not null,
                Sha256Hash                          BLOB
                
            );
            CREATE UNIQUE INDEX IF NOT EXISTS idx_player_steamid 
            ON Player (SteamId);");
            cnn.Execute(
                @"create table IF NOT EXISTS PlayerInDemo
            (
                SteamId                             INTEGER not null,
                Demo                                TEXT not null,
                StartingTeam                        INTEGER not null, -- 0 -> CT, 1 -> T
                Name                                TEXT not null, -- Name in Match
                KillCount                           INTEGER,
                FlashbangThrownCount                INTEGER,
                SmokeThrownCount                    INTEGER,
                HeThrownCount                       INTEGER,
                MolotovThrownCount                  INTEGER,
                IncendiaryThrownCount               INTEGER,
                DecoyThrownCount                    INTEGER,
                TradeKillCount                      INTEGER,
                BombPlantedCount                    INTEGER,
                BombDefusedCount                    INTEGER,
                BombExplodedCount                   INTEGER,
                CrouchKillCount                     INTEGER,
                JumpKillCount                       INTEGER,
                --WeaponFiredCount                    INTEGER,
                HitCount                            INTEGER,
                HeadshotCount                       INTEGER,
                DeathCount                          INTEGER,
                AssistCount                         INTEGER,
                EntryKillCount                      INTEGER,
                MvpCount                            INTEGER,
                --KnifeKillCount                      INTEGER,
                TeamKillCount                       INTEGER,
                --DamageHealthCount                   INTEGER,
                --DamageArmorCount                    INTEGER,
                RoundPlayedCount                    INTEGER,
                --AverageHealthDamage                 REAL,
                AverageHltvRating                   REAL,
                AverageEseaRws                      REAL,
                OneKillCount                        INTEGER,
                TwoKillCount                        INTEGER,
                ThreeKillCount                      INTEGER,
                FourKillCount                       INTEGER,
                FiveKillCount                       INTEGER
                --ClutchCount                         INTEGER,
                --ClutchWonCount                      INTEGER,
                --ClutchLostCount                     INTEGER
            );
            CREATE INDEX IF NOT EXISTS idx_demo_steamid 
            ON PlayerInDemo (SteamId);
            CREATE INDEX IF NOT EXISTS idx_player_demo
            ON PlayerInDemo (Demo);");
            cnn.Execute(
               @"create table OverTimeDemo
            (
                Demo                                TEXT,
                Number                              INTEGER not null,
                CTStartingTeamScore                 INTEGER not null,
                TStartingTeamScore                  INTEGER not null
            )");
            cnn.Execute(
                @"create table DemoChatMessages
            (
                Demo                                TEXT,
                Message                             TEXT not null,
                SteamId                             INTEGER
            )");
            cnn.Execute(
            @"create table Demo
            (
                ID                                  TEXT identity primary key,
                Name                                TEXT,
                Date                                datetime not null,
                Source                              TEXT,
                ClientName                          TEXT not null,
                HostName                            TEXT not null,
                IsGOTV                              INTEGER not null, -- 0=false, 1=true
                DemoTickrate                        REAL not null,
                HostTickrate                        REAL not null,
                Duration                            REAL not null,
                Ticks                               INTEGER not null,
                MapName                             TEXT not null,
                FileRef                             INTEGER not null,
                Comment                             TEXT,
                WatchStatus                         INTEGER not null, -- 0=None, 1=to watch, 2=watched 
                CTStartingTeamFirstHalfScore        INTEGER not null,
                TStartingTeamFirstHalfScore         INTEGER not null,
                CTStartingTeamSecondHalfScore       INTEGER not null,
                TStartingTeamSecondHalfScore        INTEGER not null,
                CTStartingTeamName                  TEXT,
                TStartingTeamName                   TEXT,
                SurrendingTeam                      INTEGER, -- 0 -> CT, 1 -> T, NULL -> none
                WinningTeam                         INTEGER -- 0 -> CT, 1 -> T, NULL -> none
            )");
        }

        public void Init()
        {
            InitDatabase();
        }
    }
}
