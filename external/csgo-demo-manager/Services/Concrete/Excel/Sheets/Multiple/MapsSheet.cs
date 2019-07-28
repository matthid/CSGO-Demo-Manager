﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Core.Models.Events;
using NPOI.SS.UserModel;

namespace Services.Concrete.Excel.Sheets.Multiple
{
	public class MapsSheet : AbstractMultipleSheet
	{
		public MapsSheet(IWorkbook workbook, List<Demo> demos, long steamId = 0)
		{
			SteamId = steamId;
			Headers = new Dictionary<string, CellType>()
			{
				{"Name", CellType.String},
				{"Match", CellType.Numeric},
				{"Round", CellType.Numeric},
				{"Round win CT", CellType.Numeric},
				{"Round win T", CellType.Numeric},
				{"Round win pistol round", CellType.Numeric},
				{"Round win eco", CellType.Numeric},
				{"Round win Semi-eco", CellType.Numeric},
				{"Round win force-buy", CellType.Numeric},
				{"Bomb planted", CellType.Numeric},
				{"Bomb defused", CellType.Numeric},
				{"Bomb exploded", CellType.Numeric},
				{"Bomb planted on A", CellType.Numeric},
				{"Bomb planted on B", CellType.Numeric}
			};
			Demos = demos;
			Sheet = workbook.CreateSheet("Maps");
		}

		public override async Task GenerateContent()
		{
			await Task.Factory.StartNew(() =>
			{
				List<Map> maps = new List<Map>();
				foreach (Demo demo in Demos)
				{
					Map currentMap = new Map
					{
						Name = demo.MapName
					};
					if (maps.Contains(currentMap))
					{
						Map map = maps.First(m => m.Equals(currentMap));
						UpdateMapStats(demo, map);
					}
					else
					{
						UpdateMapStats(demo, currentMap);
						maps.Add(currentMap);
					}
				}

				int rowCount = 1;
				foreach (Map map in maps)
				{
					IRow row = Sheet.CreateRow(rowCount++);
					int columnNumber = 0;
					SetCellValue(row, columnNumber++, CellType.String, map.Name);
					SetCellValue(row, columnNumber++, CellType.String, map.MatchCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, map.RoundCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, map.WinCounterTerroritsCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, map.WinTerroristCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, map.WinPistolRoundCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, map.WinEcoRoundCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, map.WinSemiEcoCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, map.WinForceBuyCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, map.BombPlantedCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, map.BombDefusedCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, map.BombExplodedCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, map.BombPlantedOnACount);
					SetCellValue(row, columnNumber, CellType.Numeric, map.BombPlantedOnBCount);
				}
			});
		}

		private void UpdateMapStats(Demo demo, Map map)
		{
			map.MatchCount++;
			map.RoundCount += demo.Rounds.Count();
			map.BombPlantedCount += demo.BombPlantedCount;
			map.BombDefusedCount += demo.BombDefusedCount;
			map.BombExplodedCount += demo.BombExplodedCount;
			foreach (Round round in demo.Rounds)
			{
				if (SteamId != 0)
				{
					Player player = demo.Players.FirstOrDefault(p => p.SteamId == SteamId);
					if (player != null)
					{
						if (player.TeamName != round.WinnerName) continue;
					}
				}

				if (round.WinnerSide == Side.CounterTerrorist) map.WinCounterTerroritsCount++;
				if (round.WinnerSide == Side.Terrorist) map.WinTerroristCount++;
				if (round.SideTrouble != Side.None)
				{
					switch (round.Type)
					{
						case RoundType.ECO:
							map.WinEcoRoundCount++;
							break;
						case RoundType.SEMI_ECO:
							map.WinSemiEcoCount++;
							break;
						case RoundType.FORCE_BUY:
							map.WinForceBuyCount++;
							break;
						case RoundType.PISTOL_ROUND:
							map.WinPistolRoundCount++;
							break;
					}
				}
			}
			foreach (BombPlantedEvent plantedEvent in demo.BombPlanted)
			{
				if (SteamId != 0 && plantedEvent.PlanterSteamId != SteamId) continue;
				if (plantedEvent.Site == "A")
				{
					map.BombPlantedOnACount++;
				}
				else
				{
					map.BombPlantedOnBCount++;
				}
			}
		}
	}
}
