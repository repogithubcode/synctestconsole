using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using ClosedXML;
using ClosedXML.Excel;
using System.IO;

namespace ProEstimator.Business.Logic
{
    public class SpreadsheetWriter
    {

        public string WriteSpreadshet(DataTable dataTable, string diskPath)
        {
            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet1");
            string filename = string.Empty;
            int excelFirstRowIndex = 0;

            List<string> columnLetters = GetColumnLetters();

            // put column name in first row
            for (int columnIndex = 0; columnIndex < dataTable.Columns.Count; columnIndex++)
            {
                string cellName = columnLetters[columnIndex] + (excelFirstRowIndex + 1).ToString();
                worksheet.Cell(cellName).Value = dataTable.Columns[columnIndex].ColumnName;
            }

            // start putting values in next row
            excelFirstRowIndex = excelFirstRowIndex + 1;
            for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < dataTable.Columns.Count; columnIndex++)
                {
                    string cellName = columnLetters[columnIndex] + (excelFirstRowIndex + rowIndex + 1).ToString();
                    worksheet.Cell(cellName).Value = dataTable.Rows[rowIndex][columnIndex];
                }
            }

            // Make sure the folder exists
            string folderPath = Path.GetDirectoryName(diskPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            workbook.SaveAs(diskPath);

            filename = Path.GetFileName(diskPath);

            return filename;
        }

        public string WriteSpreadshet(DataTable[] dataTableArr, string diskPath)
        {
            XLWorkbook workbook = new XLWorkbook();

            string filename = string.Empty;

            foreach (DataTable dataTable in dataTableArr)
            {
                var worksheet = workbook.Worksheets.Add(dataTable.TableName);
                int excelFirstRowIndex = 0;

                List<string> columnLetters = GetColumnLetters();

                // put column name in first row
                for (int columnIndex = 0; columnIndex < dataTable.Columns.Count; columnIndex++)
                {
                    string cellName = columnLetters[columnIndex] + (excelFirstRowIndex + 1).ToString();
                    worksheet.Cell(cellName).Value = dataTable.Columns[columnIndex].ColumnName;
                }

                // start putting values in next row
                excelFirstRowIndex = excelFirstRowIndex + 1;
                for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
                {
                    for (int columnIndex = 0; columnIndex < dataTable.Columns.Count; columnIndex++)
                    {
                        string cellName = columnLetters[columnIndex] + (excelFirstRowIndex + rowIndex + 1).ToString();
                        worksheet.Cell(cellName).Value = dataTable.Rows[rowIndex][columnIndex];
                    }
                }
            }

            // Make sure the folder exists
            string folderPath = Path.GetDirectoryName(diskPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            workbook.SaveAs(diskPath);

            filename = Path.GetFileName(diskPath);

            return filename;
        }

        private List<string> GetColumnLetters()
        {
            List<string> columnLetters = new List<string>();

            for (int i = 65; i <= 90; i++)
            {
                columnLetters.Add(((char)i).ToString());
            }

            for (int i = 65; i <= 90; i++)
            {
                for (int j = 65; j <= 90; j++)
                {
                    columnLetters.Add(((char)i).ToString() + ((char)j).ToString());
                }
            }

            return columnLetters;
        }

    }
}
