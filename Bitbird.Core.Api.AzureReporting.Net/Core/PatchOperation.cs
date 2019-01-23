namespace Bitbird.Core.Api.AzureReporting.Net.Core
{
    /// <summary>
    /// Model-class for the communication with the Azure DevOps REST interface.
    /// Describes an operation that is to be executed.
    /// See Azure DevOps REST interface documentation.
    /// </summary>
    public class PatchOperation
    {
        /// <summary>
        /// The operation to be executed. i.e. "add". See Azure DevOps REST interface documentation.
        /// </summary>
        public string Op { get; set; }
        /// <summary>
        /// The Path on which to execute the operation. i.e. "/fields/System.Title". See Azure DevOps REST interface documentation.
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// The Value that is used to execute the operation. i.e. "My CreateTicketModel". See Azure DevOps REST interface documentation.
        /// </summary>
        public object Value { get; set; }

        public PatchOperation() { }
        public PatchOperation(string op, string path, object value)
        {
            Op = op;
            Path = path;
            Value = value;
        }
    }
}