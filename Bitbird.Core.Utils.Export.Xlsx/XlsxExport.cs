using Bitbird.Core.Data.Validation;
using JetBrains.Annotations;

namespace Bitbird.Core.Utils.Export.Xlsx
{
    public class XlsxExport
    {
        [ValidatorCheckRecursive]
        [ValidatorCheckNotNull]
        [UsedImplicitly, CanBeNull]
        public XlsxColumn[] Columns { get; set; }
    }
}