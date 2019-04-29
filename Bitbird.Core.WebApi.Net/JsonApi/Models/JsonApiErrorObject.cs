namespace Bitbird.Core.WebApi.Net.JsonApi.Models
{
    public class JsonApiErrorObject
    {
        /// <summary>
        /// a unique identifier for this particular occurrence of the problem.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// a links object containing the following members:
        /// </summary>
        public JsonApiErrorLinks Links { get; set; }

        /// <summary>
        /// the HTTP status code applicable to this problem, expressed as a string value.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// an application-specific error code, expressed as a string value.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// a short, human-readable summary of the problem that SHOULD NOT change from occurrence to occurrence of the problem, except for purposes of localization.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// a human-readable explanation specific to this occurrence of the problem. Like title, this field’s value can be localized.
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// an object containing references to the source of the error, optionally including any of the following members:
        /// </summary>
        public JsonApiErrorSource Source { get; set; }
    }
}