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

## Queries we want to do

- List all Demos for a specific Account/List of Accounts
    Demos.Select(d -> d.Players.Contains(p => player == p))
- List current Stats for a specific Account/List of Accounts
- Historic Analysis of Data (for Account)
  - Aggregated statistics (ALL)
  - Progression Statistics (by Month/Day/Year)


## Analyzed data

```fsharp
type Demo =
  { Kills / Stats (All Players)
     }

```