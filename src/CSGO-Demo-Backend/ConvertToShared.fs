module ConvertToShared

type CSDemo = Core.Models.Demo
type FSDemo = Shared.Demo
type CSPlayer = Core.Models.Player
type FSPlayer = Shared.Player
type CSTeam = Core.Models.Team
type FSTeam = Shared.Team
type CSWeapon = Core.Models.Weapon
type FSWeapon = Shared.Weapon
type CSSide = Core.Models.Side
type FSSide = Shared.Side
type CSWeaponType = Core.Models.WeaponType
type FSWeaponType = Shared.WeaponType
type CSEquipmentElement = DemoInfo.EquipmentElement
type FSEquipmentElement = Shared.EquipmentElement
type CSEntryKillEvent = Core.Models.Events.EntryKillEvent
type FSEntryKillEvent = Shared.EntryKillEvent
type CSEntryHoldKillEvent = Core.Models.Events.EntryHoldKillEvent
type FSEntryHoldKillEvent = Shared.EntryHoldKillEvent
type CSKillEvent = Core.Models.Events.KillEvent
type FSKillEvent = Shared.KillEvent
type CSClutchEvent = Core.Models.Events.ClutchEvent
type FSClutchEvent = Shared.ClutchEvent

let ofFloat (d:float32) : Shared.FloatNum =
    float d
let ofDecimal (d:decimal) : Shared.DecimalNum =
    float d
let ofSteamId (d:int64) : Shared.SteamId =
    d.ToString()

let ofDict (d:System.Collections.Generic.IDictionary<_,_>) =
    d |> Seq.map (fun kv -> kv.Key, kv.Value) |> Map.ofSeq
    
let ofSide (ev:CSSide) : FSSide =
    match ev with
    | CSSide.Spectate -> FSSide.Spectate
    | CSSide.Terrorist -> FSSide.Terrorist
    | CSSide.CounterTerrorist-> FSSide.CounterTerrorist
    | CSSide.None -> FSSide.NoneSide
    | _ -> failwithf "unknown side %d" (int ev)

let ofEquipmentElement (ev:CSEquipmentElement) : FSEquipmentElement =
    match ev with
    | CSEquipmentElement.Unknown -> FSEquipmentElement.Unknown

    //Pistoles
    | CSEquipmentElement.P2000           -> FSEquipmentElement.P2000
    | CSEquipmentElement.Glock           -> FSEquipmentElement.Glock
    | CSEquipmentElement.P250            -> FSEquipmentElement.P250
    | CSEquipmentElement.Deagle          -> FSEquipmentElement.Deagle 
    | CSEquipmentElement.FiveSeven       -> FSEquipmentElement.FiveSeven
    | CSEquipmentElement.DualBarettas    -> FSEquipmentElement.DualBarettas
    | CSEquipmentElement.Tec9            -> FSEquipmentElement.Tec9
    | CSEquipmentElement.CZ              -> FSEquipmentElement.CZ
    | CSEquipmentElement.USP             -> FSEquipmentElement.USP
    | CSEquipmentElement.Revolver        -> FSEquipmentElement.Revolver 

    //SMGs
    | CSEquipmentElement.MP7             -> FSEquipmentElement.MP7   
    | CSEquipmentElement.MP9             -> FSEquipmentElement.MP9
    | CSEquipmentElement.Bizon           -> FSEquipmentElement.Bizon
    | CSEquipmentElement.Mac10           -> FSEquipmentElement.Mac10
    | CSEquipmentElement.UMP             -> FSEquipmentElement.UMP
    | CSEquipmentElement.P90             -> FSEquipmentElement.P90
    | CSEquipmentElement.MP5SD           -> FSEquipmentElement.MP5SD

    //Heavy
    | CSEquipmentElement.SawedOff        -> FSEquipmentElement.SawedOff
    | CSEquipmentElement.Nova            -> FSEquipmentElement.Nova
    | CSEquipmentElement.Swag7           -> FSEquipmentElement.Swag7
    | CSEquipmentElement.XM1014          -> FSEquipmentElement.XM1014
    | CSEquipmentElement.M249            -> FSEquipmentElement.M249
    | CSEquipmentElement.Negev           -> FSEquipmentElement.Negev

    //Rifle
    | CSEquipmentElement.Gallil          -> FSEquipmentElement.Gallil 
    | CSEquipmentElement.Famas           -> FSEquipmentElement.Famas
    | CSEquipmentElement.AK47            -> FSEquipmentElement.AK47
    | CSEquipmentElement.M4A4            -> FSEquipmentElement.M4A4
    | CSEquipmentElement.M4A1            -> FSEquipmentElement.M4A1
    | CSEquipmentElement.Scout           -> FSEquipmentElement.Scout
    | CSEquipmentElement.SG556           -> FSEquipmentElement.SG556
    | CSEquipmentElement.AUG             -> FSEquipmentElement.AUG
    | CSEquipmentElement.AWP             -> FSEquipmentElement.AWP
    | CSEquipmentElement.Scar20          -> FSEquipmentElement.Scar20
    | CSEquipmentElement.G3SG1           -> FSEquipmentElement.G3SG1

    //Equipment
    | CSEquipmentElement.Zeus            -> FSEquipmentElement.Zeus
    | CSEquipmentElement.Kevlar          -> FSEquipmentElement.Kevlar
    | CSEquipmentElement.Helmet          -> FSEquipmentElement.Helmet
    | CSEquipmentElement.Bomb            -> FSEquipmentElement.Bomb
    | CSEquipmentElement.Knife           -> FSEquipmentElement.Knife
    | CSEquipmentElement.DefuseKit       -> FSEquipmentElement.DefuseKit
    | CSEquipmentElement.World           -> FSEquipmentElement.World

    //Grenades
    | CSEquipmentElement.Decoy           -> FSEquipmentElement.Decoy
    | CSEquipmentElement.Molotov         -> FSEquipmentElement.Molotov
    | CSEquipmentElement.Incendiary      -> FSEquipmentElement.Incendiary
    | CSEquipmentElement.Flash           -> FSEquipmentElement.Flash
    | CSEquipmentElement.Smoke           -> FSEquipmentElement.Smoke
    | CSEquipmentElement.HE              -> FSEquipmentElement.HE
    | _ -> failwithf "unknown EquipmentElement %d" (int ev)
let ofWeaponType (ev:CSWeaponType) : FSWeaponType =
    match ev with
    | CSWeaponType.Pistol    -> FSWeaponType.Pistol
    | CSWeaponType.Rifle     -> FSWeaponType.Rifle
    | CSWeaponType.Sniper    -> FSWeaponType.Sniper
    | CSWeaponType.SMG       -> FSWeaponType.SMG
    | CSWeaponType.Heavy     -> FSWeaponType.Heavy
    | CSWeaponType.Equipment -> FSWeaponType.Equipment
    | CSWeaponType.Grenade   -> FSWeaponType.Grenade
    | CSWeaponType.Unkown    -> FSWeaponType.Unkown
    | _ -> failwithf "unknown WeaponType %d" (int ev)

let ofWeapon (weapon:CSWeapon) : FSWeapon =
    {
        Element = weapon.Element |> ofEquipmentElement
        Type = weapon.Type |> ofWeaponType
        Name = weapon.Name
        KillAward = weapon.KillAward
    }

let ofEntryKillEvent (ev:CSEntryKillEvent) : FSEntryKillEvent =
    {
        Tick = ev.Tick
        Seconds = ev.Seconds |> ofFloat
    
        // Others
        RoundNumber = ev.RoundNumber
        KillerSteamId = ev.KillerSteamId |> ofSteamId
        KillerName = ev.KillerName
        KillerSide = ev.KillerSide |> ofSide
        KilledSteamId  = ev.KilledSteamId |> ofSteamId
        KilledName = ev.KilledName
        KilledSide = ev.KilledSide |> ofSide
        Weapon = ev.Weapon |> ofWeapon
        HasWon = ev.HasWon
        HasWonRound = ev.HasWonRound    
    }
let ofEntryHoldKillEvent (ev:CSEntryHoldKillEvent) : FSEntryHoldKillEvent =
    {
        Tick = ev.Tick
        Seconds = float ev.Seconds
        
        // Others
        RoundNumber = ev.RoundNumber
        KillerSteamId = ev.KillerSteamId |> ofSteamId
        KillerName = ev.KillerName
        KillerSide = ev.KillerSide |> ofSide
        KilledSteamId  = ev.KilledSteamId |> ofSteamId
        KilledName = ev.KilledName
        KilledSide = ev.KilledSide |> ofSide
        Weapon = ev.Weapon |> ofWeapon
        HasWon = ev.HasWon
        HasWonRound = ev.HasWonRound    
    }
let ofKillEvent (ev:CSKillEvent) : FSKillEvent =
    {
        // BaseEvent
        Tick = ev.Tick
        Seconds = ev.Seconds |> ofFloat
        
        // Others
        KillerSteamId = ev.KillerSteamId |> ofSteamId
        KilledSteamId = ev.KilledSteamId |> ofSteamId
        AssisterSteamId = ev.AssisterSteamId |> ofSteamId
        Weapon = ev.Weapon |> ofWeapon
        //[JsonProperty("heatmap_point")] public KillHeatmapPoint Point
        //[JsonProperty("killer_vel_x")] public float KillerVelocityX
        //[JsonProperty("killer_vel_y")] public float KillerVelocityY
        //[JsonProperty("killer_vel_z")] public float KillerVelocityZ
        KillerSide = ev.KillerSide |> ofSide
        KillerTeam = ev.KillerTeam
        KilledSide = ev.KilledSide |> ofSide
        KilledTeam = ev.KilledTeam
        KillerName = ev.KillerName
        KilledName = ev.KilledName
        AssisterName = ev.AssisterName
        RoundNumber = ev.RoundNumber
        /// <summary>
        /// Number of seconds elapsed since the freezetime end
        /// </summary>
        TimeDeathSeconds = ev.TimeDeathSeconds |> ofFloat
        IsKillerCrouching = ev.IsKillerCrouching
        KillerIsBlinded = ev.KillerIsBlinded
        IsTradeKill = ev.IsTradeKill
        IsHeadshot = ev.IsHeadshot
        KillerIsControllingBot = ev.KillerIsControllingBot
        KilledIsControllingBot = ev.KilledIsControllingBot
        VictimIsBlinded = ev.VictimIsBlinded
        AssisterIsControllingBot = ev.AssisterIsControllingBot
    }

let ofClutchEvent (ev:CSClutchEvent) : FSClutchEvent =
    {
        OpponentCount = ev.OpponentCount
        HasWon = ev.HasWon
        RoundNumber = ev.RoundNumber
    }

let ofPlayer (player:CSPlayer) : FSPlayer =
    {
        SteamId = player.SteamId |> ofSteamId
        Name = player.Name
        KillCount = player.KillCount
        CrouchKillCount = player.CrouchKillCount
        JumpKillCount = player.JumpKillCount
        Score = player.Score
        TeamKillCount = player.TeamKillCount
        AssistCount = player.AssistCount
        TradeKillCount = player.TradeKillCount
        TradeDeathCount = player.TradeDeathCount
        BombPlantedCount = player.BombPlantedCount
        BombDefusedCount = player.BombDefusedCount
        BombExplodedCount = player.BombExplodedCount
        DeathCount = player.DeathCount
        FiveKillCount = player.FiveKillCount
        FourKillCount = player.FourKillCount
        ThreeKillCount = player.ThreeKillCount
        TwoKillCount = player.TwoKillCount
        OneKillCount = player.OneKillCount
        HeadshotCount = player.HeadshotCount
        KillDeathRatio = player.KillDeathRatio |> ofDecimal
        RoundMvpCount = player.RoundMvpCount
        RatingHltv = player.RatingHltv |> ofFloat
        EseaRwsPointCount = player.EseaRwsPointCount |> ofDecimal
        EseaRws = player.EseaRws |> ofDecimal
        ShotCount = player.ShotCount
        HitCount = player.HitCount
        Accuracy = player.Accuracy
        IsVacBanned = player.IsVacBanned
        IsOverwatchBanned = player.IsOverwatchBanned
        FlashbangThrownCount = player.FlashbangThrownCount
        SmokeThrownCount = player.SmokeThrownCount
        HeGrenadeThrownCount = player.HeGrenadeThrownCount
        MolotovThrownCount = player.MolotovThrownCount
        IncendiaryThrownCount = player.IncendiaryThrownCount
        DecoyThrownCount = player.DecoyThrownCount
        RoundPlayedCount = player.RoundPlayedCount
        TeamName = player.TeamName
        StartMoneyRounds = player.StartMoneyRounds |> ofDict
        EquipementValueRounds = player.EquipementValueRounds |> ofDict
        RoundsMoneyEarned = player.RoundsMoneyEarned |> ofDict
        TimeDeathRounds = player.TimeDeathRounds |> ofDict |> Map.map (fun k v -> ofFloat v)
        EntryKills = player.EntryKills |> Seq.map ofEntryKillEvent |> Seq.toList
        EntryHoldKills = player.EntryHoldKills |> Seq.map ofEntryHoldKillEvent |> Seq.toList
        Kills = player.Kills |> Seq.map ofKillEvent |> Seq.toList
        Deaths = player.Deaths |> Seq.map ofKillEvent |> Seq.toList
        Assists = player.Assists |> Seq.map ofKillEvent |> Seq.toList
        // [JsonProperty("players_hurted")] public ObservableCollection<PlayerHurtedEvent> PlayersHurted
        Clutches = player.Clutches |> Seq.map ofClutchEvent |> Seq.toList
        //[JsonProperty("rank_old")] public int RankNumberOld
        //[JsonProperty("rank_new")] public int RankNumberNew
        //[JsonProperty("win_count")] public int WinCount
        //[JsonIgnore] public string AvatarUrl
        SuicideCount = player.SuicideCount
    }

let ofTeam (team:CSTeam) : FSTeam =
    {
        Name = team.Name
        Score = team.Score
        ScoreFirstHalf = team.ScoreFirstHalf
        ScoreSecondHalf = team.ScoreSecondHalf
        Players = team.Players |> Seq.map ofPlayer |> Seq.toList
    }
let ofDemo (demo:CSDemo) : FSDemo =
    {
        Id = demo.Id
        Name = demo.Name
        NameWithoutExtension = demo.NameWithoutExtension
        Date = demo.Date
        SourceName = demo.SourceName
        Comment = demo.Comment
        Status = demo.Status
        ClientName = demo.ClientName
        Hostname = demo.Hostname
        Type = demo.Type
        Tickrate = demo.Tickrate |> ofFloat
        ServerTickrate = demo.ServerTickrate |> ofFloat
        Duration = demo.Duration |> ofFloat
        Ticks = demo.Ticks
        MapName = demo.MapName
        Path = demo.Path
        // HasCheater : bool // [JsonIgnore] public bool HasCheater => _cheaterCounter > 0;
        CheaterCount = demo.CheaterCount
        ScoreTeamCt = demo.ScoreTeamCt
        ScoreTeamT = demo.ScoreTeamT
        ScoreFirstHalfTeamCt = demo.ScoreFirstHalfTeamCt
        ScoreFirstHalfTeamT = demo.ScoreFirstHalfTeamT
        ScoreSecondHalfTeamCt = demo.ScoreSecondHalfTeamCt
        ScoreSecondHalfTeamT = demo.ScoreSecondHalfTeamT
        FiveKillCount = demo.FiveKillCount
        FourKillCount = demo.FourKillCount
        ThreeKillCount = demo.ThreeKillCount
        TwoKillCount = demo.TwoKillCount
        OneKillCount = demo.OneKillCount
        TeamCT = demo.TeamCT |> ofTeam
        TeamT = demo.TeamT |> ofTeam
    }