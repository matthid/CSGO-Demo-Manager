﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Core.Models.Events;
using Services.Interfaces;
using Services.Models.Stats;

namespace Services.Concrete
{
	public class FlashbangService : IFlashbangService
	{
		public Demo Demo { get; set; }

		/// <summary>
		/// Generate data for the heatmap that display flash times for each player
		/// </summary>
		/// <returns></returns>
		public async Task<List<FlashbangDataPoint>> GetPlayersFlashTimesData()
		{
			List<FlashbangDataPoint> data = new List<FlashbangDataPoint>();
			await Task.Factory.StartNew(() =>
			{
				foreach (Player player in Demo.Players)
				{
					Dictionary<long, float> playerFlashStats = new Dictionary<long, float>();

					foreach (Player pl in Demo.Players)
					{
						if (!playerFlashStats.ContainsKey(pl.SteamId)) playerFlashStats.Add(pl.SteamId, 0);
					}

					foreach (PlayerBlindedEvent e in Demo.PlayerBlinded.Where(e => player.SteamId == e.ThrowerSteamId))
					{
						if (!playerFlashStats.ContainsKey(e.VictimSteamId))
							playerFlashStats.Add(e.VictimSteamId, 0);
						playerFlashStats[e.VictimSteamId] += e.Duration;
					}

					data.AddRange(playerFlashStats.Select(playerStats => new FlashbangDataPoint
					{
						Flasher = player.Name,
						Flashed = Demo.Players.First(p => p.SteamId == playerStats.Key).Name,
						Duration = Math.Round((decimal)playerStats.Value, 2, MidpointRounding.AwayFromZero)
					}));
				}
			});

			return data;
		}

		/// <summary>
		/// Generate data for the heatmap that display flash times by team
		/// </summary>
		/// <returns></returns>
		public async Task<List<FlashbangDataPoint>> GetTeamsFlashTimesData()
		{
			List<FlashbangDataPoint> data = new List<FlashbangDataPoint>();

			await Task.Factory.StartNew(() =>
			{
				Dictionary<string, float> teamCtStats = new Dictionary<string, float>();
				Dictionary<string, float> teamTStats = new Dictionary<string, float>();

				foreach (PlayerBlindedEvent e in Demo.PlayerBlinded)
				{
					if (e.ThrowerTeamName == Demo.TeamCT.Name)
					{
						if (!teamCtStats.ContainsKey(e.VictimTeamName))
							teamCtStats.Add(e.VictimTeamName, 0);
						teamCtStats[e.VictimTeamName] += e.Duration;
					}
					else
					{
						if (!teamTStats.ContainsKey(e.VictimTeamName))
							teamTStats.Add(e.VictimTeamName, 0);
						teamTStats[e.VictimTeamName] += e.Duration;
					}
				}

				data.AddRange(teamCtStats.Select(ctStats => new FlashbangDataPoint
				{
					Flasher = Demo.TeamCT.Name,
					Flashed = ctStats.Key,
					Duration = Math.Round((decimal)ctStats.Value, 2, MidpointRounding.AwayFromZero)
				}));

				data.AddRange(teamTStats.Select(tStats => new FlashbangDataPoint
				{
					Flasher = Demo.TeamT.Name,
					Flashed = tStats.Key,
					Duration = Math.Round((decimal)tStats.Value, 2, MidpointRounding.AwayFromZero)
				}));
			});

			return data;
		}

		/// <summary>
		/// Generate data for the heatmap that display average flash times for each player
		/// </summary>
		/// <returns></returns>
		public async Task<List<FlashbangDataPoint>> GetAverageFlashTimesPlayersData()
		{
			List<FlashbangDataPoint> data = new List<FlashbangDataPoint>();

			await Task.Factory.StartNew(() =>
			{
				Dictionary<Player, float> playerFlashStats = new Dictionary<Player, float>();

				foreach (Player player in Demo.Players)
				{
					float totalDuration = Demo.PlayerBlinded
						.Where(e => e.ThrowerSteamId == player.SteamId)
						.Sum(e => e.Duration);

					if (!playerFlashStats.ContainsKey(player))
						playerFlashStats.Add(player, 0);
					playerFlashStats[player] = totalDuration;
				}

				data.AddRange(from playersValue in playerFlashStats
					let total = playersValue.Key.FlashbangThrownCount > 0
					? Math.Round((decimal)(playersValue.Value/playersValue.Key.FlashbangThrownCount),
					2, MidpointRounding.AwayFromZero)
					: 0
					select new FlashbangDataPoint
					{
						Flasher = playersValue.Key.Name,
						Flashed = "fakeplayer",
						Duration = total
					});
			});

			return data;
		}
	}
}
