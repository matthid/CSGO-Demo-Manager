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
        

        public void SaveDemo(Demo demo)
        {
            var cnn = GetConnection();
            DateTime lastAccess = DateTime.MinValue;
            string sha256 = null;
            if (File.Exists(demo.Path))
            {
                lastAccess = File.GetLastWriteTimeUtc(demo.Path);
                sha256 = SHA256CheckSum(demo.Path);
            }

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
                    WatchStatus = demo.Status == "None" ? 0 : throw new InvalidOperationException($"Unknown status '{demo.Status}'"),
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

            var ctTeam = new Team();
            ctTeam.ScoreFirstHalf = (int)result.CTStartingTeamFirstHalfScore;
            ctTeam.ScoreSecondHalf = (int)result.CTStartingTeamSecondHalfScore;
            ctTeam.Name = result.CTStartingTeamName;
            var tTeam = new Team();
            tTeam.ScoreFirstHalf = (int)result.TStartingTeamFirstHalfScore;
            tTeam.ScoreSecondHalf = (int)result.TStartingTeamSecondHalfScore;
            tTeam.Name = result.TStartingTeamName;

            var surrendingTeam =
                result.SurrendingTeam == null ? null : (result.SurrendingTeam == 0 ? ctTeam : tTeam);
            var winningTeam =
                result.WinningTeam == null ? null : (result.WinningTeam == 0 ? ctTeam : tTeam);

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
                Status = result.WatchStatus == 0 ? "None" : throw new InvalidOperationException($"Unknown watch status '{result.WatchStatus}' in database."),
                TeamCT = ctTeam,
                TeamT = tTeam,
                Winner = winningTeam,
                Surrender = surrendingTeam,
            };

            if (demo.SourceName != null)
            {
                demo.Source = Source.Factory(demo.SourceName);
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
                KillCount                           INTEGER,
                ClutchCount                         INTEGER,
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
                WeaponFiredCount                    INTEGER,
                HitCount                            INTEGER,
                ClutchWonCount                      INTEGER,
                ClutchLostCount                     INTEGER,
                HeadshotCount                       INTEGER,
                DeathCount                          INTEGER,
                AssistCount                         INTEGER,
                EntryKillCount                      INTEGER,
                MvpCount                            INTEGER,
                KnifeKillCount                      INTEGER,
                TeamKillCount                       INTEGER,
                DamageHealthCount                   INTEGER,
                DamageArmorCount                    INTEGER,
                KillPerRound                        REAL,
                AssistPerRound                      REAL,
                DeathPerRound                       REAL,
                AverageHealthDamage                 REAL,
                AaverageHltvRating                  REAL,
                AverageEseaRws                      REAL,
                OneKillCount                        INTEGER,
                TwoKillCount                        INTEGER,
                ThreeKillCount                      INTEGER,
                FourKillCount                       INTEGER,
                FiveKillCount                       INTEGER
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
