using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Date = System.DateTime;
using Duration = System.Single;

namespace Data
{
    public class Settings
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("selected_account")]
        public ObjectId SelectedAccount { get; set; }

        [BsonElement("replay_folders")]
        public List<string> ReplayFolders { get; set; }
    }

    public class AccountRef {
        [BsonElement("account_id")]
        public string AccountId { get; set; }
    }
    public class MultiAccount
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("linked_accounts")]
        public List<AccountRef> LinkedAccounts { get; set; }
    }

    public class Account
    {
        [BsonId]
        [BsonElement("steam_id")]
        public string SteamId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("banned_date")]
        public Date BannedDate { get; set; }
        [BsonElement("cheater")]
        public bool Cheater { get; set; }
    }

    public enum Source {
        MatchMaking,
        Other
    }

    public enum Team {
        CT_Starting,
        T_Starting
    }

    public class DemoComment
    {
        [BsonElement("account")]
        public ObjectId Account { get; set; }
        [BsonElement("text")]
        public string Text { get; set; }
    }

    public class DemoTeamPlayer
    {
        [BsonElement("account_id")]
        public string AccountId { get; set; }
    }

    public class DemoTeam 
    {
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("score")]
        public int Score { get; set; }
        [BsonElement("score_first_half")]
        public int ScoreFirstHalf { get; set; }
        [BsonElement("score_second_half")]
        public int ScoreSecondHalf { get; set; }
        [BsonElement("players")]
        public List<DemoTeamPlayer> Players { get; set; }
    }

    public class DemoRound
    {
        [BsonElement("number")]
        public int Number { get; set; }
        [BsonElement("start_tick")]
        public int StartTick { get; set; }
    }

    public class Demo
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("data_version")]
        public int DataVersion { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("local_file_path")]
        public string LocalFilePath { get; set; }

        [BsonElement("date")]
        public Date Date { get; set; }

        [BsonElement("source")]
        public Source Source { get; set; }

        [BsonElement("hostname")]
        public string Hostname { get; set; }

        [BsonElement("demo_tickrate")]
        public float DemoTickRate { get; set; }

        [BsonElement("server_tickrate")]
        public float ServerTickRate { get; set; }

        [BsonElement("duration")]
        public Duration Duration { get; set; }
        [BsonElement("ticks")]
        public int Ticks { get; set; }
        [BsonElement("map")]
        public string Map { get; set; }
        [BsonElement("winning_team")]
        public Team WinningTeam { get; set; }
        [BsonElement("surrendered")]
        public bool Surrendered { get; set; }
        [BsonElement("comments")]
        public List<DemoComment> Comments { get; set; }

        
        [BsonElement("ct_start_team")]
        public DemoTeam CTStartTeam { get; set; }
        [BsonElement("t_start_team")]
        public DemoTeam TStartTeam { get; set; }
        [BsonElement("rounds")]
        public List<DemoRound> Rounds { get; set; }
    }
}
