using Bitbird.Core.Data.Validation;
using JetBrains.Annotations;

namespace Bitbird.Core.Utils.Export.Xlsx
{
    public class XlsxColumn
    {
        [ValidatorCheckNotNullOrEmpty, ValidatorCheckTrimmed]
        [CanBeNull, UsedImplicitly]
        public string Property { get; set; }

        [CanBeNull, UsedImplicitly]
        public string Caption { get; set; }
    }
}