using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Bitbird.Core.Api.AzureReporting.Net.Models;
using Bitbird.Core.Api.AzureReporting.Net.Tests.Properties;
using Newtonsoft.Json;

namespace Bitbird.Core.Api.AzureReporting.Net.Tests
{
    [TestClass]
    public class TicketLoggingTests
    {
        [TestMethod]
        public async Task QueryTicketAsyncTest()
        {
            var tickets = await TicketLogging.QueryTicketsAsync();
            Console.WriteLine(JsonConvert.SerializeObject(tickets, Formatting.Indented));
        }


        [TestMethod]
        public async Task LogTicketAndAddAttachmentsAsyncTest()
        {
            var data = await TicketLogging.CreateTicketAsync(new CreateBugTicketModel
            {
                Priority = Priority.Unimportant,
                ReproductionSteps = "Steps to reproduce something..",
                Severity = Severity.Low,
                SystemInfo = "Some kind of application",
                Title = "[UNIT TEST SIDEEFFECT] Some title"
            });
            Console.WriteLine(JsonConvert.SerializeObject(data,Formatting.Indented));

            var created = await TicketLogging.QueryTicketAsync(data.Id);
            Console.WriteLine(JsonConvert.SerializeObject(data, Formatting.Indented));

            await TicketLogging.AttachFileToTicketAsync(data.Id, new IUploadAttachmentModel[]
            {
                new UploadAttachmentModel
                {
                    Filename = "test1.txt",
                    GetStream = () => new MemoryStream(Encoding.UTF8.GetBytes("Das ist ein Text."))
                },
                new UploadAttachmentModel
                {
                    Filename = "TestArchieve.zip",
                    GetStream = () => new MemoryStream(Resources.TestArchieve)
                }
            });

            data = await TicketLogging.CreateTicketAsync(new CreateFeatureTicketModel
            {
                Priority = Priority.Unimportant,
                SystemInfo = "Some kind of application",
                Title = "[UNIT TEST SIDEEFFECT] Some title",
                Description = "Some description"
            });
            Console.WriteLine(JsonConvert.SerializeObject(data, Formatting.Indented));

            await TicketLogging.AttachFileToTicketAsync(data.Id, new IUploadAttachmentModel[]
            {
                new UploadAttachmentModel
                {
                    Filename = "test1.txt",
                    GetStream = () => new MemoryStream(Encoding.UTF8.GetBytes("Das ist ein Text."))
                },
                new UploadAttachmentModel
                {
                    Filename = "TestArchieve.zip",
                    GetStream = () => new MemoryStream(Resources.TestArchieve)
                }
            });

            data = await TicketLogging.CreateTicketAsync(new CreateUserStoryTicketModel
            {
                Priority = Priority.Unimportant,
                SystemInfo = "Some kind of application",
                Title = "[UNIT TEST SIDEEFFECT] Some title",
                Description = "Some description",
                AcceptanceCriteria = "Some acceptance criteria"
            });
            Console.WriteLine(JsonConvert.SerializeObject(data, Formatting.Indented));

            await TicketLogging.AttachFileToTicketAsync(data.Id, new IUploadAttachmentModel[]
            {
                new UploadAttachmentModel
                {
                    Filename = "test1.txt",
                    GetStream = () => new MemoryStream(Encoding.UTF8.GetBytes("Das ist ein Text."))
                },
                new UploadAttachmentModel
                {
                    Filename = "TestArchieve.zip",
                    GetStream = () => new MemoryStream(Resources.TestArchieve)
                }
            });
        }
    }
}