using System.Threading.Tasks;
using Core.Models;
using NPOI.SS.UserModel;
using Services.Concrete.Excel.Sheets.Single;
using Services.Interfaces;

namespace Services.Concrete.Excel
{
	public class SingleExport : AbstractExport
	{
		private readonly Demo _demo;

		private GeneralSheet _generalSheet;

		private PlayersSheet _playersSheet;

		private RoundsSheet _roundsSheet;

		private EntryHoldKillsRoundSheet _entryHoldKillsRoundSheet;

		private EntryHoldKillsPlayerSheet _entryHoldKillsPlayerSheet;

		private EntryHoldKillsTeamSheet _entryHoldKillsTeamSheet;

		private EntryKillsRoundSheet _entryKillsRoundSheet;

		private EntryKillsPlayerSheet _entryKillsPlayerSheet;

		private EntryKillsTeamSheet _entryKillsTeamSheet;

		private KillsSheet _killsSheet;

		private KillMatrixSheet _killMatrixSheet;

		private FlashMatrixPlayersSheet _flashMatrixPlayersSheet;

		private FlashMatrixTeamsSheet _flashMatrixTeamsSheet;

		public SingleExport(Demo demo)
		{
			_demo = demo;
		}

		public override async Task<IWorkbook> Generate(ICacheService cacheService)
		{
			_demo.WeaponFired = await cacheService.GetDemoWeaponFiredAsync(_demo);
			_generalSheet = new GeneralSheet(Workbook, _demo);
			await _generalSheet.Generate(cacheService);
			_playersSheet = new PlayersSheet(Workbook, _demo);
			await _playersSheet.Generate(cacheService);
			_roundsSheet = new RoundsSheet(Workbook, _demo);
			await _roundsSheet.Generate(cacheService);
			_killsSheet = new KillsSheet(Workbook, _demo);
			await _killsSheet.Generate(cacheService);
			_entryHoldKillsRoundSheet = new EntryHoldKillsRoundSheet(Workbook, _demo);
			await _entryHoldKillsRoundSheet.Generate(cacheService);
			_entryHoldKillsPlayerSheet = new EntryHoldKillsPlayerSheet(Workbook, _demo);
			await _entryHoldKillsPlayerSheet.Generate(cacheService);
			_entryHoldKillsTeamSheet = new EntryHoldKillsTeamSheet(Workbook, _demo);
			await _entryHoldKillsTeamSheet.Generate(cacheService);
			_entryKillsRoundSheet = new EntryKillsRoundSheet(Workbook, _demo);
			await _entryKillsRoundSheet.Generate(cacheService);
			_entryKillsPlayerSheet = new EntryKillsPlayerSheet(Workbook, _demo);
			await _entryKillsPlayerSheet.Generate(cacheService);
			_entryKillsTeamSheet = new EntryKillsTeamSheet(Workbook, _demo);
			await _entryKillsTeamSheet.Generate(cacheService);
			_killMatrixSheet = new KillMatrixSheet(Workbook, _demo);
			await _killMatrixSheet.Generate(cacheService);
			_flashMatrixPlayersSheet = new FlashMatrixPlayersSheet(Workbook, _demo);
			await _flashMatrixPlayersSheet.Generate(cacheService);
			_flashMatrixTeamsSheet = new FlashMatrixTeamsSheet(Workbook, _demo);
			await _flashMatrixTeamsSheet.Generate(cacheService);

			return Workbook;
		}
	}
}
