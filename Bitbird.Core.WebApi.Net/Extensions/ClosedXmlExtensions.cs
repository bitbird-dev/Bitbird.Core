using System;
using System.Linq;
using System.Net.Http;
using Bitbird.Core.Data.Validation;
using Bitbird.Core.Utils.Export.Xlsx;
using Bitbird.Core.Utils.Export.Xlsx.Exceptions;
using ClosedXML.Extensions;
using JetBrains.Annotations;

namespace Bitbird.Core.WebApi.Extensions
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
        public static HttpResponseMessage DeliverAsXlsxAsync<T>(
            [NotNull, ItemNotNull] this T[] data, 
            [NotNull] XlsxExport export, 
            [NotNull] string title)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Any(x => x == null)) throw new ArgumentNullException(nameof(data));
            if (export == null) throw new ArgumentNullException(nameof(export));
            if (title == null) throw new ArgumentNullException(nameof(title));

            var validator = new Validator();
            ModelValidators.GetValidator<XlsxExport>().Validate(export, validator);
            validator.ThrowIfHasErrors();

            try
            {
                return data.ToXlsx(export, title).Deliver($"Export_{title}_{DateTime.Now:yyyy-MM-ddTHH-mm-ss}.xlsx");
            }
            catch (CreateTableException e)
            {
                throw new ApiErrorException(e, new ApiParameterError(e.ParameterPath, e.Message));
            }
        }
    }
}