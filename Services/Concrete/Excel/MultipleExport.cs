using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;
using NPOI.SS.UserModel;
using Services.Concrete.Excel.Sheets.Multiple;
using Services.Interfaces;

namespace Services.Concrete.Excel
{
	public class MultipleExport : AbstractExport
	{
		private readonly List<Demo> _demos;

		private readonly long _selectedStatsAccountSteamId;

		private GeneralSheet _generalSheet;

		private PlayersSheet _playersSheet;

		private MapsSheet _mapsSheet;

		private TeamsSheet _teamsSheet;

		private WeaponsSheet _weaponsSheet;

		private RoundsSheet _roundsSheet;

		private KillsSheet _killsSheet;

		private KillMatrixSheet _killMatrixSheet;

		private FlashMatrixPlayersSheet _flashMatrixPlayersSheet;

		private FlashMatrixTeamsSheet _flashMatrixTeamsSheet;

		public MultipleExport(List<Demo> demos, long selectedStatsAccountSteamId = 0)
		{
			_demos = demos;
			_selectedStatsAccountSteamId = selectedStatsAccountSteamId;
		}

		public override async Task<IWorkbook> Generate(ICacheService cacheService)
		{
			foreach (Demo demo in _demos)
			{
				demo.WeaponFired = await cacheService.GetDemoWeaponFiredAsync(demo);
			}

			_generalSheet = new GeneralSheet(Workbook, _demos);
			await _generalSheet.Generate(cacheService);
			_playersSheet = new PlayersSheet(Workbook, _demos);
			await _playersSheet.Generate(cacheService);
			_mapsSheet = new MapsSheet(Workbook, _demos, _selectedStatsAccountSteamId);
			await _mapsSheet.Generate(cacheService);
			_teamsSheet = new TeamsSheet(Workbook, _demos);
			await _teamsSheet.Generate(cacheService);
			_weaponsSheet = new WeaponsSheet(Workbook, _demos, _selectedStatsAccountSteamId);
			await _weaponsSheet.Generate(cacheService);
			_roundsSheet = new RoundsSheet(Workbook, _demos);
			await _roundsSheet.Generate(cacheService);
			_killsSheet = new KillsSheet(Workbook, _demos);
			await _killsSheet.Generate(cacheService);
			_killMatrixSheet = new KillMatrixSheet(Workbook, _demos);
			await _killMatrixSheet.Generate(cacheService);
			_flashMatrixPlayersSheet = new FlashMatrixPlayersSheet(Workbook, _demos);
			await _flashMatrixPlayersSheet.Generate(cacheService);
			_flashMatrixTeamsSheet = new FlashMatrixTeamsSheet(Workbook, _demos);
			await _flashMatrixTeamsSheet.Generate(cacheService);

			return Workbook;
		}
	}
}
