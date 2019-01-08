using Bitbird.Core.Expressions;
using Bitbird.Core.Json.Extensions;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Bitbird.Core.Export.Xlsx
{
    public class XlsxExport
    {
        public XlsxColumn[] Columns { get; set; }
    }

    public class XlsxColumn
    {
        public string Property { get; set; }
        public string Caption { get; set; }
    }

    public static class XlsxExportHelper
    {
        public static object[][] CreateTableData<T>(XlsxExport export, T[] data)
        {
            var compiledColumns = export.Columns.Select((c, idx) =>
            {
                Func<T, object> accessor;
                try
                {
                    accessor = PropertiesHelper.GetDottedPropertyGetter<T>(c.Property);
                }
                catch (Exception e)
                {
                    throw new CreateTableException($"{nameof(XlsxExport.Columns).FromCamelCaseToJsonCamelCase()}/{idx}/{nameof(XlsxColumn.Property).FromCamelCaseToJsonCamelCase()}", $"Could not access path '{c.Property}'.",e);
                }
                return new
                {
                    Column = c,
                    Accessor = accessor
                };
            }).ToArray();

            var tableData = new object[1 + data.Length][];

            tableData[0] = new object[compiledColumns.Length];
            for (var colIdx = 0; colIdx < compiledColumns.Length; colIdx++)
                tableData[0][colIdx] = compiledColumns[colIdx].Column.Caption;

            for (var rowIdx = 0; rowIdx < data.Length; rowIdx++)
            {
                tableData[1 + rowIdx] = new object[compiledColumns.Length];
                for (var colIdx = 0; colIdx < compiledColumns.Length; colIdx++)
                    tableData[1 + rowIdx][colIdx] = compiledColumns[colIdx].Accessor(data[rowIdx]);
            }

            return tableData;
        }

        public static void AddToXlsx(XLWorkbook workbook, string title, object[][] tableData)
        {
            var worksheet = workbook.Worksheets.Add(title);
            var range = worksheet.Cell(1, 1).InsertData(tableData);
            range.AddToNamed(title, XLScope.Workbook);
            worksheet.Columns().AdjustToContents();
        }

        public static XLWorkbook CreateXlsx(string title, object[][] tableData)
        {
            var workbook = new XLWorkbook();
            try
            {
                AddToXlsx(workbook, title, tableData);
            }
            catch
            {
                try { workbook.Dispose(); } catch { /* ignored */ }
                throw;
            }
            return workbook;
        }

        public static MemoryStream CreateXlsxToMemoryStream(MemoryStream memoryStream, string title, object[][] tableData)
        {
            using (var workbook = CreateXlsx(title, tableData))
            {
                workbook.SaveAs(memoryStream);
            }

            return memoryStream;
        }

        public static MemoryStream CreateXlsxToMemoryStream(string title, object[][] tableData)
        {
            var memoryStream = new MemoryStream();
            try
            {
                CreateXlsxToMemoryStream(memoryStream, title, tableData);
            }
            catch
            {
                try { memoryStream.Dispose(); } catch { /* ignored */ }
                throw;
            }
            return memoryStream;
        }

        public static XLWorkbook ToXlsx<T>(this IEnumerable<T> data, XlsxExport export, string title)
        {
            var tableData = CreateTableData(export, data.ToArray());
            return CreateXlsx(title, tableData);
        }
    }
}
