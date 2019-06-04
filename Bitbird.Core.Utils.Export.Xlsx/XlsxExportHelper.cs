using System;
using System.IO;
using System.Linq;
using Bitbird.Core.Expressions;
using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Utils.Export.Xlsx.Exceptions;
using ClosedXML.Excel;
using JetBrains.Annotations;

namespace Bitbird.Core.Utils.Export.Xlsx
{
    public static class XlsxExportHelper
    {
        [UsedImplicitly, NotNull, ItemNotNull]
        public static object[][] CreateTableData<T>(
            [NotNull] XlsxExport export,
            [NotNull, ItemNotNull] T[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Any(x => x == null)) throw new ArgumentNullException(nameof(data));
            if (export == null) throw new ArgumentNullException(nameof(export));
            if (export.Columns == null) throw new ArgumentNullException(nameof(export));
            if (export.Columns.Any(x => x == null)) throw new ArgumentNullException(nameof(export));
            if (export.Columns.Any(x => string.IsNullOrWhiteSpace(x.Property) || x.Property.Trim().Length != x.Property.Length)) throw new ArgumentNullException(nameof(export));

            var compiledColumns = export.Columns.Select((c, idx) =>
            {
                Func<T, object> accessor;
                try
                {
                    accessor = PropertiesHelper.GetDottedPropertyGetter<T, object>(c.Property);
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

        [UsedImplicitly]
        public static void AddToXlsx(
            [NotNull] XLWorkbook workbook, 
            [NotNull] string title, 
            [NotNull, ItemNotNull] object[][] tableData)
        {
            if (workbook == null) throw new ArgumentNullException(nameof(workbook));
            if (title == null) throw new ArgumentNullException(nameof(title));
            if (tableData == null) throw new ArgumentNullException(nameof(tableData));
            if (tableData.Any(x => x == null)) throw new ArgumentNullException(nameof(tableData));

            var worksheet = workbook.Worksheets.Add(title);
            var range = worksheet.Cell(1, 1).InsertData(tableData);
            range.AddToNamed(title, XLScope.Workbook);
            worksheet.Columns().AdjustToContents();
        }

        [NotNull, UsedImplicitly]
        public static XLWorkbook CreateXlsx(
            [NotNull] string title, 
            [NotNull, ItemNotNull] object[][] tableData)
        {
            if (title == null) throw new ArgumentNullException(nameof(title));
            if (tableData == null) throw new ArgumentNullException(nameof(tableData));
            if (tableData.Any(x => x == null)) throw new ArgumentNullException(nameof(tableData));

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

        [NotNull, UsedImplicitly]
        public static MemoryStream CreateXlsxToMemoryStream(
            [NotNull] MemoryStream memoryStream, 
            [NotNull] string title, 
            [NotNull, ItemNotNull] object[][] tableData)
        {
            if (memoryStream == null) throw new ArgumentNullException(nameof(memoryStream));
            if (title == null) throw new ArgumentNullException(nameof(title));
            if (tableData == null) throw new ArgumentNullException(nameof(tableData));
            if (tableData.Any(x => x == null)) throw new ArgumentNullException(nameof(tableData));

            using (var workbook = CreateXlsx(title, tableData))
            {
                workbook.SaveAs(memoryStream);
            }

            return memoryStream;
        }

        [NotNull, UsedImplicitly]
        public static MemoryStream CreateXlsxToMemoryStream(
            [NotNull] string title,
            [NotNull, ItemNotNull] object[][] tableData)
        {
            if (title == null) throw new ArgumentNullException(nameof(title));
            if (tableData == null) throw new ArgumentNullException(nameof(tableData));
            if (tableData.Any(x => x == null)) throw new ArgumentNullException(nameof(tableData));

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

        [NotNull]
        public static XLWorkbook ToXlsx<T>(
            [NotNull, ItemNotNull] this T[] data, 
            [NotNull] XlsxExport export, 
            [NotNull] string title)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Any(x => x == null)) throw new ArgumentNullException(nameof(data));
            if (export == null) throw new ArgumentNullException(nameof(export));
            if (export.Columns == null) throw new ArgumentNullException(nameof(export));
            if (export.Columns.Any(x => x == null)) throw new ArgumentNullException(nameof(export));
            if (export.Columns.Any(x => string.IsNullOrWhiteSpace(x.Property) || x.Property.Trim().Length != x.Property.Length)) throw new ArgumentNullException(nameof(export));
            if (title == null) throw new ArgumentNullException(nameof(title));

            var tableData = CreateTableData(export, data.ToArray());
            return CreateXlsx(title, tableData);
        }
    }
}
