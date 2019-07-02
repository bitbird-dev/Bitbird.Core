namespace Bitbird.Core.Web.JsonApi.Models
{
    public class JsonApiErrorSource
    {
        /// <summary>
        /// a JSON Pointer [RFC6901] to the associated entity in the request document [e.g. "/data" for a primary data object, or "/data/attributes/title" for a specific attribute].
        /// </summary>
        public string Pointer { get; set; }

        /// <summary>
        /// a string indicating which URI query parameter caused the error.
        /// </summary>
        public string Parameter { get; set; }
    }
}