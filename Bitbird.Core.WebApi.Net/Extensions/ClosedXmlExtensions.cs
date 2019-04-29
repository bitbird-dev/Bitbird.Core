using System;
using System.Collections.Generic;
using System.Net.Http;
using Bitbird.Core;
using Bitbird.Core.Export.Xlsx;
using ClosedXML.Extensions;

namespace Bitbird.Core.WebApi.Net.Extensions
{
    /// <summary>
    /// Provides extension methods to help with Excel conversion.
    /// Uses <see cref="ClosedXML"/> for conversion.
    /// </summary>
    public static class ClosedXmlExtensions
    {
        /// <summary>
        /// Converts the collection to an Xlsx-file and returns a <see cref="HttpResponseMessage"/> that delivers the file.
        /// </summary>
        /// <typeparam name="T">The type of model that is contained in the collection</typeparam>
        /// <param name="data">A collection that should be converted.</param>
        /// <param name="export">Meta-data for the generation of the Xlsx-file. See <see cref="XlsxExport"/>.</param>
        /// <param name="title">The title used for the xlsx export (represented in table-/range-names and the file name).</param>
        /// <returns>A <see cref="HttpResponseMessage"/> containing a file result that contains the converted Xlsx-file.</returns>
        public static HttpResponseMessage DeliverAsXlsxAsync<T>(this IEnumerable<T> data, XlsxExport export, string title)
        {
            try
            {
                return data.ToXlsx(export, title).Deliver($"Export_{title}_{DateTime.Now:yyyy-MM-ddTHH-mm-ss}.xlsx");
            }
            catch(CreateTableException e)
            {
                throw new ApiErrorException(e, new ApiParameterError(e.ParameterPath, e.Message));
            }
        }
    }
}