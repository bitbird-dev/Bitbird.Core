using System;
using System.Collections.Generic;
using Bitbird.Core.Api.AzureReporting.Net.Core;

namespace Bitbird.Core.Api.AzureReporting.Net.Models
{
    public abstract class CreateTicketModelBase : ICreateTicketModel
    {
        /// <summary>
        /// The ticket's title. Cannot be null.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The priority of the ticket. Cannot be null.
        /// </summary>
        public Priority Priority { get; set; }
        /// <summary>
        /// A description of the system that creates the ticket. Can be null.
        /// </summary>
        public string SystemInfo { get; set; }

        /// <summary>
        /// Creates a list of PatchOperation-instances that can be used to create this ticket.
        /// </summary>
        /// <returns>A list of PatchOperation-instances. Not null. No entry is null.</returns>
        public virtual IEnumerable<PatchOperation> ToPatchOperations()
        {
            return new[]
            {
                new PatchOperation("add", "/fields/System.Title", Title ?? throw new Exception("Ticket titles cannot be empty.")),
                new PatchOperation("add", "/fields/Microsoft.VSTS.Common.Priority", Priority.FormatForAzure()),
                new PatchOperation("add", "/fields/Microsoft.VSTS.TCM.SystemInfo", SystemInfo ?? string.Empty)
            };
        }

        /// <summary>
        /// Appends additional information to SystemInfo.
        /// </summary>
        /// <param name="info">Additional information</param>
        public void AppendSystemInfo(string info)
        {
            SystemInfo = $"{(string.IsNullOrWhiteSpace(SystemInfo) ? string.Empty : $"{SystemInfo}{Environment.NewLine}")}{info ?? throw new ArgumentNullException(nameof(info))}";
        }

        /// <summary>
        /// Returns the Uri-Path for this ticket. i.e. "/$CreateBugTicketModel".
        /// </summary>
        /// <returns>The Uri-Path for this ticket.</returns>
        public abstract string GetUriPath();
    }
}