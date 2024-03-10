using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace ProEstimatorData
{
    public class ExcelReaderInterop
    {
        Application _excelApp;
        System.Data.DataTable l_ExcelDT = null;

        public ExcelReaderInterop()
        {
            _excelApp = new Application();
        }

        public System.Data.DataTable ExcelOpenSpreadsheets(string thisFileName, string partsCompanyName, Dictionary<string, string> columnDictionary = null,
                                                        Dictionary<string, Dictionary<string, string>> nestedColumnDictionary = null)
        {
            try
            {
                Workbook workBook = _excelApp.Workbooks.Open(thisFileName,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing);

                ExcelScanIntenal(workBook, partsCompanyName, columnDictionary, nestedColumnDictionary);
                workBook.Close(false, thisFileName, null);
                Marshal.ReleaseComObject(workBook);
            }
            catch
            {

            }

            return l_ExcelDT;
        }

        private void ExcelScanIntenal(Workbook workBookIn, string partsCompanyName, Dictionary<string, string> columnDictionary = null,
                                                        Dictionary<string, Dictionary<string, string>> nestedColumnDictionary = null)
        {
            int numSheets = workBookIn.Sheets.Count;
            l_ExcelDT = new System.Data.DataTable();
            for (int sheetNum = 1; sheetNum < numSheets + 1; sheetNum++)
            {
                Worksheet sheet = (Worksheet)workBookIn.Sheets[sheetNum];

                Range excelRange = sheet.UsedRange;
                object[,] valueArray = (object[,])excelRange.get_Value(
                    XlRangeValueDataType.xlRangeValueDefault);

                GetDataFromExcelValueArry(valueArray, partsCompanyName, columnDictionary, nestedColumnDictionary);
                break;
            }
        }

        private void GetDataFromExcelValueArry(object[,] valueArray, string partsCompanyName, Dictionary<string, string> columnDictionary = null,
                                                                Dictionary<string, Dictionary<string, string>> nestedColumnDictionary = null)
        {
            DataColumn l_DataColumn = null;
            DataRow l_DataRow = null;
            int initialExcelRowIndex = 0;

            // Get columns in data table
            int l_NumOfColumns = valueArray.GetUpperBound(1);

            if ((string.Compare("Car Parts PDX", partsCompanyName, true) == 0) || (string.Compare("KSI Trading", partsCompanyName, true) == 0))
            {
                if (columnDictionary == null) // "Car Parts PDX"
                {
                    for (int l_Index = 1; l_Index <= l_NumOfColumns; l_Index++)
                    {
                        l_DataColumn = new DataColumn();

                        l_DataColumn.ColumnName = Convert.ToString(valueArray[1, l_Index]);

                        if (string.Compare("Std Price", l_DataColumn.ColumnName, true) == 0)
                        {
                            l_DataColumn.DataType = typeof(System.Decimal);
                            l_ExcelDT.Columns.Add(l_DataColumn);
                        }
                        else
                            l_ExcelDT.Columns.Add(l_DataColumn);
                    }
                    initialExcelRowIndex = 2;
                }
                else // "KSI Trading"
                {
                    foreach (KeyValuePair<string, string> eachKeyValuePair in columnDictionary)
                    {
                        l_DataColumn = new DataColumn();
                        l_DataColumn.ColumnName = eachKeyValuePair.Key;

                        if (string.Compare(eachKeyValuePair.Value, "Decimal", true) == 0)
                            l_DataColumn.DataType = typeof(System.Decimal);

                        l_ExcelDT.Columns.Add(l_DataColumn);
                    }
                    initialExcelRowIndex = 1;
                }
            }
            else if (string.Compare("Keystone", partsCompanyName, true) == 0) // "Keystone"
            {
                if (nestedColumnDictionary != null) // "Car Parts PDX"
                {
                    for (int l_Index = 1; l_Index <= l_NumOfColumns; l_Index++)
                    {
                        l_DataColumn = new DataColumn();

                        l_DataColumn.ColumnName =  Convert.ToString(valueArray[1, l_Index]);

                        Dictionary<string, string> dictionaryObj = null;
                        Boolean tryGetValue = nestedColumnDictionary.TryGetValue(l_DataColumn.ColumnName, out dictionaryObj);
                        
                        if (tryGetValue)
                        {
                            if (dictionaryObj != null)
                            {
                                foreach (KeyValuePair<string,string> eachKeyValuePair in dictionaryObj)
	                            {
                                    l_DataColumn.ColumnName = eachKeyValuePair.Key;
                                    break;
	                            }
                            }
                        }

                        if (string.Compare("Std Price", l_DataColumn.ColumnName, true) == 0)
                        {
                            l_DataColumn.DataType = typeof(System.Decimal);
                            l_ExcelDT.Columns.Add(l_DataColumn);
                        }
                        else
                            l_ExcelDT.Columns.Add(l_DataColumn);
                    }

                    initialExcelRowIndex = 2;
                }
            }

            // Get columns in data table
            int l_NumOfRows = valueArray.GetUpperBound(0);
            string cellValue = string.Empty;
            for (int l_ExcelRowIndex = initialExcelRowIndex; l_ExcelRowIndex <= l_NumOfRows; l_ExcelRowIndex++)
            {
                try
                {
                    l_DataRow = l_ExcelDT.NewRow();

                    int l_ExcelColumnIndexInner = 1;

                    for (int l_ExcelColumnIndex = 1; l_ExcelColumnIndex <= l_NumOfColumns; l_ExcelColumnIndex++)
                    {
                        cellValue = Convert.ToString(valueArray[l_ExcelRowIndex, l_ExcelColumnIndexInner]);

                        if (l_ExcelDT.Columns[l_ExcelColumnIndexInner - 1].DataType == typeof(System.Decimal))
                        {
                            if(string.IsNullOrEmpty(cellValue))
                            {
                                cellValue= "0.00";
                            }
                        }

                        l_DataRow[l_ExcelColumnIndexInner - 1] = cellValue;

                        l_ExcelColumnIndexInner = l_ExcelColumnIndexInner + 1;
                    }

                    l_ExcelDT.Rows.Add(l_DataRow);
                }
                catch (Exception ex)
                {
                    
                  
                }

            }
        }
    }
}
