using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Bitbird.Core.Data.Validation;
using Bitbird.Core.Utils.Export.Xlsx;
using Bitbird.Core.Utils.Export.Xlsx.Exceptions;
using JetBrains.Annotations;

namespace Bitbird.Core.Web.Extensions
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
        /// <param name="validator">A validator that can be used to validate parameters and constraints.</param>
        /// <returns>A <see cref="HttpResponseMessage"/> containing a file result that contains the converted Xlsx-file.</returns>
        public static HttpResponseMessage DeliverAsXlsxAsync<T>(
            [NotNull, ItemNotNull] this T[] data, 
            [NotNull] XlsxExport export, 
            [NotNull] string title,
            [NotNull] ValidatorBase validator)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Any(x => x == null)) throw new ArgumentNullException(nameof(data));
            if (export == null) throw new ArgumentNullException(nameof(export));
            if (title == null) throw new ArgumentNullException(nameof(title));
            if (validator == null) throw new ArgumentNullException(nameof(validator));

            ModelValidators.GetValidator<XlsxExport>().Validate(export, validator);
            validator.ThrowIfHasErrors();

            try
            {
                var workbook = data.ToXlsx(export, title);

                var memoryStream = new MemoryStream();
                try
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    var message = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StreamContent(memoryStream)
                    };
                    message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                    message.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = $"Export_{title}_{DateTime.Now:yyyy-MM-ddTHH-mm-ss}.xlsx"
                    };

                    return message;
                }
                catch
                {
                    try { memoryStream.Dispose(); } catch { /* ignored */ }
                    throw;
                }
            }
            catch (CreateTableException e)
            {
                throw new ApiErrorException(e, new ApiParameterError(e.ParameterPath, e.Message));
            }
        }
    }
}