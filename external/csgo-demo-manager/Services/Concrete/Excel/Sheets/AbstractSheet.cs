﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NPOI.SS.UserModel;

namespace Services.Concrete.Excel.Sheets
{
	public abstract class AbstractSheet
	{
		protected ISheet Sheet;

		protected Dictionary<string, CellType> Headers;

		public abstract Task GenerateContent();

		protected async Task GenerateHeaders()
		{
			await Task.Factory.StartNew(() =>
			{
				IRow row = Sheet.CreateRow(0);
				int i = 0;
				foreach (KeyValuePair<string, CellType> pair in Headers)
				{
					ICell cell = row.CreateCell(i);
					cell.SetCellType(pair.Value);
					cell.SetCellValue(pair.Key);
					i++;
				}
			});
		}

		public async Task Generate()
		{
			await GenerateHeaders();
			await GenerateContent();
		}

		public void SetCellValue(IRow row, int index, CellType cellType, dynamic value)
		{
			if (value == null) value = string.Empty;
			if (value is string)
			{
				value = Convert.ChangeType(value, TypeCode.String);
			}
			else
			{
				value = Convert.ChangeType(value, TypeCode.Double);
			}

			ICell cell = row.CreateCell(index);
			cell.SetCellType(cellType);
			cell.SetCellValue(value);
		}

		/// <summary>
		/// Fill empty cells with a numeric value 0
		/// </summary>
		/// <param name="rowCount"></param>
		/// <param name="columnCount"></param>
		protected void FillEmptyCells(int rowCount, int columnCount)
		{
			for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
			{
				for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
				{
					IRow row = Sheet.GetRow(rowIndex);
					ICell cell = row.GetCell(columnIndex);
					if (cell == null) SetCellValue(row, columnIndex, CellType.Numeric, 0);
				}
			}
		}
	}
}