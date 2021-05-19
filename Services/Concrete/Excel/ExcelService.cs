using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Core.Models;
using NPOI.SS.UserModel;
using Services.Interfaces;

namespace Services.Concrete.Excel
{
	public class ExcelService
	{
        private readonly ICacheService _cache;

        public ExcelService(ICacheService cache)
        {
            _cache = cache;
        }

		public async Task GenerateXls(Demo demo, string fileName)
		{
			SingleExport export = new SingleExport(demo);
			IWorkbook workbook = await export.Generate(_cache);
			FileStream sw = File.Create(fileName);
			workbook.Write(sw);
			sw.Close();
		}

		public async Task GenerateXls(List<Demo> demos, string fileName, long selectedStatsAccountSteamId = 0)
		{
			MultipleExport export = new MultipleExport(demos, selectedStatsAccountSteamId);
			IWorkbook workbook = await export.Generate(_cache);
			FileStream sw = File.Create(fileName);
			workbook.Write(sw);
			sw.Close();
		}
	}
}