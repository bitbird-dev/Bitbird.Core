using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Bitbird.Core.Api.AzureReporting.Net.Models;
using Bitbird.Core.Api.AzureReporting.Net.Tests.Properties;
using Microsoft.Azure;
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
        public async Task LogTicketAndAddAttachmentsAndDeleteTicketsAsyncTest()
        {
            if (string.IsNullOrWhiteSpace(CloudConfigurationManager.GetSetting("AzureReporting.AccessToken")))
            {
                Console.WriteLine("TEST NOT EXECUTED. Access token not specified!");
                return;
            }

            var ids = new List<long>();

            try
            {
                var data = await TicketLogging.CreateTicketAsync(new CreateBugTicketModel
                {
                    Priority = Priority.Unimportant,
                    ReproductionSteps = "Steps to reproduce something..",
                    Severity = Severity.Low,
                    SystemInfo = "Some kind of application",
                    Title = "[UNIT TEST SIDEEFFECT] Some title"
                });
                ids.Add(data.Id);
                Console.WriteLine(JsonConvert.SerializeObject(data, Formatting.Indented));

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
                ids.Add(data.Id);
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
                ids.Add(data.Id);
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

                while (ids.Any())
                {
                    var id = ids[0];

                    Console.WriteLine(await TicketLogging.DeleteTicketAsync(id));

                    ids.RemoveAt(0);
                }
            }
            catch
            {
                foreach (var id in ids)
                {
                    try{ await TicketLogging.DeleteTicketAsync(id); } catch { /* ignored */ }
                }

                throw;
            }
        }
    }
}