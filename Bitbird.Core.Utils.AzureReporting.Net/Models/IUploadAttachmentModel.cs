using System;
using System.IO;

namespace Bitbird.Core.Api.AzureReporting.Net.Models
{
    public interface IUploadAttachmentModel
    {
        string Filename { get; set; }
        Func<Stream> GetStream { get; set; }
    }
}