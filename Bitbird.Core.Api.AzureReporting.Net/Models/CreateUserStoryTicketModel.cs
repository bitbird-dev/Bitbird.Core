using System.Collections.Generic;
using System.Linq;
using Bitbird.Core.Api.AzureReporting.Net.Core;

namespace Bitbird.Core.Api.AzureReporting.Net.Models
{
    public class CreateUserStoryTicketModel : CreateTicketModelBase
    {
        /// <summary>
        /// The description of the user story. Can be null.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// A acceptance criteria for the user story. Can be null.
        /// </summary>
        public string AcceptanceCriteria { get; set; }

        /// <inheritdoc />
        public override IEnumerable<PatchOperation> ToPatchOperations()
        {
            return base.ToPatchOperations()
                .Concat(new[]
                {
                    new PatchOperation("add", "/fields/System.Description", Description ?? string.Empty),
                    new PatchOperation("add", "/fields/Microsoft.VSTS.Common.AcceptanceCriteria", AcceptanceCriteria ?? string.Empty)
                });
        }

        public override string GetUriPath() => "/$User%20Story";
    }
}