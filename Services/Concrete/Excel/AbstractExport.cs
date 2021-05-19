﻿using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Services.Interfaces;

namespace Services.Concrete.Excel
{
	public abstract class AbstractExport
	{
		protected readonly IWorkbook Workbook = new XSSFWorkbook();

		public abstract Task<IWorkbook> Generate(ICacheService cacheService);
	}
}
