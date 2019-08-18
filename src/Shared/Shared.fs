namespace Shared

type SteamId = string
type FloatNum = float
type DecimalNum = float

type BaseEvent = {
    Tick : int // [JsonProperty("tick")] public int Tick { get; set; }
    Seconds: FloatNum // [JsonProperty("seconds")] public float Seconds { get; set; }
}

type Side =
    | NoneSide
    | Spectate
    | Terrorist 
    | CounterTerrorist
type WeaponType =
    | Pistol
    | Rifle
    | Sniper
    | SMG
    | Heavy
    | Equipment
    | Grenade
    | Unkown
type EquipmentElement =
    | Unknown

    //Pistoles
    | P2000
    | Glock
    | P250
    | Deagle 
    | FiveSeven
    | DualBarettas
    | Tec9
    | CZ
    | USP
    | Revolver 

    //SMGs
    | MP7
    | MP9
    | Bizon
    | Mac10
    | UMP
    | P90
    | MP5SD

    //Heavy
    | SawedOff
    | Nova
    | Swag7
    | XM1014
    | M249
    | Negev

    //Rifle
    | Gallil
    | Famas
    | AK47
    | M4A4
    | M4A1
    | Scout
    | SG556
    | AUG
    | AWP
    | Scar20
    | G3SG1

    //Equipment
    | Zeus
    | Kevlar
    | Helmet
    | Bomb
    | Knife
    | DefuseKit
    | World

    //Grenades
    | Decoy
    | Molotov
    | Incendiary
    | Flash
    | Smoke
    | HE

type Weapon = {
    Element : EquipmentElement // [JsonProperty("element")] public EquipmentElement Element { get; set; }
    Type : WeaponType // [JsonProperty("type")] public WeaponType Type { get; set; }
    Name : string // [JsonProperty("weapon_name")] public string Name { get; set; }
    KillAward : int // [JsonIgnore] public int KillAward { get; set; }
}

type EntryKillEvent = {
    // BaseEvent
    Tick : int // [JsonProperty("tick")] public int Tick { get; set; }
    Seconds: FloatNum // [JsonProperty("seconds")] public float Seconds { get; set; }

    // Others
    RoundNumber: int // [JsonProperty("round_number")] public int RoundNumber { get; set; }
    KillerSteamId : SteamId // [JsonProperty("killer_steamid")] [JsonConverter(typeof(LongToStringConverter))] public long KillerSteamId { get; set; }
    KillerName : string // [JsonProperty("killer_name")] public string KillerName { get; set; }
    KillerSide : Side // [JsonProperty("killer_side")] [JsonConverter(typeof(SideToStringConverter))] public Side KillerSide { get; set; }
    KilledSteamId : SteamId // [JsonProperty("killed_steamid")] [JsonConverter(typeof(LongToStringConverter))] public long KilledSteamId { get; set; }
    KilledName:string //[JsonProperty("killed_name")] public string KilledName { get; set; }
    KilledSide : Side // [JsonProperty("killed_side")][JsonConverter(typeof(SideToStringConverter))] public Side KilledSide { get; set; }
    Weapon : Weapon // [JsonProperty("weapon")] public Weapon Weapon { get; set; }
    HasWon : bool // [JsonProperty("has_won")] public bool HasWon { get; set; }
    HasWonRound : bool // [JsonProperty("has_won_round")] public bool HasWonRound { get; set; }
}

type EntryHoldKillEvent = {
    // BaseEvent
    Tick : int // [JsonProperty("tick")] public int Tick { get; set; }
    Seconds: FloatNum // [JsonProperty("seconds")] public float Seconds { get; set; }

    // Others
    RoundNumber: int // [JsonProperty("round_number")] public int RoundNumber { get; set; }
    KillerSteamId : SteamId // [JsonProperty("killer_steamid")] [JsonConverter(typeof(LongToStringConverter))] public long KillerSteamId { get; set; }
    KillerName : string // [JsonProperty("killer_name")] public string KillerName { get; set; }
    KillerSide: Side // [JsonProperty("killer_side")][JsonConverter(typeof(SideToStringConverter))] public Side KillerSide { get; set; }
    KilledSteamId : SteamId // [JsonProperty("killed_steamid")][JsonConverter(typeof(LongToStringConverter))]  public long KilledSteamId { get; set; }
    KilledName : string // [JsonProperty("killed_name")] public string KilledName { get; set; }
    KilledSide : Side // [JsonProperty("killed_side")] [JsonConverter(typeof(SideToStringConverter))] public Side KilledSide { get; set; }
    Weapon : Weapon // [JsonProperty("weapon")] public Weapon Weapon { get; set; }
    HasWon : bool // [JsonProperty("has_won")] public bool HasWon { get; set; }
    HasWonRound : bool // [JsonProperty("has_won_round")] public bool HasWonRound { get; set; }
}

type KillEvent = {
    // BaseEvent
    Tick : int // [JsonProperty("tick")] public int Tick { get; set; }
    Seconds: FloatNum // [JsonProperty("seconds")] public float Seconds { get; set; }
    
    // Others
    KillerSteamId: SteamId // [JsonProperty("killer_steamid")] [JsonConverter(typeof(LongToStringConverter))] public long KillerSteamId { get; set; }
    KilledSteamId : SteamId // [JsonProperty("killed_steamid")][JsonConverter(typeof(LongToStringConverter))] public long KilledSteamId { get; set; }
    AssisterSteamId : SteamId option // [JsonProperty("assister_steamid")] [JsonConverter(typeof(LongToStringConverter))] public long AssisterSteamId { get; set; }
    Weapon : Weapon // [JsonProperty("weapon")] public Weapon Weapon { get; set; }
    //[JsonProperty("heatmap_point")] public KillHeatmapPoint Point { get; set; }
    //[JsonProperty("killer_vel_x")] public float KillerVelocityX { get; set; }
    //[JsonProperty("killer_vel_y")] public float KillerVelocityY { get; set; }
    //[JsonProperty("killer_vel_z")] public float KillerVelocityZ { get; set; }
    KillerSide : Side // [JsonProperty("killer_side")] [JsonConverter(typeof(SideToStringConverter))] public Side KillerSide { get; set; }
    KillerTeam : string option // [JsonProperty("killer_team")] public string KillerTeam { get; set; }
    KilledSide : Side // [JsonProperty("killed_side")] [JsonConverter(typeof(SideToStringConverter))] public Side KilledSide { get; set; }
    KilledTeam : string option // [JsonProperty("killed_team")] public string KilledTeam { get; set; }
    KillerName : string // [JsonProperty("killer_name")] public string KillerName { get; set; }
    KilledName : string // [JsonProperty("killed_name")] public string KilledName { get; set; }
    AssisterName : string option // [JsonProperty("assister_name")] public string AssisterName { get; set; }
    RoundNumber : int // [JsonProperty("round_number")] public int RoundNumber { get; set; }
    /// <summary>
    /// Number of seconds elapsed since the freezetime end
    /// </summary>
    TimeDeathSeconds : FloatNum // [JsonProperty("time_death_seconds")] public float TimeDeathSeconds { get; set; }
    IsKillerCrouching : bool // [JsonProperty("killer_crouching")] public bool IsKillerCrouching { get; set; }
    KillerIsBlinded : bool // [JsonProperty("killer_blinded")] public bool KillerIsBlinded { get; set; }
    IsTradeKill : bool // [JsonProperty("is_trade_kill")] public bool IsTradeKill { get; set; }
    IsHeadshot : bool // [JsonProperty("is_headshot")] public bool IsHeadshot { get; set; }
    KillerIsControllingBot : bool // [JsonProperty("killer_is_controlling_bot")] public bool KillerIsControllingBot { get; set; }
    KilledIsControllingBot : bool // [JsonProperty("killed_is_controlling_bot")] public bool KilledIsControllingBot { get; set; }
    VictimIsBlinded : bool // [JsonProperty("victim_blinded")] public bool VictimIsBlinded { get; set; }
    AssisterIsControllingBot : bool // [JsonProperty("assister_is_controlling_bot")] public bool AssisterIsControllingBot { get; set; }
}

type ClutchEvent = {
    OpponentCount : int // [JsonProperty("opponent_count")] public int OpponentCount { get; set; }
    HasWon : bool // [JsonProperty("has_won")] public bool HasWon { get; set; }
    RoundNumber : int // [JsonProperty("round_number")] public int RoundNumber { get; set; }
}
    
type Player = {
    SteamId : SteamId  // [JsonProperty("steamid")][JsonConverter(typeof(LongToStringConverter))] public long SteamId

    Name : string // [JsonProperty("name")] public string Name

    KillCount : int // [JsonProperty("kill_count")] public int KillCount

    CrouchKillCount : int // [JsonProperty("crouch_kill_count")] public int CrouchKillCount

    JumpKillCount : int // [JsonProperty("jump_kill_count")] public int JumpKillCount

    Score : int // [JsonProperty("score")] public int Score

    TeamKillCount : int // [JsonProperty("tk_count")] public int TeamKillCount

    AssistCount : int // [JsonProperty("assist_count")] public int AssistCount

    TradeKillCount : int // [JsonProperty("trade_kill_count")] public int TradeKillCount

    TradeDeathCount : int // [JsonProperty("trade_death_count")] public int TradeDeathCount

    BombPlantedCount : int // [JsonProperty("bomb_planted_count")] public int BombPlantedCount

    BombDefusedCount : int // [JsonProperty("bomb_defused_count")] public int BombDefusedCount

    BombExplodedCount : int // [JsonProperty("bomb_exploded_count")] public int BombExplodedCount

    DeathCount : int // [JsonProperty("death_count")] public int DeathCount

    FiveKillCount : int // [JsonProperty("5k_count")] public int FiveKillCount

    FourKillCount : int // [JsonProperty("4k_count")] public int FourKillCount

    ThreeKillCount : int // [JsonProperty("3k_count")] public int ThreeKillCount

    TwoKillCount : int // [JsonProperty("2k_count")] public int TwoKillCount

    OneKillCount : int // [JsonProperty("1k_count")] public int OneKillCount

    HeadshotCount : int // [JsonProperty("hs_count")] public int HeadshotCount

    KillDeathRatio : DecimalNum // [JsonProperty("kd")] public decimal KillDeathRatio

    RoundMvpCount : int // [JsonProperty("mvp_count")] public int RoundMvpCount

    RatingHltv : FloatNum // [JsonProperty("hltv_rating")] public float RatingHltv

    EseaRwsPointCount : DecimalNum // [JsonIgnore] public decimal EseaRwsPointCount

    EseaRws : DecimalNum // [JsonProperty("esea_rws")] public decimal EseaRws

    ShotCount : int // [JsonProperty("shot_count")] public int ShotCount

    HitCount : int // [JsonProperty("hit_count")] public int HitCount

    Accuracy : double //[JsonIgnore] public double Accuracy => ShotCount == 0 ? 0 : Math.Round(HitCount / (double)ShotCount * 100, 2);

    IsVacBanned : bool // [JsonProperty("is_vac_banned")] public bool IsVacBanned

    IsOverwatchBanned : bool // [JsonProperty("is_ow_banned")] public bool IsOverwatchBanned

    FlashbangThrownCount : int // [JsonProperty("flashbang_count")] public int FlashbangThrownCount

    SmokeThrownCount : int // [JsonProperty("smoke_count")] public int SmokeThrownCount

    HeGrenadeThrownCount : int // [JsonProperty("he_count")] public int HeGrenadeThrownCount

    MolotovThrownCount : int // [JsonProperty("molotov_count")] public int MolotovThrownCount

    IncendiaryThrownCount : int // [JsonProperty("incendiary_count")] public int IncendiaryThrownCount

    DecoyThrownCount : int // [JsonProperty("decoy_count")] public int DecoyThrownCount

    RoundPlayedCount : int // [JsonProperty("round_count")] public int RoundPlayedCount

    TeamName : string // [JsonProperty("team_name")] public string TeamName

    StartMoneyRounds : Map<int, int> //[JsonProperty("start_money_rounds")] public Dictionary<int, int> StartMoneyRounds

    EquipementValueRounds : Map<int, int> // [JsonProperty("equipement_value_rounds")] public Dictionary<int, int> EquipementValueRounds

    RoundsMoneyEarned : Map<int, int> // [JsonProperty("rounds_money_earned")] public Dictionary<int, int> RoundsMoneyEarned

    TimeDeathRounds : Map<int, FloatNum> // [JsonProperty("time_death_rounds")] public Dictionary<int, float> TimeDeathRounds

    EntryKills : EntryKillEvent list // [JsonProperty("entry_kills")] public ObservableCollection<EntryKillEvent> EntryKills

    EntryHoldKills : EntryHoldKillEvent list // [JsonProperty("entry_hold_kills")] public ObservableCollection<EntryHoldKillEvent> EntryHoldKills

    Kills : KillEvent list // [JsonProperty("kills", IsReference = false)] public ObservableCollection<KillEvent> Kills

    Deaths : KillEvent list // [JsonProperty("deaths", IsReference = false)] public ObservableCollection<KillEvent> Deaths

    Assists : KillEvent list // [JsonProperty("assits", IsReference = false)] public ObservableCollection<KillEvent> Assists

    // [JsonProperty("players_hurted")] public ObservableCollection<PlayerHurtedEvent> PlayersHurted

    Clutches : ClutchEvent list // [JsonProperty("clutches")] public ObservableCollection<ClutchEvent> Clutches

    //[JsonProperty("rank_old")] public int RankNumberOld

    //[JsonProperty("rank_new")] public int RankNumberNew

    //[JsonProperty("win_count")] public int WinCount

    //[JsonProperty("entry_kill_won_count")] public int EntryKillWonCount // get { return EntryKills.Count(e => e.HasWon); }

    //[JsonProperty("entry_kill_loss_count")] public int EntryKillLossCount // get { return EntryKills.Count(e => e.HasWon == false); }

    //[JsonProperty("entry_hold_kill_won_count")] public int EntryHoldKillWonCount // get { return EntryHoldKills.Count(e => e.HasWon); }

    //[JsonProperty("entry_hold_kill_loss_count")] public int EntryHoldKillLossCount // get { return EntryHoldKills.Count(e => e.HasWon == false); }

    //[JsonIgnore] public string AvatarUrl

    //[JsonProperty("clutch_count")] public int ClutchCount => Clutches.Count;

    //[JsonProperty("clutch_loss_count")] public int ClutchLostCount => Clutches.Count(c => !c.HasWon);

    //[JsonProperty("clutch_won_count")] public int ClutchWonCount => Clutches.Count(c => c.HasWon);

    //[JsonProperty("1v1_won_count")] public int Clutch1V1WonCount => Clutches.Count(c => c.OpponentCount == 1 && c.HasWon);

    //[JsonProperty("1v2_won_count")] public int Clutch1V2WonCount => Clutches.Count(c => c.OpponentCount == 2 && c.HasWon);

    //[JsonProperty("1v3_won_count")] public int Clutch1V3WonCount => Clutches.Count(c => c.OpponentCount == 3 && c.HasWon);

    //[JsonProperty("1v4_won_count")] public int Clutch1V4WonCount => Clutches.Count(c => c.OpponentCount == 4 && c.HasWon);

    //[JsonProperty("1v5_won_count")] public int Clutch1V5WonCount => Clutches.Count(c => c.OpponentCount == 5 && c.HasWon);

    //[JsonProperty("1v1_loss_count")] public int Clutch1V1LossCount => Clutches.Count(c => c.OpponentCount == 1 && !c.HasWon);

    //[JsonProperty("1v2_loss_count")] public int Clutch1V2LossCount => Clutches.Count(c => c.OpponentCount == 2 && !c.HasWon);

    //[JsonProperty("1v3_loss_count")] public int Clutch1V3LossCount => Clutches.Count(c => c.OpponentCount == 3 && !c.HasWon);

    //[JsonProperty("1v4_loss_count")] public int Clutch1V4LossCount => Clutches.Count(c => c.OpponentCount == 4 && !c.HasWon);

    //[JsonProperty("1v5_loss_count")] public int Clutch1V5LossCount => Clutches.Count(c => c.OpponentCount == 5 && !c.HasWon);

    //[JsonProperty("1v1_count")] public int Clutch1V1Count => Clutches.Count(c => c.OpponentCount == 1);

    //[JsonProperty("1v2_count")] public int Clutch1V2Count => Clutches.Count(c => c.OpponentCount == 2);

    //[JsonProperty("1v3_count")] public int Clutch1V3Count => Clutches.Count(c => c.OpponentCount == 3);

    //[JsonProperty("1v4_count")] public int Clutch1V4Count => Clutches.Count(c => c.OpponentCount == 4);

    //[JsonProperty("1v5_count")] public int Clutch1V5Count => Clutches.Count(c => c.OpponentCount == 5);

    //[JsonIgnore] public decimal ClutchWonPercent => ClutchCount == 0 ? 0 : Math.Round((decimal)(ClutchWonCount * 100) / ClutchCount, 2);

    //[JsonIgnore] public decimal Clutch1V1WonPercent => Clutch1V1Count == 0 ? 0 : Math.Round((decimal)(Clutch1V1WonCount * 100) / Clutch1V1Count, 2);

    //[JsonIgnore] public decimal Clutch1V2WonPercent => Clutch1V2Count == 0 ? 0 : Math.Round((decimal)(Clutch1V2WonCount * 100) / Clutch1V2Count, 2);

    //[JsonIgnore] public decimal Clutch1V3WonPercent => Clutch1V3Count == 0 ? 0 : Math.Round((decimal)(Clutch1V3WonCount * 100) / Clutch1V3Count, 2);

    //[JsonIgnore] public decimal Clutch1V4WonPercent => Clutch1V4Count == 0 ? 0 : Math.Round((decimal)(Clutch1V4WonCount * 100) / Clutch1V4Count, 2);

    //[JsonIgnore] public decimal Clutch1V5WonPercent => Clutch1V5Count == 0 ? 0 : Math.Round((decimal)(Clutch1V5WonCount * 100) / Clutch1V5Count, 2);

    SuicideCount : int //[JsonProperty("suicide_count")] public int SuicideCount { get; set; }

    /// <summary>
    /// Total health damage made by the player
    /// </summary>
    //[JsonProperty("total_damage_health_done")] //public int TotalDamageHealthCount => PlayersHurted.ToList()
    //    .Where(playerHurtedEvent => playerHurtedEvent != null && playerHurtedEvent.AttackerSteamId == SteamId)
    //    .Sum(playerHurtedEvent => playerHurtedEvent.HealthDamage);

    /// <summary>
    /// Total armor damage made by the player
    /// </summary>
    //[JsonProperty("total_damage_armor_done")]//public int TotalDamageArmorCount => PlayersHurted.ToList()
    //    .Where(playerHurtedEvent => playerHurtedEvent != null && playerHurtedEvent.AttackerSteamId == SteamId)
    //    .Sum(playerHurtedEvent => playerHurtedEvent.ArmorDamage);

    /// <summary>
    /// Total health damage the player has received
    /// </summary>
    //[JsonProperty("total_damage_health_received")]//public int TotalDamageHealthReceivedCount => PlayersHurted.ToList()
    //    .Where(playerHurtedEvent => playerHurtedEvent != null && playerHurtedEvent.HurtedSteamId == SteamId)
    //    .Sum(playerHurtedEvent => playerHurtedEvent.HealthDamage);

    /// <summary>
    /// Total armor damage the player has received
    /// </summary>
    //[JsonProperty("total_damage_armor_received")]//public int TotalDamageArmorReceivedCount => PlayersHurted.ToList()
    //    .Where(playerHurtedEvent => playerHurtedEvent != null && playerHurtedEvent.HurtedSteamId == SteamId)
    //    .Sum(playerHurtedEvent => playerHurtedEvent.ArmorDamage);

    /// <summary>
    /// Average health damage the player has done during the match
    /// </summary>
    //[JsonProperty("average_health_damage")]//public double AverageHealthDamage
    //        double total = 0;
    //
    //        if (PlayersHurted.Any())
    //        {
    //            total = PlayersHurted.ToList().Where(playerHurtedEvent => playerHurtedEvent != null && playerHurtedEvent.AttackerSteamId == SteamId)
    //                .Aggregate(total, (current, playerHurtedEvent) => current + playerHurtedEvent.HealthDamage);
    //            if (Math.Abs(total) < 0.1) return total;
    //        }
    //        if (RoundPlayedCount > 0) total = Math.Round(total / RoundPlayedCount, 1);
    //
    //        return total;

    //[JsonIgnore]//public decimal HeadshotPercent
    //        decimal headshotPercent = 0;
    //        if (_headshotCount <= 0) return headshotPercent;
    //        if(_killCount > 0) headshotPercent = (_headshotCount * 100) / (decimal)_killCount;
    //        headshotPercent = Math.Round(headshotPercent, 2);
    //
    //        return headshotPercent;

    //[JsonIgnore]public decimal RatioEntryKill
    //        int entryKillCount = EntryKills.Count();
    //        int entryKillWin = EntryKills.Count(e => e.HasWon);
    //        int entryKillLoss = EntryKills.Count(e => e.HasWon == false);
    //
    //        decimal entryKillPercent = 0;
    //        if (entryKillWin == 0) return entryKillPercent;
    //        if (entryKillLoss == 0) return 100;
    //        entryKillPercent = (entryKillWin / (decimal)entryKillCount) * 100;
    //        entryKillPercent = Math.Round(entryKillPercent, 0);
    //
    //        return entryKillPercent;

    //[JsonIgnore]public decimal RatioEntryHoldKill
    //        int entryHoldKillCount = EntryHoldKills.Count();
    //        int entryHoldKillWin = EntryHoldKills.Count(e => e.HasWon);
    //        int entryHoldKillLoss = EntryHoldKills.Count(e => e.HasWon == false);
    //
    //        decimal entryHoldKillPercent = 0;
    //        if (entryHoldKillWin == 0) return entryHoldKillPercent;
    //        if (entryHoldKillLoss == 0) return 100;
    //        entryHoldKillPercent = entryHoldKillWin / (decimal)entryHoldKillCount * 100;
    //        entryHoldKillPercent = Math.Round(entryHoldKillPercent, 0);
    //
    //        return entryHoldKillPercent;

    //[JsonProperty("kill_per_round")]public double KillPerRound
    //        if (RoundPlayedCount > 0) return Math.Round((double)KillCount / RoundPlayedCount, 2);
    //        return 0;

    //[JsonProperty("assist_per_round")] public double AssistPerRound
    //        if (RoundPlayedCount > 0) return Math.Round((double)AssistCount / RoundPlayedCount, 2);
    //        return 0;

    //[JsonProperty("death_per_round")] public double DeathPerRound
    //        if (RoundPlayedCount > 0) return Math.Round((double)DeathCount / RoundPlayedCount, 2);
    //        return 0;

    //[JsonProperty("total_time_death")]
    //public float TotalTimeDeath => TimeDeathRounds.Sum(kvp => kvp.Value);

    //[JsonProperty("avg_time_death")] //public double AverageTimeDeath
    //        if (RoundPlayedCount > 0) return Math.Round((double)TotalTimeDeath / RoundPlayedCount, 2);
    //        return 0;

    //IsControllingBot : bool // [JsonIgnore] public bool IsControllingBot

    //[JsonIgnore]public Side Side

    //[JsonIgnore]    public bool IsAlive

    //[JsonIgnore]    public bool IsConnected

    //[JsonIgnore]    public float FlashDurationTemp

    //[JsonIgnore]    public bool HasBomb

    //[JsonIgnore]    public bool HasEntryKill

    //[JsonIgnore]    public bool HasEntryHoldKill

    //[JsonIgnore]    public int MatchCount { get; set; } = 1;

}


type Team = {

    Name : string //[JsonProperty("team_name")] public string Name

    Score : int // [JsonProperty("score")] public int Score

    ScoreFirstHalf: int // [JsonProperty("score_first_half")] public int ScoreFirstHalf

    ScoreSecondHalf : int // [JsonProperty("score_second_half")] public int ScoreSecondHalf

    Players : Player list // [JsonProperty("team_players", IsReference = false)] public ObservableCollection<Player> Players { get; set; }

    //[JsonIgnore] public Side CurrentSide { get; set; }

    /// <summary>
    /// Keep track of loss serie to determine money reward at each round
    /// </summary>
    //[JsonIgnore] public int LossRowCount { get; set; }
    //[JsonIgnore] public int EntryHoldKillCount => Players.SelectMany(p => p.EntryHoldKills).Count();
    //[JsonIgnore] public int EntryKillCount => Players.SelectMany(p => p.EntryKills).Count();
    //[JsonIgnore] public int EntryHoldKillWonCount => Players.SelectMany(p => p.EntryHoldKills).Count(e => e.HasWon);
    //[JsonIgnore] public int EntryHoldKillLossCount => Players.SelectMany(p => p.EntryHoldKills).Count(e => e.HasWon == false);
    //[JsonIgnore] public decimal RatioEntryHoldKill
    //        int total = Players.SelectMany(p => p.EntryHoldKills).Count();
    //        int won = Players.SelectMany(p => p.EntryHoldKills).Count(e => e.HasWon);
    //        int loss = Players.SelectMany(p => p.EntryHoldKills).Count(e => e.HasWon == false);
    //
    //        decimal percent = 0;
    //        if (EntryKillWonCount == 0) return percent;
    //        if (loss == 0) return 100;
    //        percent = won / (decimal)total * 100;
    //        percent = Math.Round(percent, 0);
    //
    //        return percent;
    //[JsonIgnore] public int EntryKillWonCount => Players.SelectMany(p => p.EntryKills).Count(e => e.HasWon);
    //[JsonIgnore] public int EntryKillLossCount => Players.SelectMany(p => p.EntryKills).Count(e => e.HasWon == false);
    //[JsonIgnore] public int FlashbangThrownCount => Players.Where(playerExtended => playerExtended.TeamName == Name)
    //    .Sum(playerExtended => playerExtended.FlashbangThrownCount);
    //[JsonIgnore] public int HeGrenadeThrownCount => Players.Where(playerExtended => playerExtended.TeamName == Name)
    //    .Sum(playerExtended => playerExtended.HeGrenadeThrownCount);
    //[JsonIgnore] public int SmokeThrownCount => Players.Where(playerExtended => playerExtended.TeamName == Name)
    //    .Sum(playerExtended => playerExtended.SmokeThrownCount);
    //[JsonIgnore] public int MolotovThrownCount => Players.Where(playerExtended => playerExtended.TeamName == Name)
    //    .Sum(playerExtended => playerExtended.MolotovThrownCount);
    //[JsonIgnore] public int IncendiaryThrownCount => Players.Where(playerExtended => playerExtended.TeamName == Name)
    //    .Sum(playerExtended => playerExtended.IncendiaryThrownCount);
    //[JsonIgnore] public int DecoyThrownCount => Players.Where(playerExtended => playerExtended.TeamName == Name)
    //    .Sum(playerExtended => playerExtended.DecoyThrownCount);
    //[JsonIgnore] public int TradeKillCount => Players.Sum(p => p.TradeKillCount);
    //[JsonIgnore] public int TradeDeathCount => Players.Sum(p => p.TradeDeathCount);
    //[JsonIgnore] public decimal RatioEntryKill
    //        int entryKillCount = Players.SelectMany(p => p.EntryKills).Count();
    //        int entryKillWin = Players.SelectMany(p => p.EntryKills).Count(e => e.HasWon);
    //        int entryKillLoss = Players.SelectMany(p => p.EntryKills).Count(e => e.HasWon == false);
    //
    //        decimal entryKillPercent = 0;
    //        if (EntryKillWonCount == 0) return entryKillPercent;
    //        if (entryKillLoss == 0) return 100;
    //        entryKillPercent = (entryKillWin / (decimal)entryKillCount) * 100;
    //        entryKillPercent = Math.Round(entryKillPercent, 0);
    //
    //        return entryKillPercent;
    //[JsonIgnore] public int MatchCount { get; set; } = 1;
    //[JsonIgnore] public int WinCount { get; set; } = 0;
    //[JsonIgnore] public int LostCount { get; set; } = 0;
    //[JsonIgnore] public int KillCount => Players.Sum(player => player.KillCount);
    //[JsonIgnore] public int AssistCount => Players.Sum(player => player.AssistCount);
    //[JsonIgnore] public int DeathCount => Players.Sum(player => player.DeathCount);
    //[JsonIgnore] public int RoundCount { get; set; } = 0;
    //[JsonIgnore] public int WinRoundCount { get; set; } = 0;
    //[JsonIgnore] public int LostRoundCount { get; set; } = 0;
    //[JsonIgnore] public int WinRoundCtCount { get; set; } = 0;
    //[JsonIgnore] public int LostRoundCtCount { get; set; } = 0;
    //[JsonIgnore] public int WinRoundTCount { get; set; } = 0;
    //[JsonIgnore] public int LostRoundTCount { get; set; } = 0;
    //[JsonIgnore] public int WinPistolRoundCount { get; set; } = 0;
    //[JsonIgnore] public int WinEcoRoundCount { get; set; } = 0;
    //[JsonIgnore] public int WinSemiEcoRoundCount { get; set; } = 0;
    //[JsonIgnore] public int WinForceBuyRoundCount { get; set; } = 0;
    //[JsonIgnore] public int BombPlantedCount => Players.Sum(player => player.BombPlantedCount);
    //[JsonIgnore] public int BombDefusedCount => Players.Sum(player => player.BombDefusedCount);
    //[JsonIgnore] public int BombExplodedCount => Players.Sum(player => player.BombExplodedCount);
    //[JsonIgnore] public int BombPlantedOnACount { get; set; } = 0;
    //[JsonIgnore] public int BombPlantedOnBCount { get; set; } = 0;
    //[JsonIgnore] public int FiveKillCount => Players.Sum(player => player.FiveKillCount);
    //[JsonIgnore] public int FourKillCount => Players.Sum(player => player.FourKillCount);
    //[JsonIgnore] public int ThreeKillCount => Players.Sum(player => player.ThreeKillCount);
    //[JsonIgnore] public int TwoKillCount => Players.Sum(player => player.TwoKillCount);
    //[JsonIgnore] public int OneKillCount => Players.Sum(player => player.OneKillCount);
    //[JsonIgnore] public int JumpKillCount => Players.Sum(player => player.JumpKillCount);
    //[JsonIgnore] public int CrouchKillCount => Players.Sum(player => player.CrouchKillCount);
}

type Demo = {
    Id : string // id
    Name : string // name
    NameWithoutExtension : string //=> System.IO.Path.GetFileNameWithoutExtension(Name);
    Date : System.DateTime // [JsonProperty("date")]
    SourceName : string // [JsonProperty("source")]
    Comment : string // [JsonProperty("comment")]
    Status : string // [JsonProperty("status")]
    ClientName : string // [JsonProperty("client_name")]
    Hostname : string // [JsonProperty("hostname")]
    Type : string // [JsonProperty("type")]
    Tickrate : FloatNum // [JsonProperty("tickrate")]
    ServerTickrate : FloatNum // [JsonProperty("server_tickrate")]
    Duration : FloatNum // [JsonProperty("duration")]
    Ticks : int // [JsonProperty("ticks")]
    MapName : string // [JsonProperty("map_name")]
    Path : string //[JsonProperty("path")]
    // HasCheater : bool // [JsonIgnore] public bool HasCheater => _cheaterCounter > 0;
    CheaterCount : int // [JsonProperty("cheater_count")]
    /// <summary>
    /// Score of the team that started as CT
    /// </summary>
    ScoreTeamCt : int //[JsonProperty("score_team1")] public int ScoreTeamCt => TeamCT.Score;

    /// <summary>
    /// Score of the team that started as T
    /// </summary>
    ScoreTeamT : int // [JsonProperty("score_team2")] public int ScoreTeamT => TeamT.Score;

    /// <summary>
    /// First half score of the team that started as CT
    /// </summary>
    ScoreFirstHalfTeamCt : int // [JsonProperty("score_half1_team1")] public int ScoreFirstHalfTeamCt => TeamCT.ScoreFirstHalf;

    /// <summary>
    /// First half score of the team that started as T
    /// </summary>
    ScoreFirstHalfTeamT : int //[JsonProperty("score_half1_team2")] public int ScoreFirstHalfTeamT => TeamT.ScoreFirstHalf;

    /// <summary>
    /// Second half score of the team that started as CT
    /// </summary>
    ScoreSecondHalfTeamCt : int // [JsonProperty("score_half2_team1")] public int ScoreSecondHalfTeamCt => TeamCT.ScoreSecondHalf;

    /// <summary>
    /// Second half score of the team that started as T
    /// </summary>
    ScoreSecondHalfTeamT : int // [JsonProperty("score_half2_team2")] public int ScoreSecondHalfTeamT => TeamT.ScoreSecondHalf;

    FiveKillCount : int // [JsonProperty("5k_count")] public int FiveKillCount

    FourKillCount : int // [JsonProperty("4k_count")] public int FourKillCount
    ThreeKillCount : int //[JsonProperty("3k_count")] public int ThreeKillCount

    TwoKillCount : int // [JsonProperty("2k_count")] public int TwoKillCount

    OneKillCount : int// [JsonProperty("1k_count")] public int OneKillCount

    //TeamCT : Team //[JsonProperty("team_ct", IsReference = false)] public Team TeamCT

    //TeamT : Team //[JsonProperty("team_t", IsReference = false)]    public Team TeamT

    //[JsonProperty("team_surrender", IsReference = true)]    public Team Surrender
    //[JsonProperty("team_winner", IsReference = true)]    public Team Winner
    //[JsonProperty("rounds", IsReference = false)]    public ObservableCollection<Round> Rounds
    //[JsonProperty("players", IsReference = false)]    public ObservableCollection<Player> Players
    //[JsonProperty("most_killing_weapon")]    public Weapon MostKillingWeapon
    //[JsonProperty("overtimes", IsReference = false)]    public ObservableCollection<Overtime> Overtimes
    //[JsonProperty("most_headshot_player", IsReference = true)]    public Player MostHeadshotPlayer
    //[JsonProperty("most_bomb_planted_player", IsReference = true)]    public Player MostBombPlantedPlayer
    //[JsonProperty("most_entry_kill", IsReference = true)]    public Player MostEntryKillPlayer
    //[JsonProperty("bomb_planted", IsReference = false)]    public ObservableCollection<BombPlantedEvent> BombPlanted
    //[JsonProperty("bomb_defused", IsReference = false)] public ObservableCollection<BombDefusedEvent> BombDefused
    //[JsonProperty("bomb_exploded", IsReference = false)] public ObservableCollection<BombExplodedEvent> BombExploded
    //[JsonProperty("kills", IsReference = false)] public ObservableCollection<KillEvent> Kills
    //[JsonProperty("weapon_fired", IsReference = false)] public ObservableCollection<WeaponFireEvent> WeaponFired
    //[JsonProperty("player_blinded_events", IsReference = false)] public ObservableCollection<PlayerBlindedEvent> PlayerBlinded
    //[JsonProperty("player_hurted", IsReference = false)] public ObservableCollection<PlayerHurtedEvent> PlayersHurted
    //[JsonIgnore]  public string DateAsString => _date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    //[JsonIgnore]  public Source.Source Source
    //[JsonIgnore]  public string DurationTime => TimeSpan.FromSeconds(_duration).ToString(@"hh\:mm\:ss");
    //[JsonProperty("kill_count")] public int KillCount
    //[JsonProperty("clutch_count")] public int ClutchCount
    //[JsonProperty("trade_kill_count")]    public int TradeKillCount
    //[JsonProperty("bomb_defused_count")]    public int BombDefusedCount
    //[JsonProperty("bomb_planted_count")]    public int BombPlantedCount
    //[JsonProperty("bomb_exploded_count")]    public int BombExplodedCount
    //[JsonProperty("flashbang_thrown_count")]    public int FlashbangThrownCount
    //[JsonProperty("smoke_thrown_count")]    public int SmokeThrownCount
    //[JsonProperty("he_thrown_count")]    public int HeThrownCount
    //[JsonProperty("decoy_thrown_count")]    public int DecoyThrownCount
    //[JsonProperty("molotov_thrown_count")]    public int MolotovThrownCount
    //[JsonProperty("incendiary_thrown_count")]    public int IncendiaryThrownCount
    //[JsonIgnore]  public string WinStatus
    //[JsonIgnore]  public ObservableCollection<PositionPoint> PositionPoints
    //[JsonProperty("decoys")]  public ObservableCollection<DecoyStartedEvent> DecoyStarted
    //[JsonProperty("incendiaries")]    public ObservableCollection<MolotovFireStartedEvent> IncendiariesFireStarted
    //[JsonProperty("molotovs")]    public ObservableCollection<MolotovFireStartedEvent> MolotovsFireStarted
    //[JsonProperty("damage_health_count")]    public int DamageHealthCount
    //[JsonProperty("damage_armor_count")]    public int DamageArmorCount
    //[JsonIgnore]  public Weapon MostDamageWeapon
    //        Dictionary<Weapon, int> weapons = new Dictionary<Weapon, int>();
    //
    //        foreach (PlayerHurtedEvent playerHurtedEvent in Rounds.SelectMany(round => round.PlayersHurted))
    //        {
    //            if (!weapons.ContainsKey(playerHurtedEvent.Weapon))
    //                weapons[playerHurtedEvent.Weapon] = playerHurtedEvent.HealthDamage + playerHurtedEvent.ArmorDamage;
    //            else
    //                weapons[playerHurtedEvent.Weapon] += playerHurtedEvent.HealthDamage + playerHurtedEvent.ArmorDamage;
    //        }
    //
    //        return weapons.OrderByDescending(x => x.Value).FirstOrDefault().Key;
    //[JsonProperty("average_health_damage")]    public double AverageHealthDamage
    //[JsonProperty("kill_per_round")]    public double KillPerRound
    //[JsonProperty("assist_per_round")]    public double AssistPerRound
    //[JsonProperty("jump_kill_count")]    public int JumpKillCount
    //[JsonProperty("crouch_kill_count")]    public int CrouchKillCount
    //[JsonProperty("headshot_count")]    public int HeadshotCount
    //[JsonProperty("death_count")]    public int DeathCount
    //[JsonProperty("assist_count")]    public int AssistCount
    //[JsonProperty("entry_kill_count")]    public int EntryKillCount
    //[JsonProperty("knife_kill_count")]    public int KnifeKillCount
    //[JsonProperty("mvp_count")]    public int MvpCount
    //[JsonProperty("teamkill_count")]    public int TeamKillCount
    //[JsonProperty("death_per_round")]    public double DeathPerRound
    //[JsonProperty("clutch_lost_count")]    public int ClutchLostCount
    //[JsonProperty("clutch_won_count")]    public int ClutchWonCount
    //[JsonProperty("shot_count")]    public int WeaponFiredCount
    //[JsonProperty("hit_count")]    public int HitCount
    //[JsonProperty("average_hltv_rating")]    public float AverageHltvRating
    //[JsonProperty("average_esea_rws")]    public decimal AverageEseaRws
    //[JsonProperty("chat_messages")]    public List<string> ChatMessageList
}

type DemoData =
    { Demos : Demo list
      Pages : int }

type BackgroundTaskId = string

type BackgroundTask =
    { Id : BackgroundTaskId
      Name : string 
      Progress : double
      Messages : string list }

type StartMMDownloadResult =
    { Status : string; Task : BackgroundTaskId }

type Notification =
    | DemosFound of string list
    | TaskStarted of BackgroundTask
    | TaskCompleted of BackgroundTaskId
    | TaskProgressChanged of BackgroundTaskId * double
    | TaskMessageChanged of BackgroundTaskId * string
    | Hint of string
    | Error of string


type BackgroundTasks =
    { Tasks : BackgroundTask list }