using System;
using System.IO;

namespace Bitbird.Core.Api.AzureReporting.Net.Models
{
    public class UploadAttachmentModel : IUploadAttachmentModel
    {
        public string Filename { get; set; }
        public Func<Stream> GetStream { get; set; }
    }
}