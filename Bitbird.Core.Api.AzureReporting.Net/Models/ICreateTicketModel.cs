using System.Collections.Generic;
using Bitbird.Core.Api.AzureReporting.Net.Core;

namespace Bitbird.Core.Api.AzureReporting.Net.Models
{
    public interface ICreateTicketModel
    {
        string GetUriPath();
        IEnumerable<PatchOperation> ToPatchOperations();
    }
}