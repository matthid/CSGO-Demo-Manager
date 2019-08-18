# Demo Manager

## Source data

- *.dem files (Matchmaking / FaceIt)
  - Download ourself (link)
  - Allow Upload? -> Maybe later
- Account Data `Account list`:
  ```fsharp
  type SteamId = string
  type Account = SteamId list
  ```


## POC structure

Defined in `Data` project

`multi_account:`
```json
{
  "linked_accounts": [ { "account_id": "<steamId>" } ]
}
```

accounts:
```json
  {
     steam_id: <steamId>
     multi_account: multi_account._id
     name: string
     banned_date: 
     cheater: bool
     // cached stats?

  }
```

demos:
```json
  {
      data_version: int // detect if we should queue an update
      name: string
      date:
      source: 
      hostname:
      demo_tickrate:
      server_tickrate:
      duration:
      ticks:
      map:
      winning_team:
      surrendered:
      comments: [ { account: muti_account._id, text: string } ]
      ct_start_team: {
          score:
          score_first_half:
          score_second_half:
          players: [
              { account_id: <steamId>
                
                // Cached stats (for this demo)
                }
          ]
      }
      t_start_team: { ... }
      rounds: [ {
          number: 1
          start_tick: int
          freezeTimeEndTick: int
          endTick: int
          endTickOfficially: int
          durationInSec: float
          roundEndReason: 
          winningTeam:
          // Infos on the round's entry hold (first kill as CT)
          entryHoldKill: {
              killer: <account_id>
              victim: <account_id>
          }
          // Infos on the round's entry kill (first kill as T)
          entryKill: {
              killer: <account_id>
              victim: <account_id>
          }
          players: [{
             player: <account_id>,
             team: 
             lossbonus:
             flashes: [
                 { players_flashed: [ { id: <account_id> } ]
                   tick: int
                   seconds: float
                   point: { x: int; y: int }
                    }
             ]
             grenades: [
                 { tick: int
                   seconds: float
                   point: { x: int; y: int }
                    }
             ]
             smokes: [
                 { tick: int
                   seconds: float
                   point: { x: int; y: int }
                    }
             ]
             hurtEvents: [
                 { attacker: <account_id> 
                   armorDamage: int
                   healthDamage: int 
                   hitGroup: 
                   weapon:
                   }
             ]
             fire: [
                 { 
                   shooter_angle_pitch: float
                   shooter_angle_yaw: float
                   shooter_pos: { x: float; y: float; z: float }
                   shooter_velocity: { x: float; y: float; z: float }
                   point: { x: float; y: float }}
             ]
             ?killed: { 
                 by: <account_id>; 
                 assister: <account_id>;
                 weapon: ak; 
                 trade: bool
                 headshot: bool
                 crouched_kill: bool
                 killer_position: { x: float; y: float }
                 victim_position: { x: float; y: float }
                 killer_velocity: { x: float; y: float; z: float }
                 killer_controlling_bot: bool
                 victim_controlling_bot: bool
                 assister_controlling_bot: bool
                 killer_blinded: bool
                 victim_blinded: bool
                  }
             unusedNades: []
             startMoney: int
             equipmentValue: int
          }, {
             botname: bot,
             team: 
             lossbonus:
             startMoney: int
             equipmentValue: int
          }]
      }]

      // cached stats?
  }
```

## Queries we want to do

- List all Demos for a specific Account/List of Accounts

  ```json
  { "$or": [ {"ct_start_team.players.account_id": "accountId" }, {"t_start_team.players.account_id": "accountId" } ]})
  ```
- List current stats for a specific Account/List of Accounts

- Historic Analysis of Data
  - 

## Analyzed data

```fsharp
type Demo =
  { Kills / Stats (All Players)
     }

```