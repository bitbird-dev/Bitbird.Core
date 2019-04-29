using System.Collections.Generic;
using System.Linq;
using Bitbird.Core.Api.AzureReporting.Net.Core;

namespace Bitbird.Core.Api.AzureReporting.Net.Models
{
    public class CreateBugTicketModel : CreateTicketModelBase
    {
        /// <summary>
        /// A description of steps to reproduce the problem. Can be null.
        /// </summary>
        public string ReproductionSteps { get; set; }
        /// <summary>
        /// The severity of the problem.
        /// </summary>
        public Severity Severity { get; set; }

        /// <inheritdoc />
        public override IEnumerable<PatchOperation> ToPatchOperations()
        {
            return base.ToPatchOperations()
                .Concat(new[]
                {
                    new PatchOperation("add", "/fields/Microsoft.VSTS.TCM.ReproSteps", ReproductionSteps ?? string.Empty),
                    new PatchOperation("add", "/fields/Microsoft.VSTS.Common.Severity", Severity.FormatForAzure())
                });
        }

        public override string GetUriPath() => "/$Bug";
    }
}